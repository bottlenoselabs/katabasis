// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace Katabasis
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

		internal readonly IntPtr _handle;
		internal readonly _FAudio.F3DAUDIO_HANDLE _handle3D;

		private RendererDetail[]? _rendererDetails;

		private _FAudio.FACTNotificationDescription _notificationDesc;

		public AudioEngine(string settingsFile)
			: this(
				settingsFile,
				new TimeSpan(0, 0, 0, 0, (int)_FAudio.FACT_ENGINE_LOOKAHEAD_DEFAULT),
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
			_FAudio.FACTAudioEngine* handle;
			_FAudio.FACTCreateEngine(0, &handle);
			_handle = (IntPtr)handle;

			// Grab RendererDetails
			ushort rendererCount;
			_FAudio.FACTAudioEngine_GetRendererCount(
				handle,
				&rendererCount);
			if (rendererCount == 0)
			{
				_FAudio.FACTAudioEngine_Release(handle);
				throw new NoAudioHardwareException();
			}

			_rendererDetails = new RendererDetail[rendererCount];
			byte[] converted = new byte[0xFF * sizeof(short)];
			for (ushort i = 0; i < rendererCount; i += 1)
			{
				_FAudio.FACTRendererDetails details;
				_FAudio.FACTAudioEngine_GetRendererDetails(
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
			var settings = default(_FAudio.FACTRuntimeParameters);
			settings.pGlobalSettingsBuffer = (void*)buffer;
			settings.globalSettingsBufferSize = (uint)bufferLen;
			settings.fnNotificationCallback = new _FAudio.FACTNotificationCallback { Pointer = &OnXACTNotification };

			// Special parameters from constructor
			settings.lookAheadTime = (uint)lookAheadTime.Milliseconds;
			if (!string.IsNullOrEmpty(rendererId))
			{
				// FIXME: wchar_t? -flibit
				settings.pRendererID = (short*)Marshal.StringToHGlobalAuto(rendererId);
			}

			// Init engine, finally
			if (_FAudio.FACTAudioEngine_Initialize((_FAudio.FACTAudioEngine*)_handle, &settings) != 0)
			{
				throw new InvalidOperationException("Engine initialization failed!");
			}

			// Free the settings strings
			if ((IntPtr)settings.pRendererID != IntPtr.Zero)
			{
				Marshal.FreeHGlobal((IntPtr)settings.pRendererID);
			}

			// Init 3D audio
			_handle3D.Data = (byte*)Marshal.AllocHGlobal(_FAudio.F3DAUDIO_HANDLE_BYTESIZE);
			_FAudio.FACT3DInitialize((_FAudio.FACTAudioEngine*)_handle, _handle3D);

			// Grab channel count for DSP_SETTINGS
			_FAudio.FAudioWaveFormatExtensible mixFormat;
			_FAudio.FACTAudioEngine_GetFinalMixFormat((_FAudio.FACTAudioEngine*)_handle, &mixFormat);
			_channels = mixFormat.Format.nChannels;

			// All XACT references have to go through here...
			_notificationDesc = default;
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
				_FAudio.FACTAudioEngine_ShutDown((_FAudio.FACTAudioEngine*)_handle);
				_FAudio.FACTAudioEngine_Release((_FAudio.FACTAudioEngine*)_handle);
				_rendererDetails = null;
				IsDisposed = true;
			}
		}

		[UnmanagedCallersOnly]
		private static void OnXACTNotification(_FAudio.FACTNotification* notification)
		{
			WeakReference? reference;
			var not = (_FAudio.FACTNotification_FNA*)notification;
			switch (not->type)
			{
				case _FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED:
				{
					var target = not->anon.waveBank.pWaveBank;
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

				case _FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED:
				{
					var target = not->anon.soundBank.pSoundBank;
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

				case _FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED:
				{
					var target = not->anon.cue.pCue;
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

			var category = _FAudio.FACTAudioEngine_GetCategory((_FAudio.FACTAudioEngine*)_handle, name);

			if (category == _FAudio.FACTCATEGORY_INVALID)
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

			var variable = _FAudio.FACTAudioEngine_GetGlobalVariableIndex((_FAudio.FACTAudioEngine*)_handle, name);

			if (variable == _FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			float result;
			_FAudio.FACTAudioEngine_GetGlobalVariable((_FAudio.FACTAudioEngine*)_handle, variable, &result);
			return result;
		}

		public void SetGlobalVariable(string name, float value)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var variable = _FAudio.FACTAudioEngine_GetGlobalVariableIndex((_FAudio.FACTAudioEngine*)_handle, name);

			if (variable == _FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			_FAudio.FACTAudioEngine_SetGlobalVariable((_FAudio.FACTAudioEngine*)_handle, variable, value);
		}

		public void Update() => _FAudio.FACTAudioEngine_DoWork((_FAudio.FACTAudioEngine*)_handle);

		internal void RegisterWaveBank(IntPtr ptr, WeakReference reference)
		{
			_notificationDesc.type = _FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED;
			_notificationDesc.pWaveBank = (_FAudio.FACTWaveBank*)ptr;
			_FAudio.FACTAudioEngine_RegisterNotification((_FAudio.FACTAudioEngine*)_handle, (_FAudio.FACTNotificationDescription*)Unsafe.AsPointer(ref _notificationDesc));
			lock (_xactPointers)
			{
				_xactPointers.Add(ptr, reference);
			}
		}

		internal void RegisterSoundBank(IntPtr ptr, WeakReference reference)
		{
			_notificationDesc.type = _FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED;
			_notificationDesc.pSoundBank = (_FAudio.FACTSoundBank*)ptr;
			_FAudio.FACTAudioEngine_RegisterNotification((_FAudio.FACTAudioEngine*)_handle, (_FAudio.FACTNotificationDescription*)Unsafe.AsPointer(ref _notificationDesc));
			lock (_xactPointers)
			{
				_xactPointers.Add(ptr, reference);
			}
		}

		internal void RegisterCue(IntPtr ptr, WeakReference reference)
		{
			_notificationDesc.type = _FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED;
			_notificationDesc.pCue = (_FAudio.FACTCue*)ptr;
			_FAudio.FACTAudioEngine_RegisterNotification((_FAudio.FACTAudioEngine*)_handle, (_FAudio.FACTNotificationDescription*)Unsafe.AsPointer(ref _notificationDesc));
			lock (_xactPointers)
			{
				_xactPointers.Add(ptr, reference);
			}
		}

		internal void UnregisterWaveBank(IntPtr ptr)
		{
			lock (_xactPointers)
			{
				if (!_xactPointers.ContainsKey(ptr))
				{
					return;
				}

				_notificationDesc.type = _FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED;
				_notificationDesc.pWaveBank = (_FAudio.FACTWaveBank*)ptr;
				_FAudio.FACTAudioEngine_UnRegisterNotification((_FAudio.FACTAudioEngine*)_handle, (_FAudio.FACTNotificationDescription*)Unsafe.AsPointer(ref _notificationDesc));
				_xactPointers.Remove(ptr);
			}
		}

		internal void UnregisterSoundBank(IntPtr ptr)
		{
			lock (_xactPointers)
			{
				if (!_xactPointers.ContainsKey(ptr))
				{
					return;
				}

				_notificationDesc.type = _FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED;
				_notificationDesc.pSoundBank = (_FAudio.FACTSoundBank*)ptr;
				_FAudio.FACTAudioEngine_UnRegisterNotification((_FAudio.FACTAudioEngine*)_handle, (_FAudio.FACTNotificationDescription*)Unsafe.AsPointer(ref _notificationDesc));
				_xactPointers.Remove(ptr);
			}
		}

		internal void UnregisterCue(IntPtr ptr)
		{
			lock (_xactPointers)
			{
				if (!_xactPointers.ContainsKey(ptr))
				{
					return;
				}

				_notificationDesc.type = _FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED;
				_notificationDesc.pCue = (_FAudio.FACTCue*)ptr;
				_FAudio.FACTAudioEngine_UnRegisterNotification((_FAudio.FACTAudioEngine*)_handle, (_FAudio.FACTNotificationDescription*)Unsafe.AsPointer(ref _notificationDesc));
				_xactPointers.Remove(ptr);
			}
		}

		private class IntPtrComparer : IEqualityComparer<IntPtr>
		{
			public bool Equals(IntPtr x, IntPtr y) => x == y;

			public int GetHashCode(IntPtr obj) => obj.GetHashCode();
		}
	}
}
