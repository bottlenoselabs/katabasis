// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.cue.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public sealed unsafe class Cue : IDisposable
	{
		private readonly SoundBank _bank;
		private IntPtr _handle;
		private WeakReference? _selfReference;

		internal Cue(IntPtr cue, string name, SoundBank soundBank)
		{
			_handle = cue;
			Name = name;
			_bank = soundBank;

			_selfReference = new WeakReference(this, true);
			_bank._engine.RegisterCue(_handle, _selfReference);
		}

		public bool IsCreated
		{
			get
			{
				uint state;
				_FAudio.FACTCue_GetState((_FAudio.FACTCue*)_handle, &state);
				return (state & _FAudio.FACT_STATE_CREATED) != 0;
			}
		}

		public bool IsDisposed { get; private set; }

		public bool IsPaused
		{
			get
			{
				uint state;
				_FAudio.FACTCue_GetState((_FAudio.FACTCue*)_handle, &state);
				return (state & _FAudio.FACT_STATE_PAUSED) != 0;
			}
		}

		public bool IsPlaying
		{
			get
			{
				uint state;
				_FAudio.FACTCue_GetState((_FAudio.FACTCue*)_handle, &state);
				return (state & _FAudio.FACT_STATE_PLAYING) != 0;
			}
		}

		public bool IsPrepared
		{
			get
			{
				uint state;
				_FAudio.FACTCue_GetState((_FAudio.FACTCue*)_handle, &state);
				return (state & _FAudio.FACT_STATE_PREPARED) != 0;
			}
		}

		public bool IsPreparing
		{
			get
			{
				uint state;
				_FAudio.FACTCue_GetState((_FAudio.FACTCue*)_handle, &state);
				return (state & _FAudio.FACT_STATE_PREPARING) != 0;
			}
		}

		public bool IsStopped
		{
			get
			{
				uint state;
				_FAudio.FACTCue_GetState((_FAudio.FACTCue*)_handle, &state);
				return (state & _FAudio.FACT_STATE_STOPPED) != 0;
			}
		}

		public bool IsStopping
		{
			get
			{
				uint state;
				_FAudio.FACTCue_GetState((_FAudio.FACTCue*)_handle, &state);
				return (state & _FAudio.FACT_STATE_STOPPING) != 0;
			}
		}

		public string Name { get; }

		public void Dispose()
		{
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}

		public event EventHandler<EventArgs>? Disposing;

		~Cue()
		{
			if (AudioEngine.ProgramExiting)
			{
				return;
			}

			if (!IsDisposed && IsPlaying)
			{
				// STOP LEAKING YOUR CUES, ARGH
				GC.ReRegisterForFinalize(this);
				return;
			}

			ReleaseUnmanagedResources();
		}

		public void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			emitter._emitterData.ChannelCount = _bank._dspSettings.SrcChannelCount;
			emitter._emitterData.CurveDistanceScaler = float.MaxValue;
			_FAudio.FACT3DCalculate(
				_bank._engine._handle3D,
				(_FAudio.F3DAUDIO_LISTENER*)Unsafe.AsPointer(ref listener._listenerData),
				(_FAudio.F3DAUDIO_EMITTER*)Unsafe.AsPointer(ref emitter._emitterData),
				(_FAudio.F3DAUDIO_DSP_SETTINGS*)Unsafe.AsPointer(ref _bank._dspSettings));

			_FAudio.FACT3DApply((_FAudio.F3DAUDIO_DSP_SETTINGS*)Unsafe.AsPointer(ref _bank._dspSettings), (_FAudio.FACTCue*)_handle);
		}

		public float GetVariable(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var variable = _FAudio.FACTCue_GetVariableIndex((_FAudio.FACTCue*)_handle, name);

			if (variable == _FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			float result;
			_FAudio.FACTCue_GetVariable((_FAudio.FACTCue*)_handle, variable, &result);
			return result;
		}

		public void Pause() => _FAudio.FACTCue_Pause((_FAudio.FACTCue*)_handle, 1);

		public void Play() => _FAudio.FACTCue_Play((_FAudio.FACTCue*)_handle);

		public void Resume() => _FAudio.FACTCue_Pause((_FAudio.FACTCue*)_handle, 0);

		public void SetVariable(string name, float value)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var variable = _FAudio.FACTCue_GetVariableIndex((_FAudio.FACTCue*)_handle, name);

			if (variable == _FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			_FAudio.FACTCue_SetVariable(
				(_FAudio.FACTCue*)_handle,
				variable,
				value);
		}

		public void Stop(AudioStopOptions options) =>
			_FAudio.FACTCue_Stop(
				(_FAudio.FACTCue*)_handle,
				options == AudioStopOptions.Immediate ? _FAudio.FACT_FLAG_STOP_IMMEDIATE : _FAudio.FACT_FLAG_STOP_RELEASE);

		internal void OnCueDestroyed()
		{
			IsDisposed = true;
			_handle = IntPtr.Zero;
			_selfReference = null;
		}

		private void ReleaseUnmanagedResources()
		{
			lock (_bank._engine._gcSync)
			{
				if (!IsDisposed)
				{
					Disposing?.Invoke(this, EventArgs.Empty);

					// If this is Disposed, stop leaking memory!
					if (!_bank._engine.IsDisposed)
					{
						_bank._engine.UnregisterCue(_handle);
						_FAudio.FACTCue_Destroy((_FAudio.FACTCue*)_handle);
					}

					OnCueDestroyed();
				}
			}
		}
	}
}
