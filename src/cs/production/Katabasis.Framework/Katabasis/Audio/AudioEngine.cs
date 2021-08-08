// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/dd940262.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class AudioEngine : IDisposable
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
		internal readonly byte[] _handle3D;

		private readonly RendererDetail[] _rendererDetails;

		private FAudio.FACTNotificationDescription _notificationDesc;

		public AudioEngine(string settingsFile)
			: this(
				settingsFile,
				new TimeSpan(0, 0, 0, 0, (int)FAudio.FACT_ENGINE_LOOKAHEAD_DEFAULT),
				string.Empty)
		{
		}

		public AudioEngine(string settingsFile, TimeSpan lookAheadTime, string rendererId)
		{
			if (string.IsNullOrEmpty(settingsFile))
			{
				throw new ArgumentNullException(nameof(settingsFile));
			}

			// Read entire file into memory, let FACT manage the pointer
			IntPtr bufferLen;
			IntPtr buffer = TitleContainer.ReadToPointer(settingsFile, out bufferLen);

			// Generate engine parameters
			var settings = default(FAudio.FACTRuntimeParameters);
			settings.pGlobalSettingsBuffer = buffer;
			settings.globalSettingsBufferSize = (uint)bufferLen;
			FAudio.FACTNotificationCallback xactNotificationFunc = OnXACTNotification;
			settings.fnNotificationCallback = Marshal.GetFunctionPointerForDelegate(xactNotificationFunc);

			// Special parameters from constructor
			settings.lookAheadTime = (uint)lookAheadTime.Milliseconds;
			if (!string.IsNullOrEmpty(rendererId))
			{
				// FIXME: wchar_t? -flibit
				settings.pRendererID = Marshal.StringToHGlobalAuto(rendererId);
			}

			// Init engine, finally
			FAudio.FACTCreateEngine(0, out _handle);
			if (FAudio.FACTAudioEngine_Initialize(_handle, ref settings) != 0)
			{
				throw new InvalidOperationException("Engine initialization failed!");
			}

			// Free the settings strings
			if (settings.pRendererID != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(settings.pRendererID);
			}

			// Grab RendererDetails
			FAudio.FACTAudioEngine_GetRendererCount(_handle, out var rendererCount);
			if (rendererCount == 0)
			{
				Dispose();
				throw new NoAudioHardwareException();
			}

			_rendererDetails = new RendererDetail[rendererCount];
			byte[] converted = new byte[0xFF * sizeof(short)];
			for (ushort i = 0; i < rendererCount; i += 1)
			{
				FAudio.FACTAudioEngine_GetRendererDetails(_handle, i, out var details);
				unsafe
				{
					for (var j = 0; j < 0xFF; j += 1)
					{
						Marshal.Copy((IntPtr) details.displayName, converted, 0, converted.Length);
						string name = System.Text.Encoding.Unicode.GetString(converted).TrimEnd('\0');
						Marshal.Copy((IntPtr) details.rendererID, converted, 0, converted.Length);
						string id = System.Text.Encoding.Unicode.GetString(converted).TrimEnd('\0');
						_rendererDetails[i] = new RendererDetail(name, id);
					}
				}
			}

			// Init 3D audio
			_handle3D = new byte[FAudio.F3DAUDIO_HANDLE_BYTESIZE];
			FAudio.FACT3DInitialize(_handle, _handle3D);

			// Grab channel count for DSP_SETTINGS
			FAudio.FACTAudioEngine_GetFinalMixFormat(_handle, out var mixFormat);
			_channels = mixFormat.Format.nChannels;

			// All XACT references have to go through here...
			_notificationDesc = default;
		}

		public ReadOnlyCollection<RendererDetail> RendererDetails => new(_rendererDetails);

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
				FAudio.FACTAudioEngine_ShutDown(_handle);
				IsDisposed = true;
			}
		}

		[MonoPInvokeCallback(typeof(FAudio.FACTNotificationCallback))]
		private static unsafe void OnXACTNotification(IntPtr notification)
		{
			WeakReference? reference;
			var not = (FAudio.FACTNotification*)notification;
			switch (not->type)
			{
				case FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED:
				{
					var target = not->anon.waveBank.pWaveBank;
					lock (_xactPointers)
					{
						if (_xactPointers.TryGetValue(target, out reference))
						{
							if (reference.IsAlive)
							{
								(reference.Target as WaveBank)!.OnWaveBankDestroyed();
							}
						}

						_xactPointers.Remove(target);
					}

					break;
				}

				case FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED:
				{
					var target = not->anon.soundBank.pSoundBank;
					lock (_xactPointers)
					{
						if (_xactPointers.TryGetValue(target, out reference))
						{
							if (reference.IsAlive)
							{
								(reference.Target as SoundBank)!.OnSoundBankDestroyed();
							}
						}

						_xactPointers.Remove(target);
					}

					break;
				}

				case FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED:
				{
					var target = not->anon.cue.pCue;
					lock (_xactPointers)
					{
						if (_xactPointers.TryGetValue(target, out reference))
						{
							if (reference.IsAlive)
							{
								(reference.Target as Cue)!.OnCueDestroyed();
							}
						}

						_xactPointers.Remove(target);
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

			var category = FAudio.FACTAudioEngine_GetCategory(_handle, name);

			if (category == FAudio.FACTCATEGORY_INVALID)
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

			var variable = FAudio.FACTAudioEngine_GetGlobalVariableIndex(_handle, name);

			if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			FAudio.FACTAudioEngine_GetGlobalVariable(_handle, variable, out var result);
			return result;
		}

		public void SetGlobalVariable(string name, float value)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var variable = FAudio.FACTAudioEngine_GetGlobalVariableIndex(_handle, name);

			if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			FAudio.FACTAudioEngine_SetGlobalVariable(_handle, variable, value);
		}

		public void Update() => FAudio.FACTAudioEngine_DoWork(_handle);

		internal void RegisterWaveBank(IntPtr ptr, WeakReference reference)
		{
			_notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED;
			_notificationDesc.pWaveBank = ptr;
			FAudio.FACTAudioEngine_RegisterNotification(_handle, ref _notificationDesc);
			lock (_xactPointers)
			{
				_xactPointers.Add(ptr, reference);
			}
		}

		internal void RegisterSoundBank(IntPtr ptr, WeakReference reference)
		{
			_notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED;
			_notificationDesc.pSoundBank = ptr;
			FAudio.FACTAudioEngine_RegisterNotification(_handle, ref _notificationDesc);
			lock (_xactPointers)
			{
				_xactPointers.Add(ptr, reference);
			}
		}

		internal void RegisterCue(IntPtr ptr, WeakReference reference)
		{
			_notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED;
			_notificationDesc.pCue = ptr;
			FAudio.FACTAudioEngine_RegisterNotification(_handle, ref _notificationDesc);
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

				_notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED;
				_notificationDesc.pWaveBank = ptr;
				FAudio.FACTAudioEngine_UnRegisterNotification(_handle, ref _notificationDesc);
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

				_notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED;
				_notificationDesc.pSoundBank = ptr;
				FAudio.FACTAudioEngine_UnRegisterNotification(_handle, ref _notificationDesc);
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

				_notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED;
				_notificationDesc.pCue = ptr;
				FAudio.FACTAudioEngine_UnRegisterNotification(_handle, ref _notificationDesc);
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
