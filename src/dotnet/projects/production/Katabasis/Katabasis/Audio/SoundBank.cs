// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.soundbank.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class SoundBank : IDisposable
	{
		internal FAudio.F3DAUDIO_DSP_SETTINGS _dspSettings;
		internal AudioEngine _engine;
		private IntPtr _handle;
		private WeakReference? _selfReference;

		public SoundBank(AudioEngine audioEngine, string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException(nameof(filename));
			}

			byte[] buffer = TitleContainer.ReadAllBytes(filename);
			var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

			FAudio.FACTAudioEngine_CreateSoundBank(
				audioEngine._handle,
				pin.AddrOfPinnedObject(),
				(uint)buffer.Length,
				0,
				0,
				out _handle);

			pin.Free();

			_engine = audioEngine;
			_selfReference = new WeakReference(this, true);
			_dspSettings = default;
			_dspSettings.SrcChannelCount = 1;
			_dspSettings.DstChannelCount = _engine._channels;
			_dspSettings.pMatrixCoefficients = Marshal.AllocHGlobal(
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
				FAudio.FACTSoundBank_GetState(_handle, out var state);
				return (state & FAudio.FACT_STATE_INUSE) != 0;
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

			var cue = FAudio.FACTSoundBank_GetCueIndex(_handle, name);

			if (cue == FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid cue name!");
			}

			FAudio.FACTSoundBank_Prepare(
				_handle,
				cue,
				0,
				0,
				out var result);

			return new Cue(result, name, this);
		}

		public void PlayCue(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var cue = FAudio.FACTSoundBank_GetCueIndex(_handle, name);

			if (cue == FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid cue name!");
			}

			FAudio.FACTSoundBank_Play(_handle, cue, 0, 0, IntPtr.Zero);
		}

		public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var cue = FAudio.FACTSoundBank_GetCueIndex(_handle, name);

			if (cue == FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid cue name!");
			}

			emitter._emitterData.ChannelCount = _dspSettings.SrcChannelCount;
			emitter._emitterData.CurveDistanceScaler = float.MaxValue;
			FAudio.FACT3DCalculate(
				_engine._handle3D,
				ref listener._listenerData,
				ref emitter._emitterData,
				ref _dspSettings);

			FAudio.FACTSoundBank_Play3D(
				_handle,
				cue,
				0,
				0,
				ref _dspSettings,
				IntPtr.Zero);
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
						FAudio.FACTSoundBank_Destroy(_handle);
						Marshal.FreeHGlobal(_dspSettings.pMatrixCoefficients);
					}

					OnSoundBankDestroyed();
				}
			}
		}
	}
}
