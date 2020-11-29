// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Ankura
{
    // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.wavebank.aspx
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
    public class WaveBank : IDisposable
    {
        private IntPtr _handle;

        private readonly AudioEngine _engine;
        private WeakReference? _selfReference;

        // Non-streaming WaveBanks
        private byte[]? _buffer;
        private GCHandle _pin;

        // Streaming WaveBanks
        private IntPtr _ioStream;

        public event EventHandler<EventArgs>? Disposing;

        public bool IsDisposed { get; private set; }

        public bool IsPrepared
        {
            get
            {
                FAudio.FACTWaveBank_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_PREPARED) != 0;
            }
        }

        public bool IsInUse
        {
            get
            {
                FAudio.FACTWaveBank_GetState(_handle, out var state);
                return (state & FAudio.FACT_STATE_INUSE) != 0;
            }
        }

        public WaveBank(AudioEngine audioEngine, string nonStreamingWaveBankFilename)
        {
            if (audioEngine == null)
            {
                throw new ArgumentNullException(nameof(audioEngine));
            }

            if (string.IsNullOrEmpty(nonStreamingWaveBankFilename))
            {
                throw new ArgumentNullException(nameof(nonStreamingWaveBankFilename));
            }

            _buffer = TitleContainer.ReadAllBytes(nonStreamingWaveBankFilename);
            _pin = GCHandle.Alloc(_buffer, GCHandleType.Pinned);

            FAudio.FACTAudioEngine_CreateInMemoryWaveBank(
                audioEngine._handle,
                _pin.AddrOfPinnedObject(),
                (uint)_buffer.Length,
                0,
                0,
                out _handle);

            _engine = audioEngine;
            _selfReference = new WeakReference(this, true);
            _engine.RegisterWaveBank(_handle, _selfReference);
            IsDisposed = false;
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "TODO: Unused parameters?")]
        public WaveBank(
            AudioEngine audioEngine,
            string streamingWaveBankFilename,
            int offset,
            short packetSize)
        {
            if (audioEngine == null)
            {
                throw new ArgumentNullException(nameof(audioEngine));
            }

            if (string.IsNullOrEmpty(streamingWaveBankFilename))
            {
                throw new ArgumentNullException(nameof(streamingWaveBankFilename));
            }

            var safeName = FileHelpers.NormalizeFilePathSeparators(streamingWaveBankFilename);
            if (!Path.IsPathRooted(safeName))
            {
                safeName = Path.Combine(
                    TitleLocation.Path,
                    safeName);
            }

            _ioStream = FAudio.FAudio_fopen(safeName);

            var settings = default(FAudio.FACTStreamingParameters);
            settings.file = _ioStream;
            FAudio.FACTAudioEngine_CreateStreamingWaveBank(
                audioEngine._handle,
                ref settings,
                out _handle);

            _engine = audioEngine;
            _selfReference = new WeakReference(this, true);
            _engine.RegisterWaveBank(_handle, _selfReference);
            IsDisposed = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WaveBank()
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

        internal void OnWaveBankDestroyed()
        {
            IsDisposed = true;
            if (_buffer != null)
            {
                _pin.Free();
                _buffer = null;
            }
            else if (_ioStream != IntPtr.Zero)
            {
                FAudio.FAudio_close(_ioStream);
                _ioStream = IntPtr.Zero;
            }

            _handle = IntPtr.Zero;
            _selfReference = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_engine._gcSync)
            {
                if (!IsDisposed)
                {
                    Disposing?.Invoke(this, EventArgs.Empty);

                    // If this is disposed, stop leaking memory!
                    if (!_engine.IsDisposed)
                    {
                        _engine.UnregisterWaveBank(_handle);
                        FAudio.FACTWaveBank_Destroy(_handle);
                    }

                    OnWaveBankDestroyed();
                }
            }
        }
    }
}
