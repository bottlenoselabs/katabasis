// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static bottlenoselabs.FAudio;

namespace bottlenoselabs.Katabasis
{
	// http://msdn.microsoft.com/en-us/library/dd940262.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class AudioEngine : IDisposable
	{
		public const int ContentVersion = 46;

		// STOP LEAKING YOUR XACT DATA, GOOD GRIEF PEOPLE
		internal static bool ProgramExiting;

		private static readonly IntPtrComparer _comparer = new();

		// If this isn't static, destructors gets confused like idiots
		private static readonly Dictionary<IntPtr, WeakReference> _xactPointers = new(_comparer);
		internal readonly ushort _channels;

		internal readonly object _gcSync = new();

		internal readonly FACTAudioEngine* _handle;
		internal readonly F3DAUDIO_HANDLE _handle3D;

		private RendererDetail[]? _rendererDetails;

		private FACTNotificationDescription _notificationDesc;

		public AudioEngine(string settingsFile)
			: this(
				settingsFile,
				new TimeSpan(0, 0, 0, 0, (int)FACT_ENGINE_LOOKAHEAD_DEFAULT),
				string.Empty)
		{
		}

		public AudioEngine(string settingsFile, TimeSpan lookAheadTime, string rendererId)
		{
			if (string.IsNullOrEmpty(settingsFile))
			{
				throw new ArgumentNullException(nameof(settingsFile));
			}

			// Allocate (but don't initialize just yet!)
			FACTAudioEngine* handle;
			FACTCreateEngine(0, &handle);
			_handle = handle;

			// Grab RendererDetails
			ushort rendererCount;
			FACTAudioEngine_GetRendererCount(
				handle,
				&rendererCount);
			if (rendererCount == 0)
			{
				FACTAudioEngine_Release(handle);
				throw new NoAudioHardwareException();
			}

			_rendererDetails = new RendererDetail[rendererCount];
			byte[] converted = new byte[0xFF * sizeof(short)];
			for (ushort i = 0; i < rendererCount; i += 1)
			{
				FACTRendererDetails details;
				FACTAudioEngine_GetRendererDetails(
					handle,
					i,
					&details);
				Marshal.Copy((IntPtr) details._displayName, converted, 0, converted.Length);
				string name = System.Text.Encoding.Unicode.GetString(converted).TrimEnd('\0');
				Marshal.Copy((IntPtr) details._rendererID, converted, 0, converted.Length);
				string id = System.Text.Encoding.Unicode.GetString(converted).TrimEnd('\0');
				_rendererDetails[i] = new RendererDetail(name, id);
			}

			// Read entire file into memory, let FACT manage the pointer
			var buffer = TitleContainer.ReadToPointer(settingsFile, out var bufferLen);

			// Generate engine parameters
			var settings = default(FACTRuntimeParameters);
			settings.pGlobalSettingsBuffer = (void*)buffer;
			settings.globalSettingsBufferSize = (uint)bufferLen;
			settings.fnNotificationCallback.Data.Pointer = &OnXACTNotification;

			// Special parameters from constructor
			settings.lookAheadTime = (uint)lookAheadTime.Milliseconds;
			if (!string.IsNullOrEmpty(rendererId))
			{
				// FIXME: wchar_t? -flibit
				settings.pRendererID = (short*)Marshal.StringToHGlobalAuto(rendererId);
			}

			// Init engine, finally
			if (FACTAudioEngine_Initialize(_handle, &settings) != 0)
			{
				throw new InvalidOperationException("Engine initialization failed!");
			}

			// Free the settings strings
			if ((IntPtr)settings.pRendererID != IntPtr.Zero)
			{
				Marshal.FreeHGlobal((IntPtr)settings.pRendererID);
			}

			// Init 3D audio
			_handle3D.Data = (byte*)Marshal.AllocHGlobal(F3DAUDIO_HANDLE_BYTESIZE);
			FACT3DInitialize(_handle, _handle3D);

			// Grab channel count for DSP_SETTINGS
			FAudioWaveFormatExtensible mixFormat;
			FACTAudioEngine_GetFinalMixFormat(_handle, &mixFormat);
			_channels = mixFormat.Format.nChannels;

			// All XACT references have to go through here...
			var notificationDesc = default(FACTNotificationDescription);
			notificationDesc.flags = FACT_FLAG_NOTIFICATION_PERSIST;
			notificationDesc.type = FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED;
			FACTAudioEngine_RegisterNotification(handle, &notificationDesc);
			
			notificationDesc.type = FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED;
			FACTAudioEngine_RegisterNotification(handle, &notificationDesc);
			notificationDesc.type = FACTNOTIFICATIONTYPE_CUEDESTROYED;
			FACTAudioEngine_RegisterNotification(handle, &notificationDesc);

			_notificationDesc = notificationDesc;
		}

		public ReadOnlyCollection<RendererDetail> RendererDetails => new(_rendererDetails!);

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public event EventHandler<EventArgs>? Disposing;

		~AudioEngine() => Dispose(false);

		protected virtual void Dispose(bool disposing)
		{
			lock (_gcSync)
			{
				if (IsDisposed)
				{
					return;
				}

				Disposing?.Invoke(this, EventArgs.Empty);
				FACTAudioEngine_ShutDown(_handle);
				FACTAudioEngine_Release(_handle);
				_rendererDetails = null;
				IsDisposed = true;
			}
		}

		[UnmanagedCallersOnly]
		private static void OnXACTNotification(FACTNotification* notification)
		{
			WeakReference? reference;

			switch (notification->type)
			{
				case FACTNOTIFICATIONTYPE_WAVEDESTROYED:
				{
					var target = notification->waveBank.pWaveBank;
					lock (_xactPointers)
					{
						if (_xactPointers.TryGetValue((IntPtr)target, out reference))
						{
							if (reference.IsAlive)
							{
								(reference.Target as WaveBank)!.OnWaveBankDestroyed();
							}
						}

						_xactPointers.Remove((IntPtr)target);
					}

					break;
				}
				case FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED:
				{
					var target = notification->soundBank.pSoundBank;
					lock (_xactPointers)
					{
						if (_xactPointers.TryGetValue((IntPtr)target, out reference))
						{
							if (reference.IsAlive)
							{
								(reference.Target as SoundBank)!.OnSoundBankDestroyed();
							}
						}

						_xactPointers.Remove((IntPtr)target);
					}

					break;
				}
				case FACTNOTIFICATIONTYPE_CUEDESTROYED:
				{
					var target = notification->cue.pCue;
					lock (_xactPointers)
					{
						if (_xactPointers.TryGetValue((IntPtr)target, out reference))
						{
							if (reference.IsAlive)
							{
								(reference.Target as Cue)!.OnCueDestroyed();
							}
						}

						_xactPointers.Remove((IntPtr)target);
					}

					break;
				}
			}
		}

		public AudioCategory GetCategory(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var category = FACTAudioEngine_GetCategory(_handle, name);

			if (category == FACTCATEGORY_INVALID)
			{
				throw new InvalidOperationException("Invalid category name!");
			}

			return new AudioCategory(this, category, name);
		}

		public float GetGlobalVariable(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var variable = FACTAudioEngine_GetGlobalVariableIndex(_handle, name);

			if (variable == FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			float result;
			FACTAudioEngine_GetGlobalVariable(_handle, variable, &result);
			return result;
		}

		public void SetGlobalVariable(string name, float value)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var variable = FACTAudioEngine_GetGlobalVariableIndex(_handle, name);

			if (variable == FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			FACTAudioEngine_SetGlobalVariable(_handle, variable, value);
		}

		public void Update() => FACTAudioEngine_DoWork(_handle);

		internal void RegisterPointer(IntPtr ptr, WeakReference reference)
		{
			lock (_xactPointers)
			{
				_xactPointers.Add(ptr, reference);
			}
		}

		private class IntPtrComparer : IEqualityComparer<IntPtr>
		{
			public bool Equals(IntPtr x, IntPtr y) => x == y;

			public int GetHashCode(IntPtr obj) => obj.GetHashCode();
		}
	}
}
