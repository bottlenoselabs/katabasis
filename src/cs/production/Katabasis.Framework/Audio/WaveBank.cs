// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using static bottlenoselabs.FAudio;

namespace bottlenoselabs.Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.wavebank.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class WaveBank : IDisposable
	{
		private readonly AudioEngine _engine;

		private IntPtr _handle;

		private IntPtr _bankData;
		private ulong _bankDataLen; // Non-zero for in-memory WaveBanks

		private WeakReference? _selfReference;

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

			_bankData = TitleContainer.ReadToPointer(nonStreamingWaveBankFilename, out _bankDataLen);

			FACTWaveBank* waveBank;
			FACTAudioEngine_CreateInMemoryWaveBank(
				(FACTAudioEngine*)audioEngine._handle,
				(void*)_bankData,
				(uint)_bankDataLen,
				0,
				0,
				&waveBank);
			_handle = (IntPtr)waveBank;

			_engine = audioEngine;
			_selfReference = new WeakReference(this, true);
			_engine.RegisterPointer(_handle, _selfReference);
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

			_bankData = (IntPtr)FAudio_fopen(safeName);

			FACTWaveBank* waveBank;
			var settings = default(FACTStreamingParameters);
			settings.file = (void*)_bankData;
			FACTAudioEngine_CreateStreamingWaveBank(
				audioEngine._handle,
				(FACTStreamingParameters*)Unsafe.AsPointer(ref settings),
				&waveBank);
			_handle = (IntPtr)waveBank;

			_engine = audioEngine;
			_selfReference = new WeakReference(this, true);
			_engine.RegisterPointer(_handle, _selfReference);
			IsDisposed = false;
		}

		public bool IsDisposed { get; private set; }

		public bool IsPrepared
		{
			get
			{
				uint state;
				FACTWaveBank_GetState((FACTWaveBank*)_handle, &state);
				return (state & FACT_STATE_PREPARED) != 0;
			}
		}

		public bool IsInUse
		{
			get
			{
				uint state;
				FACTWaveBank_GetState((FACTWaveBank*)_handle, &state);
				return (state & FACT_STATE_INUSE) != 0;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public event EventHandler<EventArgs>? Disposing;

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
			if (_bankData != IntPtr.Zero)
			{
				if (_bankDataLen != 0)
				{
					FNAPlatform.FreeFilePointer(_bankData);
					_bankDataLen = 0;
				}
				else
				{
					FAudio_close((FAudioIOStream*)_bankData);
				}

				_bankData = IntPtr.Zero;
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
						FACTWaveBank_Destroy((FACTWaveBank*)_handle);
					}

					OnWaveBankDestroyed();
				}
			}
		}
	}
}
