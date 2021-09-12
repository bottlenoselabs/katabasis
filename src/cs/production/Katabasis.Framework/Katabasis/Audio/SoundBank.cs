// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.soundbank.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class SoundBank : IDisposable
	{
		internal _FAudio.F3DAUDIO_DSP_SETTINGS _dspSettings;
		internal AudioEngine _engine;
		private IntPtr _handle;
		private WeakReference? _selfReference;

		public SoundBank(AudioEngine audioEngine, string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException(nameof(filename));
			}

			var buffer = TitleContainer.ReadToPointer(filename, out var bufferLen);

			_FAudio.FACTSoundBank* soundBank;
			_FAudio.FACTAudioEngine_CreateSoundBank(
				(_FAudio.FACTAudioEngine*)audioEngine._handle,
				(void*)buffer,
				(uint) bufferLen,
				0,
				0,
				&soundBank);
			_handle = (IntPtr)soundBank;

			FNAPlatform.FreeFilePointer(buffer);

			_engine = audioEngine;
			_selfReference = new WeakReference(this, true);
			_dspSettings = default;
			_dspSettings.SrcChannelCount = 1;
			_dspSettings.DstChannelCount = _engine._channels;
			_dspSettings.pMatrixCoefficients = (float*)Marshal.AllocHGlobal(
				4 *
				(int)_dspSettings.SrcChannelCount *
				(int)_dspSettings.DstChannelCount);

			_engine.RegisterSoundBank(_handle, _selfReference);
			IsDisposed = false;
		}

		public bool IsDisposed { get; private set; }

		public bool IsInUse
		{
			get
			{
				uint state;
				_FAudio.FACTSoundBank_GetState((_FAudio.FACTSoundBank*)_handle, &state);
				return (state & _FAudio.FACT_STATE_INUSE) != 0;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public event EventHandler<EventArgs>? Disposing;

		~SoundBank()
		{
			if (AudioEngine.ProgramExiting)
			{
				return;
			}

			if (!IsDisposed && IsInUse)
			{
				// STOP LEAKING YOUR BANKS, ARGH
				GC.ReRegisterForFinalize(this);
				return;
			}

			Dispose(false);
		}

		public Cue GetCue(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var cue = _FAudio.FACTSoundBank_GetCueIndex((_FAudio.FACTSoundBank*)_handle, name);

			if (cue == _FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid cue name!");
			}

			_FAudio.FACTCue* result;
			_FAudio.FACTSoundBank_Prepare(
				(_FAudio.FACTSoundBank*)_handle,
				cue,
				0,
				0,
				&result);

			return new Cue((IntPtr)result, name, this);
		}

		public void PlayCue(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var cue = _FAudio.FACTSoundBank_GetCueIndex((_FAudio.FACTSoundBank*)_handle, name);

			if (cue == _FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid cue name!");
			}

			_FAudio.FACTSoundBank_Play((_FAudio.FACTSoundBank*)_handle, cue, 0, 0, (_FAudio.FACTCue**)IntPtr.Zero);
		}

		public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var cue = _FAudio.FACTSoundBank_GetCueIndex((_FAudio.FACTSoundBank*)_handle, name);

			if (cue == _FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid cue name!");
			}

			emitter._emitterData.ChannelCount = _dspSettings.SrcChannelCount;
			emitter._emitterData.CurveDistanceScaler = float.MaxValue;
			_FAudio.FACT3DCalculate(
				_engine._handle3D,
				(_FAudio.F3DAUDIO_LISTENER*)Unsafe.AsPointer(ref listener._listenerData),
				(_FAudio.F3DAUDIO_EMITTER*)Unsafe.AsPointer(ref emitter._emitterData),
				(_FAudio.F3DAUDIO_DSP_SETTINGS*)Unsafe.AsPointer(ref _dspSettings));

			_FAudio.FACTSoundBank_Play3D(
				(_FAudio.FACTSoundBank*)_handle,
				cue,
				0,
				0,
				(_FAudio.F3DAUDIO_DSP_SETTINGS*)Unsafe.AsPointer(ref _dspSettings),
				(_FAudio.FACTCue**)IntPtr.Zero);
		}

		internal void OnSoundBankDestroyed()
		{
			IsDisposed = true;
			_handle = IntPtr.Zero;
			_selfReference = null;
		}

		protected void Dispose(bool disposing)
		{
			lock (_engine._gcSync)
			{
				if (!IsDisposed)
				{
					Disposing?.Invoke(this, EventArgs.Empty);

					// If this is disposed, stop leaking memory!
					if (!_engine.IsDisposed)
					{
						_engine.UnregisterSoundBank(_handle);
						_FAudio.FACTSoundBank_Destroy((_FAudio.FACTSoundBank*)_handle);
						Marshal.FreeHGlobal((IntPtr)_dspSettings.pMatrixCoefficients);
					}

					OnSoundBankDestroyed();
				}
			}
		}
	}
}
