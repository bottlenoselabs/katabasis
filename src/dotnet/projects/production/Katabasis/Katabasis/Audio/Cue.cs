// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
    // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.cue.aspx
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
    public sealed class Cue : IDisposable
    {
        private IntPtr _handle;
        private readonly SoundBank _bank;
        private WeakReference? _selfReference;

        public bool IsCreated
        {
            get
            {
                FAudio.FACTCue_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_CREATED) != 0;
            }
        }

        public bool IsDisposed { get; private set; }

        public bool IsPaused
        {
            get
            {
                FAudio.FACTCue_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_PAUSED) != 0;
            }
        }

        public bool IsPlaying
        {
            get
            {
                FAudio.FACTCue_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_PLAYING) != 0;
            }
        }

        public bool IsPrepared
        {
            get
            {
                FAudio.FACTCue_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_PREPARED) != 0;
            }
        }

        public bool IsPreparing
        {
            get
            {
                FAudio.FACTCue_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_PREPARING) != 0;
            }
        }

        public bool IsStopped
        {
            get
            {
                FAudio.FACTCue_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_STOPPED) != 0;
            }
        }

        public bool IsStopping
        {
            get
            {
                FAudio.FACTCue_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_STOPPING) != 0;
            }
        }

        public string Name { get; }

        public event EventHandler<EventArgs>? Disposing;

        internal Cue(IntPtr cue, string name, SoundBank soundBank)
        {
            _handle = cue;
            Name = name;
            _bank = soundBank;

            _selfReference = new WeakReference(this, true);
            _bank._engine.RegisterCue(_handle, _selfReference);
        }

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

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            emitter._emitterData.ChannelCount = _bank._dspSettings.SrcChannelCount;
            emitter._emitterData.CurveDistanceScaler = float.MaxValue;
            FAudio.FACT3DCalculate(
                _bank._engine._handle3D,
                ref listener._listenerData,
                ref emitter._emitterData,
                ref _bank._dspSettings);
            FAudio.FACT3DApply(ref _bank._dspSettings, _handle);
        }

        public float GetVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var variable = FAudio.FACTCue_GetVariableIndex(_handle, name);

            if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
            {
                throw new InvalidOperationException("Invalid variable name!");
            }

            FAudio.FACTCue_GetVariable(_handle, variable, out var result);
            return result;
        }

        public void Pause()
        {
            FAudio.FACTCue_Pause(_handle, 1);
        }

        public void Play()
        {
            FAudio.FACTCue_Play(_handle);
        }

        public void Resume()
        {
            FAudio.FACTCue_Pause(_handle, 0);
        }

        public void SetVariable(string name, float value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var variable = FAudio.FACTCue_GetVariableIndex(_handle, name);

            if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
            {
                throw new InvalidOperationException("Invalid variable name!");
            }

            FAudio.FACTCue_SetVariable(
                _handle,
                variable,
                value);
        }

        public void Stop(AudioStopOptions options)
        {
            FAudio.FACTCue_Stop(
                _handle,
                options == AudioStopOptions.Immediate ? FAudio.FACT_FLAG_STOP_IMMEDIATE : FAudio.FACT_FLAG_STOP_RELEASE);
        }

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
                        FAudio.FACTCue_Destroy(_handle);
                    }

                    OnCueDestroyed();
                }
            }
        }
    }
}
