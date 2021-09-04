// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.dynamicsoundeffectinstance.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Need tests.")]
	public sealed class DynamicSoundEffectInstance : SoundEffectInstance
	{
		private const int MinimumBufferCheck = 3;
		private readonly AudioChannels _channels;

		private readonly List<IntPtr> _queuedBuffers;
		private readonly List<uint> _queuedSizes;

		private readonly int _sampleRate;
		internal FAudio.FAudioWaveFormatEx _format;

		public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels)
		{
			_sampleRate = sampleRate;
			_channels = channels;
			_isDynamic = true;

			_format = default;
			_format.wFormatTag = 1;
			_format.nChannels = (ushort)channels;
			_format.nSamplesPerSec = (uint)sampleRate;
			_format.wBitsPerSample = 16;
			_format.nBlockAlign = (ushort)(2 * _format.nChannels);
			_format.nAvgBytesPerSec = _format.nBlockAlign * _format.nSamplesPerSec;
			_format.cbSize = 0;

			_queuedBuffers = new List<IntPtr>();
			_queuedSizes = new List<uint>();

			InitDSPSettings(_format.nChannels);
		}

		// ReSharper disable once InconsistentlySynchronizedField
		public int PendingBufferCount => _queuedBuffers.Count;

		public override bool IsLooped
		{
			get => false;
			set
			{
				// No-op, DynamicSoundEffectInstance cannot be looped!
			}
		}

		public event EventHandler<EventArgs>? BufferNeeded;

		~DynamicSoundEffectInstance() =>
			// FIXME: ReRegisterForFinalize? -flibit
			Dispose();

		public TimeSpan GetSampleDuration(int sizeInBytes) => SoundEffect.GetSampleDuration(sizeInBytes, _sampleRate, _channels);

		public int GetSampleSizeInBytes(TimeSpan duration) => SoundEffect.GetSampleSizeInBytes(duration, _sampleRate, _channels);

		public override void Play()
		{
			// Wait! What if we need more buffers?
			Update();

			// Okay we're good
			base.Play();
			lock (FrameworkDispatcher.Streams)
			{
				FrameworkDispatcher.Streams.Add(this);
			}
		}

		public void SubmitBuffer(byte[] buffer) => SubmitBuffer(buffer, 0, buffer.Length);

		public void SubmitBuffer(byte[] buffer, int offset, int count)
		{
			var next = Marshal.AllocHGlobal(count);
			Marshal.Copy(buffer, offset, next, count);
			lock (_queuedBuffers)
			{
				_queuedBuffers.Add(next);
				if (State != SoundState.Stopped)
				{
					var buf = default(FAudio.FAudioBuffer);
					buf.AudioBytes = (uint)count;
					buf.pAudioData = next;
					buf.PlayLength = buf.AudioBytes /
					                 (uint)_channels /
					                 (uint)(_format.wBitsPerSample / 8);

					FAudio.FAudioSourceVoice_SubmitSourceBuffer(_handle, ref buf, IntPtr.Zero);
				}
				else
				{
					_queuedSizes.Add((uint)count);
				}
			}
		}

		public void SubmitFloatBufferEXT(float[] buffer) => SubmitFloatBufferEXT(buffer, 0, buffer.Length);

		public void SubmitFloatBufferEXT(float[] buffer, int offset, int count)
		{
			/* Float samples are the typical format received from decoders.
			 * We currently use this for the VideoPlayer.
			 * -flibit
			 */
			if (State != SoundState.Stopped && _format.wFormatTag == 1)
			{
				throw new InvalidOperationException("Submit a float buffer before Playing!");
			}

			_format.wFormatTag = 3;
			_format.wBitsPerSample = 32;
			_format.nBlockAlign = (ushort)(4 * _format.nChannels);
			_format.nAvgBytesPerSec = _format.nBlockAlign * _format.nSamplesPerSec;

			var next = Marshal.AllocHGlobal(count * sizeof(float));
			Marshal.Copy(buffer, offset, next, count);
			lock (_queuedBuffers)
			{
				_queuedBuffers.Add(next);
				if (State != SoundState.Stopped)
				{
					var buf = default(FAudio.FAudioBuffer);
					buf.AudioBytes = (uint)count * sizeof(float);
					buf.pAudioData = next;
					buf.PlayLength = buf.AudioBytes /
					                 (uint)_channels /
					                 (uint)(_format.wBitsPerSample / 8);

					FAudio.FAudioSourceVoice_SubmitSourceBuffer(_handle, ref buf, IntPtr.Zero);
				}
				else
				{
					_queuedSizes.Add((uint)count * sizeof(float));
				}
			}
		}

		internal void QueueInitialBuffers()
		{
			var buffer = default(FAudio.FAudioBuffer);
			lock (_queuedBuffers)
			{
				for (var i = 0; i < _queuedBuffers.Count; i += 1)
				{
					buffer.AudioBytes = _queuedSizes[i];
					buffer.pAudioData = _queuedBuffers[i];
					buffer.PlayLength = buffer.AudioBytes /
					                    (uint)_channels /
					                    (uint)(_format.wBitsPerSample / 8);

					FAudio.FAudioSourceVoice_SubmitSourceBuffer(_handle, ref buffer, IntPtr.Zero);
				}

				_queuedSizes.Clear();
			}
		}

		internal void ClearBuffers()
		{
			lock (_queuedBuffers)
			{
				foreach (var buf in _queuedBuffers)
				{
					Marshal.FreeHGlobal(buf);
				}

				_queuedBuffers.Clear();
				_queuedSizes.Clear();
			}
		}

		internal void Update()
		{
			if (State != SoundState.Playing)
			{
				// Shh, we don't need you right now...
				return;
			}

			if (_handle != IntPtr.Zero)
			{
				FAudio.FAudioSourceVoice_GetState(_handle, out var state, FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED);
				while (PendingBufferCount > state.BuffersQueued)
				{
					lock (_queuedBuffers)
					{
						Marshal.FreeHGlobal(_queuedBuffers[0]);
						_queuedBuffers.RemoveAt(0);
					}
				}
			}

			// Do we need even more buffers?
			for (var i = MinimumBufferCheck - PendingBufferCount;
				i > 0 && BufferNeeded != null;
				i -= 1)
			{
				BufferNeeded(this, EventArgs.Empty);
			}
		}
	}
}
