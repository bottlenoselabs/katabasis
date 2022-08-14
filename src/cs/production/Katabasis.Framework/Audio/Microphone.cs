// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class Microphone
	{
		/* FIXME: This is what XNA4 aims for, but it _could_ be lower.
		 * Something worth looking at is falling back to lower sample
		 * rates in powers of two, i.e. 44100, 22050, 11025, etc.
		 * -flibit
		 */
		internal const int SamplerRate = 44100;

		internal static ReadOnlyCollection<Microphone>? MicList;
		private readonly uint _handle;
		public readonly string Name;

		private TimeSpan _bufferDuration;

		internal Microphone(uint id, string name)
		{
			_handle = id;
			Name = name;
			_bufferDuration = TimeSpan.FromSeconds(1.0);
			State = MicrophoneState.Stopped;
		}

		public static ReadOnlyCollection<Microphone> All => MicList ??= new ReadOnlyCollection<Microphone>(FNAPlatform.GetMicrophones());

		public static Microphone? Default => All.Count == 0 ? null : All[0];

		public TimeSpan BufferDuration
		{
			get => _bufferDuration;
			set
			{
				if (value.Milliseconds < 100 ||
				    value.Milliseconds > 1000 ||
				    value.Milliseconds % 10 != 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_bufferDuration = value;
			}
		}

		public bool IsHeadset =>
			// FIXME: I think this is just for Windows Phone? -flibit
			false;

		public int SampleRate => SamplerRate;

		public MicrophoneState State { get; private set; }

		public event EventHandler<EventArgs>? BufferReady;

		internal void CheckBuffer()
		{
			if (BufferReady != null &&
			    GetSampleDuration(FNAPlatform.GetMicrophoneQueuedBytes(_handle)) > _bufferDuration)
			{
				BufferReady(this, EventArgs.Empty);
			}
		}

		public int GetData(byte[] buffer) => GetData(buffer, 0, buffer.Length);

		public int GetData(byte[] buffer, int offset, int count)
		{
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentException(null, nameof(offset));
			}

			if (count <= 0 || offset + count > buffer.Length)
			{
				throw new ArgumentException(null, nameof(count));
			}

			return FNAPlatform.GetMicrophoneSamples(_handle, buffer, offset, count);
		}

		public TimeSpan GetSampleDuration(int sizeInBytes) => SoundEffect.GetSampleDuration(sizeInBytes, SampleRate, AudioChannels.Mono);

		public int GetSampleSizeInBytes(TimeSpan duration) => SoundEffect.GetSampleSizeInBytes(duration, SampleRate, AudioChannels.Mono);

		public void Start()
		{
			FNAPlatform.StartMicrophone(_handle);
			State = MicrophoneState.Started;
		}

		public void Stop()
		{
			FNAPlatform.StopMicrophone(_handle);
			State = MicrophoneState.Stopped;
		}
	}
}
