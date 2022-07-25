// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static bottlenoselabs.FAudio;

namespace bottlenoselabs.Katabasis
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
				FACTCue_GetState((FACTCue*)_handle, &state);
				return (state & FACT_STATE_CREATED) != 0;
			}
		}

		public bool IsDisposed { get; private set; }

		public bool IsPaused
		{
			get
			{
				uint state;
				FACTCue_GetState((FACTCue*)_handle, &state);
				return (state & FACT_STATE_PAUSED) != 0;
			}
		}

		public bool IsPlaying
		{
			get
			{
				uint state;
				FACTCue_GetState((FACTCue*)_handle, &state);
				return (state & FACT_STATE_PLAYING) != 0;
			}
		}

		public bool IsPrepared
		{
			get
			{
				uint state;
				FACTCue_GetState((FACTCue*)_handle, &state);
				return (state & FACT_STATE_PREPARED) != 0;
			}
		}

		public bool IsPreparing
		{
			get
			{
				uint state;
				FACTCue_GetState((FACTCue*)_handle, &state);
				return (state & FACT_STATE_PREPARING) != 0;
			}
		}

		public bool IsStopped
		{
			get
			{
				uint state;
				FACTCue_GetState((FACTCue*)_handle, &state);
				return (state & FACT_STATE_STOPPED) != 0;
			}
		}

		public bool IsStopping
		{
			get
			{
				uint state;
				FACTCue_GetState((FACTCue*)_handle, &state);
				return (state & FACT_STATE_STOPPING) != 0;
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
			FACT3DCalculate(
				_bank._engine._handle3D,
				(F3DAUDIO_LISTENER*)Unsafe.AsPointer(ref listener._listenerData),
				(F3DAUDIO_EMITTER*)Unsafe.AsPointer(ref emitter._emitterData),
				(F3DAUDIO_DSP_SETTINGS*)Unsafe.AsPointer(ref _bank._dspSettings));

			FACT3DApply((F3DAUDIO_DSP_SETTINGS*)Unsafe.AsPointer(ref _bank._dspSettings), (FACTCue*)_handle);
		}

		public float GetVariable(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var variable = FACTCue_GetVariableIndex((FACTCue*)_handle, name);

			if (variable == FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			float result;
			FACTCue_GetVariable((FACTCue*)_handle, variable, &result);
			return result;
		}

		public void Pause() => FACTCue_Pause((FACTCue*)_handle, 1);

		public void Play() => FACTCue_Play((FACTCue*)_handle);

		public void Resume() => FACTCue_Pause((FACTCue*)_handle, 0);

		public void SetVariable(string name, float value)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			var variable = FACTCue_GetVariableIndex((FACTCue*)_handle, name);

			if (variable == FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException("Invalid variable name!");
			}

			FACTCue_SetVariable(
				(FACTCue*)_handle,
				variable,
				value);
		}

		public void Stop(AudioStopOptions options) =>
			FACTCue_Stop(
				(FACTCue*)_handle,
				options == AudioStopOptions.Immediate ? FACT_FLAG_STOP_IMMEDIATE : FACT_FLAG_STOP_RELEASE);

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
						FACTCue_Destroy((FACTCue*)_handle);
					}

					OnCueDestroyed();
				}
			}
		}
	}
}
