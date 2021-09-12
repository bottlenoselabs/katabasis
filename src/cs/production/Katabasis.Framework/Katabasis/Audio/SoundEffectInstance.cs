// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.soundeffectinstance.aspx
	public unsafe class SoundEffectInstance : IDisposable
	{
		private readonly SoundEffect? _parentEffect;
		private readonly WeakReference _selfReference;
		private _FAudio.F3DAUDIO_DSP_SETTINGS _dspSettings;
		internal IntPtr _handle;
		private bool _hasStarted;
		private bool _is3D;
		internal bool _isDynamic;
		private bool _looped;
		private float _pan;
		private float _pitch;
		private SoundState _state;
		private bool _usingReverb;
		private float _volume = 1.0f;

		internal SoundEffectInstance(SoundEffect? parent = null)
		{
			SoundEffect.Device();

			_selfReference = new WeakReference(this, true);
			_parentEffect = parent;
			_isDynamic = this is DynamicSoundEffectInstance;
			_hasStarted = false;
			_is3D = false;
			_usingReverb = false;
			_state = SoundState.Stopped;

			if (!_isDynamic)
			{
				InitDSPSettings(_parentEffect!._channels);
			}

			_parentEffect?.Instances.Add(_selfReference);
		}

		public bool IsDisposed { get; protected set; }

		public virtual bool IsLooped
		{
			get => _looped;
			set
			{
				if (_hasStarted)
				{
					throw new InvalidOperationException();
				}

				_looped = value;
			}
		}

		public float Pan
		{
			get => _pan;
			set
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException("SoundEffectInstance");
				}

				if (value > 1.0f || value < -1.0f)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_pan = value;
				if (_is3D)
				{
					return;
				}

				SetPanMatrixCoefficients();
				if (_handle != IntPtr.Zero)
				{
					_FAudio.FAudioVoice_SetOutputMatrix(
						(_FAudio.FAudioVoice*)_handle,
						(_FAudio.FAudioVoice*)SoundEffect.Device().MasterVoice,
						_dspSettings.SrcChannelCount,
						_dspSettings.DstChannelCount,
						_dspSettings.pMatrixCoefficients,
						0);
				}
			}
		}

		public float Pitch
		{
			get => _pitch;
			set
			{
				_pitch = MathHelper.Clamp(value, -1.0f, 1.0f);
				if (_handle != IntPtr.Zero)
				{
					UpdatePitch();
				}
			}
		}

		public SoundState State
		{
			get
			{
				if (!_isDynamic &&
				    _handle != IntPtr.Zero &&
				    _state == SoundState.Playing)
				{
					_FAudio.FAudioVoiceState state;
					_FAudio.FAudioSourceVoice_GetState(
						(_FAudio.FAudioSourceVoice*)_handle,
						&state,
						_FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED);

					if (state.BuffersQueued == 0)
					{
						Stop(true);
					}
				}

				return _state;
			}
		}

		public float Volume
		{
			get => _volume;
			set
			{
				_volume = value;
				if (_handle != IntPtr.Zero)
				{
					_FAudio.FAudioVoice_SetVolume(
						(_FAudio.FAudioVoice*)_handle,
						_volume,
						0);
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~SoundEffectInstance()
		{
			if (!IsDisposed && State == SoundState.Playing)
			{
				// STOP LEAKING YOUR INSTANCES, ARGH
				GC.ReRegisterForFinalize(this);
				return;
			}

			Dispose();
		}

		public void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			if (listener == null)
			{
				throw new ArgumentNullException(nameof(listener));
			}

			if (emitter == null)
			{
				throw new ArgumentNullException(nameof(emitter));
			}

			if (IsDisposed)
			{
				throw new ObjectDisposedException("SoundEffectInstance");
			}

			_is3D = true;
			SoundEffect.FAudioContext dev = SoundEffect.Device();
			emitter._emitterData.CurveDistanceScaler = dev.CurveDistanceScaler;
			emitter._emitterData.ChannelCount = _dspSettings.SrcChannelCount;
			_FAudio.F3DAudioCalculate(
				dev.Handle3D!,
				(_FAudio.F3DAUDIO_LISTENER*)Unsafe.AsPointer(ref listener._listenerData),
				(_FAudio.F3DAUDIO_EMITTER*)Unsafe.AsPointer(ref emitter._emitterData),
				_FAudio.F3DAUDIO_CALCULATE_MATRIX | _FAudio.F3DAUDIO_CALCULATE_DOPPLER,
				(_FAudio.F3DAUDIO_DSP_SETTINGS*)Unsafe.AsPointer(ref _dspSettings));

			if (_handle != IntPtr.Zero)
			{
				UpdatePitch();
				_FAudio.FAudioVoice_SetOutputMatrix(
					(_FAudio.FAudioVoice*)_handle,
					(_FAudio.FAudioVoice*)SoundEffect.Device().MasterVoice,
					_dspSettings.SrcChannelCount,
					_dspSettings.DstChannelCount,
					_dspSettings.pMatrixCoefficients,
					0);
			}
		}

		public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
		{
			if (listeners == null)
			{
				throw new ArgumentNullException(nameof(listeners));
			}

			if (listeners.Length == 1)
			{
				Apply3D(listeners[0], emitter);
				return;
			}

			throw new NotSupportedException("Only one listener is supported.");
		}

		public virtual void Play()
		{
			if (State == SoundState.Playing)
			{
				return;
			}

			if (State == SoundState.Paused)
			{
				/* Just resume the existing handle */
				_FAudio.FAudioSourceVoice_Start((_FAudio.FAudioSourceVoice*)_handle, 0, 0);
				_state = SoundState.Playing;
				return;
			}

			SoundEffect.FAudioContext dev = SoundEffect.Device();

			/* Create handle */
			if (_isDynamic)
			{
				_FAudio.FAudioSourceVoice* handle;
				_FAudio.FAudio_CreateSourceVoice(
					(_FAudio.FAudio*)dev.Handle,
					&handle,
					(_FAudio.FAudioWaveFormatEx*)Unsafe.AsPointer(ref (this as DynamicSoundEffectInstance)!._format),
					_FAudio.FAUDIO_VOICE_USEFILTER,
					_FAudio.FAUDIO_DEFAULT_FREQ_RATIO,
					(_FAudio.FAudioVoiceCallback*)IntPtr.Zero,
					(_FAudio.FAudioVoiceSends*)IntPtr.Zero,
					(_FAudio.FAudioEffectChain*)IntPtr.Zero);
				_handle = (IntPtr)handle;
			}
			else
			{
				_FAudio.FAudioSourceVoice* handle;
				_FAudio.FAudio_CreateSourceVoice(
					(_FAudio.FAudio*)dev.Handle,
					&handle,
					(_FAudio.FAudioWaveFormatEx*)_parentEffect!._formatPtr,
					_FAudio.FAUDIO_VOICE_USEFILTER,
					_FAudio.FAUDIO_DEFAULT_FREQ_RATIO,
					(_FAudio.FAudioVoiceCallback*)IntPtr.Zero,
					(_FAudio.FAudioVoiceSends*)IntPtr.Zero,
					(_FAudio.FAudioEffectChain*)IntPtr.Zero);
				_handle = (IntPtr)handle;
			}

			if (_handle == IntPtr.Zero)
			{
				return; /* What */
			}

			/* Apply current properties */
			_FAudio.FAudioVoice_SetVolume((_FAudio.FAudioVoice*)_handle, _volume, 0);
			UpdatePitch();
			if (_is3D || Pan != 0.0f)
			{
				_FAudio.FAudioVoice_SetOutputMatrix(
					(_FAudio.FAudioVoice*)_handle,
					(_FAudio.FAudioVoice*)SoundEffect.Device().MasterVoice,
					_dspSettings.SrcChannelCount,
					_dspSettings.DstChannelCount,
					_dspSettings.pMatrixCoefficients,
					0);
			}

			/* For static effects, submit the buffer now */
			if (_isDynamic)
			{
				var dynamicSoundEffect = this as DynamicSoundEffectInstance;
				dynamicSoundEffect!.QueueInitialBuffers();
			}
			else
			{
				if (IsLooped)
				{
					_parentEffect!._handle.LoopCount = 255;
					_parentEffect._handle.LoopBegin = _parentEffect._loopStart;
					_parentEffect._handle.LoopLength = _parentEffect._loopLength;
				}
				else
				{
					_parentEffect!._handle.LoopCount = 0;
					_parentEffect._handle.LoopBegin = 0;
					_parentEffect._handle.LoopLength = 0;
				}

				_FAudio.FAudioSourceVoice_SubmitSourceBuffer(
					(_FAudio.FAudioSourceVoice*)_handle,
					(_FAudio.FAudioBuffer*)Unsafe.AsPointer(ref _parentEffect._handle),
					(_FAudio.FAudioBufferWMA*)IntPtr.Zero);
			}

			/* Play, finally. */
			_FAudio.FAudioSourceVoice_Start((_FAudio.FAudioSourceVoice*)_handle, 0, 0);
			_state = SoundState.Playing;
			_hasStarted = true;
		}

		public void Pause()
		{
			if (_handle != IntPtr.Zero && State == SoundState.Playing)
			{
				_FAudio.FAudioSourceVoice_Stop((_FAudio.FAudioSourceVoice*)_handle, 0, 0);
				_state = SoundState.Paused;
			}
		}

		public void Resume()
		{
			var state = State; // Triggers a query, update
			if (_handle == IntPtr.Zero)
			{
				// XNA4 just plays if we've not started yet.
				Play();
			}
			else if (state == SoundState.Paused)
			{
				_FAudio.FAudioSourceVoice_Start((_FAudio.FAudioSourceVoice*)_handle, 0, 0);
				_state = SoundState.Playing;
			}
		}

		public void Stop() => Stop(true);

		public void Stop(bool immediate)
		{
			if (_handle == IntPtr.Zero)
			{
				return;
			}

			if (immediate)
			{
				_FAudio.FAudioSourceVoice_Stop((_FAudio.FAudioSourceVoice*)_handle, 0, 0);
				_FAudio.FAudioSourceVoice_FlushSourceBuffers((_FAudio.FAudioSourceVoice*)_handle);
				_FAudio.FAudioVoice_DestroyVoice((_FAudio.FAudioVoice*)_handle);
				_handle = IntPtr.Zero;
				_usingReverb = false;
				_state = SoundState.Stopped;

				if (_isDynamic)
				{
					var dynamicSoundEffect = this as DynamicSoundEffectInstance;
					lock (FrameworkDispatcher.Streams)
					{
						FrameworkDispatcher.Streams.Remove(dynamicSoundEffect!);
					}

					dynamicSoundEffect!.ClearBuffers();
				}
			}
			else
			{
				if (_isDynamic)
				{
					throw new InvalidOperationException();
				}

				_FAudio.FAudioSourceVoice_ExitLoop((_FAudio.FAudioSourceVoice*)_handle, 0);
			}
		}

		internal void InitDSPSettings(uint srcChannels)
		{
			_dspSettings = default;
			_dspSettings.DopplerFactor = 1.0f;
			_dspSettings.SrcChannelCount = srcChannels;
			_dspSettings.DstChannelCount = SoundEffect.Device().DeviceDetails.OutputFormat.Format.nChannels;

			var memorySize = 4 *
			                 (int)_dspSettings.SrcChannelCount *
			                 (int)_dspSettings.DstChannelCount;

			_dspSettings.pMatrixCoefficients = (float*)Marshal.AllocHGlobal(memorySize);
			unsafe
			{
				var memPtr = (byte*)_dspSettings.pMatrixCoefficients;
				for (var i = 0; i < memorySize; i += 1)
				{
					memPtr[i] = 0;
				}
			}

			SetPanMatrixCoefficients();
		}

		internal unsafe void INTERNAL_applyReverb(float rvGain)
		{
			if (_handle == IntPtr.Zero)
			{
				return;
			}

			if (!_usingReverb)
			{
				SoundEffect.Device().AttachReverb(_handle);
				_usingReverb = true;
			}

			// Re-using this float array...
			var outputMatrix = (float*)_dspSettings.pMatrixCoefficients;
			outputMatrix[0] = rvGain;
			if (_dspSettings.SrcChannelCount == 2)
			{
				outputMatrix[1] = rvGain;
			}

			_FAudio.FAudioVoice_SetOutputMatrix(
				(_FAudio.FAudioVoice*)_handle,
				(_FAudio.FAudioVoice*)SoundEffect.Device().ReverbVoice,
				_dspSettings.SrcChannelCount,
				1,
				_dspSettings.pMatrixCoefficients,
				0);
		}

		internal void INTERNAL_applyLowPassFilter(float cutoff)
		{
			if (_handle == IntPtr.Zero)
			{
				return;
			}

			var p = default(_FAudio.FAudioFilterParameters);
			p.Type = _FAudio.FAudioFilterType.FAudioLowPassFilter;
			p.Frequency = cutoff;
			p.OneOverQ = 1.0f;
			_FAudio.FAudioVoice_SetFilterParameters((_FAudio.FAudioVoice*)_handle, &p, 0);
		}

		internal void INTERNAL_applyHighPassFilter(float cutoff)
		{
			if (_handle == IntPtr.Zero)
			{
				return;
			}

			var p = default(_FAudio.FAudioFilterParameters);
			p.Type = _FAudio.FAudioFilterType.FAudioHighPassFilter;
			p.Frequency = cutoff;
			p.OneOverQ = 1.0f;
			_FAudio.FAudioVoice_SetFilterParameters((_FAudio.FAudioVoice*)_handle, &p, 0);
		}

		internal void INTERNAL_applyBandPassFilter(float center)
		{
			if (_handle == IntPtr.Zero)
			{
				return;
			}

			var p = default(_FAudio.FAudioFilterParameters);
			p.Type = _FAudio.FAudioFilterType.FAudioBandPassFilter;
			p.Frequency = center;
			p.OneOverQ = 1.0f;
			_FAudio.FAudioVoice_SetFilterParameters((_FAudio.FAudioVoice*)_handle, &p, 0);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Stop(true);
				_parentEffect?.Instances.Remove(_selfReference);
				Marshal.FreeHGlobal((IntPtr)_dspSettings.pMatrixCoefficients);
				IsDisposed = true;
			}
		}

		private void UpdatePitch()
		{
			float doppler;
			var dopplerScale = SoundEffect.Device().DopplerScaleAudioContext;
			if (!_is3D || dopplerScale == 0.0f)
			{
				doppler = 1.0f;
			}
			else
			{
				doppler = _dspSettings.DopplerFactor * dopplerScale;
			}

			_FAudio.FAudioSourceVoice_SetFrequencyRatio(
				(_FAudio.FAudioSourceVoice*)_handle,
				(float)Math.Pow(2.0, _pitch) * doppler,
				0);
		}

		private unsafe void SetPanMatrixCoefficients()
		{
			/* Two major things to notice:
			 * 1. The spec assumes any speaker count >= 2 has Front Left/Right.
			 * 2. Stereo panning is WAY more complicated than you think.
			 *    The main thing is that hard panning does NOT eliminate an
			 *    entire channel; the two channels are blended on each side.
			 * Aside from that, XNA is pretty naive about the output matrix.
			 * -flibit
			 */
			var outputMatrix = (float*)_dspSettings.pMatrixCoefficients;
			if (_dspSettings.SrcChannelCount == 1)
			{
				if (_dspSettings.DstChannelCount == 1)
				{
					outputMatrix[0] = 1.0f;
				}
				else
				{
					outputMatrix[0] = _pan > 0.0f ? 1.0f - _pan : 1.0f;
					outputMatrix[1] = _pan < 0.0f ? 1.0f + _pan : 1.0f;
				}
			}
			else
			{
				if (_dspSettings.DstChannelCount == 1)
				{
					outputMatrix[0] = 1.0f;
					outputMatrix[1] = 1.0f;
				}
				else
				{
					if (_pan <= 0.0f)
					{
						// Left speaker blends left/right channels
						outputMatrix[0] = (0.5f * _pan) + 1.0f;
						outputMatrix[1] = 0.5f * -_pan;
						// Right speaker gets less of the right channel
						outputMatrix[2] = 0.0f;
						outputMatrix[3] = _pan + 1.0f;
					}
					else
					{
						// Left speaker gets less of the left channel
						outputMatrix[0] = -_pan + 1.0f;
						outputMatrix[1] = 0.0f;
						// Right speaker blends right/left channels
						outputMatrix[2] = 0.5f * _pan;
						outputMatrix[3] = (0.5f * -_pan) + 1.0f;
					}
				}
			}
		}
	}
}
