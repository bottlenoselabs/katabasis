using System;

namespace bottlenoselabs.Katabasis.Samples;

public static class Sounds
{
	private static readonly Random Rand = new();
	private static AudioSource _source = null!;
	private static int _jingleCounter;

	public static void Load()
	{
		_source = new AudioSource();
	}

	public static void Reset()
	{
		_jingleCounter = 0;
	}

	public static void PlayBallHitsPaddle()
	{
		PlaySoundWave(220.0f, TimeSpan.FromMilliseconds(50), WaveType.Sin, 0.3f);
	}

	public static void PlayPlayerGainsPoint()
	{
		PlaySoundWave(440.0f, TimeSpan.FromMilliseconds(50), WaveType.Square, 0.3f);
	}

	public static void TryPlayResetJingle()
	{
		// use jingle counter as a timeline to play notes
		_jingleCounter++;

		const int speed = 7; // play a short A minor chord
		var duration = TimeSpan.FromMilliseconds(100);
		switch (_jingleCounter)
		{
			case speed * 1:
				PlaySoundWave(440.0f, duration, WaveType.Sin, 0.2f);
				break;
			case speed * 2:
				PlaySoundWave(523.25f, duration, WaveType.Sin, 0.2f);
				break;
			case speed * 3:
				PlaySoundWave(659.25f, duration, WaveType.Sin, 0.2f);
				break;
			case speed * 4:
				PlaySoundWave(783.99f, duration, WaveType.Sin, 0.2f);
				break;
			// only play this jingle once per game
			case > speed * 4:
				_jingleCounter = int.MaxValue - 1;
				break;
		}
	}

	private static void PlaySoundWave(double freq, TimeSpan duration, WaveType wt, float volume)
	{
		var src = _source;
		src.Instance.Stop();

		src.BufferSize = src.Instance.GetSampleSizeInBytes(duration);
		src.Buffer = new byte[src.BufferSize];

		var size = src.BufferSize - 1;
		for (var i = 0; i < size; i += 2)
		{
			var time = (double)src.TotalTime / AudioSource.SampleRate;

			short currentSample = wt switch
			{
				WaveType.Sin => (short)(Math.Sin(2 * Math.PI * freq * time) * short.MaxValue * volume),
				WaveType.Tan => (short)(Math.Tan(2 * Math.PI * freq * time) * short.MaxValue * volume),
				WaveType.Square => (short)(Math.Sign(Math.Sin(2 * Math.PI * freq * time)) * (double)short.MaxValue * volume),
				WaveType.Noise => (short)(Rand.Next(-short.MaxValue, short.MaxValue) * volume),
				_ => 0
			};

			src.Buffer[i] = (byte)(currentSample & 0xFF);
			src.Buffer[i + 1] = (byte)(currentSample >> 8);
			src.TotalTime += 2;
		}

		src.Instance.SubmitBuffer(src.Buffer);
		src.Instance.Play();
	}

	private sealed class AudioSource
	{
		public const int SampleRate = 48000;
		public readonly DynamicSoundEffectInstance Instance;
		public byte[] Buffer;
		public int BufferSize;
		public int TotalTime;

		public AudioSource()
		{
			Instance = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
			BufferSize = Instance.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(500));
			Buffer = new byte[BufferSize];
			Instance.Volume = 0.4f;
			Instance.IsLooped = false;
		}
	}
}
