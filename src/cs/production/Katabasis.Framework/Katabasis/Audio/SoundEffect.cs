// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using bottlenoselabs;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.soundeffect.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public sealed unsafe class SoundEffect : IDisposable
	{
		internal FAudio.FAudioBuffer _handle;
		internal IntPtr _formatPtr;
		internal ushort _channels;
		internal uint _sampleRate;
		internal uint _loopLength;
		internal uint _loopStart;
		internal List<WeakReference> Instances = new();

		internal unsafe SoundEffect(
			string name,
			byte[] buffer,
			int offset,
			int count,
			byte[]? extraData,
			ushort wFormatTag,
			ushort nChannels,
			uint nSamplesPerSec,
			uint nAvgBytesPerSec,
			ushort nBlockAlign,
			ushort wBitsPerSample,
			int loopStart,
			int loopLength)
		{
			Device();
			Name = name;
			_channels = nChannels;
			_sampleRate = nSamplesPerSec;
			_loopStart = (uint)loopStart;
			_loopLength = (uint)loopLength;

			/* Buffer format */
			if (extraData == null)
			{
				_formatPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FAudio.FAudioWaveFormatEx)));
			}
			else
			{
				_formatPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FAudio.FAudioWaveFormatEx)) + extraData.Length);
				Marshal.Copy(
					extraData,
					0,
					_formatPtr + Marshal.SizeOf(typeof(FAudio.FAudioWaveFormatEx)),
					extraData.Length);
			}

			FAudio.FAudioWaveFormatEx* pcm = (FAudio.FAudioWaveFormatEx*) _formatPtr;
			pcm->wFormatTag = wFormatTag;
			pcm->nChannels = nChannels;
			pcm->nSamplesPerSec = nSamplesPerSec;
			pcm->nAvgBytesPerSec = nAvgBytesPerSec;
			pcm->nBlockAlign = nBlockAlign;
			pcm->wBitsPerSample = wBitsPerSample;
			pcm->cbSize = (ushort) (extraData?.Length ?? 0);

			/* Easy stuff */
			_handle = default;
			_handle.Flags = FAudio.FAUDIO_END_OF_STREAM;
			_handle.pContext = (void*)IntPtr.Zero;

			/* Buffer data */
			_handle.AudioBytes = (uint)count;
			_handle.pAudioData = (byte*)Marshal.AllocHGlobal(count);
			Marshal.Copy(
				buffer,
				offset,
				(IntPtr)_handle.pAudioData,
				count);

			/* Play regions */
			_handle.PlayBegin = 0;
			if (wFormatTag == 1)
			{
				_handle.PlayLength = (uint)(
					count /
					nChannels /
					(wBitsPerSample / 8));
			}
			else if (wFormatTag == 2)
			{
				_handle.PlayLength = (uint)(count / nBlockAlign * ((nBlockAlign / nChannels) - 6) * 2);
			}
			else if (wFormatTag == 0x166)
			{
				var xma2 = (FAudio.FAudioXMA2WaveFormatEx*) _formatPtr;
				// dwSamplesEncoded / nChannels / (wBitsPerSample / 8) doesn't always (if ever?) match up.
				_handle.PlayLength = xma2->dwPlayLength;
			}

			/* Set by Instances! */
			_handle.LoopBegin = 0;
			_handle.LoopLength = 0;
			_handle.LoopCount = 0;
		}

		public SoundEffect(
			byte[] buffer,
			int sampleRate,
			AudioChannels channels)
			: this(
				string.Empty,
				buffer,
				0,
				buffer.Length,
				null,
				1,
				(ushort)channels,
				(uint)sampleRate,
				(uint)(sampleRate * (ushort)channels * 2),
				(ushort)((ushort)channels * 2),
				16,
				0,
				0)
		{
		}

		public SoundEffect(
			byte[] buffer,
			int offset,
			int count,
			int sampleRate,
			AudioChannels channels,
			int loopStart,
			int loopLength)
			: this(
				string.Empty,
				buffer,
				offset,
				count,
				null,
				1,
				(ushort)channels,
				(uint)sampleRate,
				(uint)(sampleRate * (ushort)channels * 2),
				(ushort)((ushort)channels * 2),
				16,
				loopStart,
				loopLength)
		{
		}

		public TimeSpan Duration => TimeSpan.FromSeconds(_handle.PlayLength / (double)_sampleRate);

		public bool IsDisposed { get; private set; }

		public string Name { get; set; }

		public static float MasterVolume
		{
			get
			{
				float result;
				FAudio.FAudioVoice_GetVolume((FAudio.FAudioVoice*)Device().MasterVoice, &result);
				return result;
			}
			set =>
				FAudio.FAudioVoice_SetVolume((FAudio.FAudioVoice*)Device().MasterVoice, value, 0);
		}

		public static float DistanceScale
		{
			get => Device().CurveDistanceScaler;
			set
			{
				if (value <= 0.0f)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				Device().CurveDistanceScaler = value;
			}
		}

		public static float DopplerScale
		{
			get => Device().DopplerScaleAudioContext;
			set
			{
				if (value < 0.0f)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				Device().DopplerScaleAudioContext = value;
			}
		}

		public static float SpeedOfSound
		{
			get => Device().SpeedOfSoundAudioContext;
			set
			{
				FAudioContext dev = Device();
				dev.SpeedOfSoundAudioContext = value;
				FAudio.F3DAudioInitialize(
					dev.DeviceDetails.OutputFormat.dwChannelMask,
					dev.SpeedOfSoundAudioContext,
					dev.Handle3D);
			}
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			/* FIXME: Is it ironic that we're generating
				 * garbage with ToArray while cleaning up after
				 * the program's leaks?
				 * -flibit
				 */
			foreach (WeakReference instance in Instances.ToArray())
			{
				var target = instance.Target;
				if (target != null)
				{
					(target as IDisposable)!.Dispose();
				}
			}

			Instances.Clear();
			Marshal.FreeHGlobal(_formatPtr);
			Marshal.FreeHGlobal((IntPtr)_handle.pAudioData);
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		~SoundEffect()
		{
			if (Instances.Count > 0)
			{
				// STOP LEAKING YOUR INSTANCES, ARGH
				GC.ReRegisterForFinalize(this);
				return;
			}

			Dispose();
		}

		public bool Play() => Play(1.0f, 0.0f, 0.0f);

		public bool Play(float volume, float pitch, float pan)
		{
			SoundEffectInstance instance = new(this);
			instance.Volume = volume;
			instance.Pitch = pitch;
			instance.Pan = pan;
			instance.Play();
			if (instance.State != SoundState.Playing)
			{
				// Ran out of AL sources, probably.
				instance.Dispose();
				return false;
			}

			return true;
		}

		public SoundEffectInstance CreateInstance() => new(this);

		public static TimeSpan GetSampleDuration(
			int sizeInBytes,
			int sampleRate,
			AudioChannels channels)
		{
			sizeInBytes /= 2; // 16-bit PCM!
			// ReSharper disable once PossibleLossOfFraction
			var ms = (int)(sizeInBytes / (int)channels / (sampleRate / 1000.0f));
			return new TimeSpan(0, 0, 0, 0, ms);
		}

		public static int GetSampleSizeInBytes(TimeSpan duration, int sampleRate, AudioChannels channels) =>
			(int)(duration.TotalSeconds * sampleRate * (int)channels * 2);

		public static SoundEffect FromFile(string filePath)
		{
			var stream = TitleContainer.OpenStream(filePath);
			return FromStream(stream);
		}

		public static SoundEffect FromStream(Stream stream)
		{
			// Sample data
			byte[] data;

			// WaveFormatEx data
			ushort wFormatTag;
			ushort nChannels;
			uint nSamplesPerSec;
			uint nAvgBytesPerSec;
			ushort nBlockAlign;
			ushort wBitsPerSample;
			// ushort cbSize;

			var samplerLoopStart = 0;
			var samplerLoopEnd = 0;

			using (BinaryReader reader = new(stream))
			{
				// RIFF Signature
				string signature = new(reader.ReadChars(4));
				if (signature != "RIFF")
				{
					throw new NotSupportedException("Specified stream is not a wave file.");
				}

				reader.ReadUInt32(); // Riff Chunk Size

				string format = new(reader.ReadChars(4));
				if (format != "WAVE")
				{
					throw new NotSupportedException("Specified stream is not a wave file.");
				}

				// WAVE Header
				string formatSignature = new(reader.ReadChars(4));
				while (formatSignature != "fmt ")
				{
					reader.ReadBytes(reader.ReadInt32());
					formatSignature = new string(reader.ReadChars(4));
				}

				var formatChunkSize = reader.ReadInt32();

				wFormatTag = reader.ReadUInt16();
				nChannels = reader.ReadUInt16();
				nSamplesPerSec = reader.ReadUInt32();
				nAvgBytesPerSec = reader.ReadUInt32();
				nBlockAlign = reader.ReadUInt16();
				wBitsPerSample = reader.ReadUInt16();

				// Reads residual bytes
				if (formatChunkSize > 16)
				{
					reader.ReadBytes(formatChunkSize - 16);
				}

				// data Signature
				string dataSignature = new(reader.ReadChars(4));
				while (dataSignature.ToLowerInvariant() != "data")
				{
					reader.ReadBytes(reader.ReadInt32());
					dataSignature = new string(reader.ReadChars(4));
				}

				if (dataSignature != "data")
				{
					throw new NotSupportedException("Specified wave file is not supported.");
				}

				var waveDataLength = reader.ReadInt32();
				data = reader.ReadBytes(waveDataLength);

				// Scan for other chunks
				while (reader.PeekChar() != -1)
				{
					byte[] chunkIDBytes = reader.ReadBytes(4);
					if (chunkIDBytes.Length < 4)
					{
						break; // EOL!
					}

					byte[] chunkSizeBytes = reader.ReadBytes(4);
					if (chunkSizeBytes.Length < 4)
					{
						break; // EOL!
					}

					var chunkID = BitConverter.ToInt32(chunkIDBytes, 0);
					var chunkDataSize = BitConverter.ToInt32(chunkSizeBytes, 0);
					// "smpl", Sampler Chunk Found
					if (chunkID == 0x736D706C)
					{
						reader.ReadUInt32(); // Manufacturer
						reader.ReadUInt32(); // Product
						reader.ReadUInt32(); // Sample Period
						reader.ReadUInt32(); // MIDI Unity Note
						reader.ReadUInt32(); // MIDI Pitch Fraction
						reader.ReadUInt32(); // SMPTE Format
						reader.ReadUInt32(); // SMPTE Offset
						var numSampleLoops = reader.ReadUInt32();
						var samplerData = reader.ReadInt32();

						for (var i = 0; i < numSampleLoops; i += 1)
						{
							reader.ReadUInt32(); // Cue Point ID
							reader.ReadUInt32(); // Type
							var start = reader.ReadInt32();
							var end = reader.ReadInt32();
							reader.ReadUInt32(); // Fraction
							reader.ReadUInt32(); // Play Count

							if (i == 0)
							{
								// Grab loopStart and loopEnd from first sample loop
								samplerLoopStart = start;
								samplerLoopEnd = end;
							}
						}

						if (samplerData != 0)
						{
							// Read Sampler Data if it exists
							reader.ReadBytes(samplerData);
						}
					}
					else
					{
						// Read unwanted chunk data and try again
						reader.ReadBytes(chunkDataSize);
					}
				}

				// End scan
			}

			return new SoundEffect(
				string.Empty,
				data,
				0,
				data.Length,
				null,
				wFormatTag,
				nChannels,
				nSamplesPerSec,
				nAvgBytesPerSec,
				nBlockAlign,
				wBitsPerSample,
				samplerLoopStart,
				samplerLoopEnd - samplerLoopStart);
		}

		internal static FAudioContext Device()
		{
			if (FAudioContext.Context != null)
			{
				return FAudioContext.Context;
			}

			FAudioContext.Create();
			if (FAudioContext.Context == null)
			{
				throw new NoAudioHardwareException();
			}

			return FAudioContext.Context;
		}

		internal class FAudioContext
		{
			public static FAudioContext? Context;
			public readonly FAudio.FAudioDeviceDetails DeviceDetails;

			public readonly IntPtr Handle;
			public readonly FAudio.F3DAUDIO_HANDLE Handle3D;
			public readonly IntPtr MasterVoice;

			private FAudio.FAudioVoiceSends _reverbSends;

			public float CurveDistanceScaler;
			public float DopplerScaleAudioContext;

			public IntPtr ReverbVoice;
			public float SpeedOfSoundAudioContext;

			private FAudioContext(IntPtr ctx, uint devices)
			{
				Handle = ctx;

				uint i;
				for (i = 0; i < devices; i += 1)
				{
					FAudio.FAudioDeviceDetails result;
					FAudio.FAudio_GetDeviceDetails((FAudio.FAudioSystem*)Handle, i, &result);
					DeviceDetails = result;
					if ((DeviceDetails.Role & FAudio.FAudioDeviceRole.FAudioDefaultGameDevice) ==
					    FAudio.FAudioDeviceRole.FAudioDefaultGameDevice)
					{
						break;
					}
				}

				if (i == devices)
				{
					i = 0; /* Oh well. */
					FAudio.FAudioDeviceDetails result;
					FAudio.FAudio_GetDeviceDetails((FAudio.FAudioSystem*)Handle, i, &result);
					DeviceDetails = result;
				}

				FAudio.FAudioMasteringVoice* masterVoice;
				if (FAudio.FAudio_CreateMasteringVoice(
					(FAudio.FAudioSystem*)Handle,
					&masterVoice,
					FAudio.FAUDIO_DEFAULT_CHANNELS,
					FAudio.FAUDIO_DEFAULT_SAMPLERATE,
					0,
					i,
					(FAudio.FAudioEffectChain*)IntPtr.Zero) != 0)
				{
					FAudio.FAudio_Release((FAudio.FAudioSystem*)ctx);
					Handle = IntPtr.Zero;
					FNALoggerEXT.LogError!("Failed to create mastering voice!");
					return;
				}

				MasterVoice = (IntPtr)masterVoice;

				CurveDistanceScaler = 1.0f;
				DopplerScaleAudioContext = 1.0f;
				SpeedOfSoundAudioContext = 343.5f;
				Handle3D.Data = (byte*)Marshal.AllocHGlobal(FAudio.F3DAUDIO_HANDLE_BYTESIZE);
				FAudio.F3DAudioInitialize(
					DeviceDetails.OutputFormat.dwChannelMask,
					SpeedOfSoundAudioContext,
					Handle3D);

				Context = this;
			}

			public void Dispose()
			{
				if (ReverbVoice != IntPtr.Zero)
				{
					FAudio.FAudioVoice_DestroyVoice((FAudio.FAudioVoice*)ReverbVoice);
					ReverbVoice = IntPtr.Zero;
					Marshal.FreeHGlobal((IntPtr)_reverbSends.pSends);
				}

				if (MasterVoice != IntPtr.Zero)
				{
					FAudio.FAudioVoice_DestroyVoice((FAudio.FAudioVoice*)MasterVoice);
				}

				if (Handle != IntPtr.Zero)
				{
					FAudio.FAudio_Release((FAudio.FAudioSystem*)Handle);
				}

				Context = null;
			}

			public void AttachReverb(IntPtr voice)
			{
				// Only create a reverb voice if they ask for it!
				if (ReverbVoice == IntPtr.Zero)
				{
					FAudio.FAPO* reverb;
					FAudio.FAudioCreateReverb(&reverb, 0);

					var chainPtr = Marshal.AllocHGlobal(
						Marshal.SizeOf(typeof(FAudio.FAudioEffectChain)));

					var reverbChain = (FAudio.FAudioEffectChain*)chainPtr;
					reverbChain->EffectCount = 1;
					reverbChain->pEffectDescriptors = (FAudio.FAudioEffectDescriptor*)Marshal.AllocHGlobal(
						Marshal.SizeOf(typeof(FAudio.FAudioEffectDescriptor)));

					var reverbDesc = reverbChain->pEffectDescriptors;

					reverbDesc->InitialState = 1;
					reverbDesc->OutputChannels = (uint)(DeviceDetails.OutputFormat.Format.nChannels == 6 ? 6 : 1);
					reverbDesc->pEffect = reverb;

					FAudio.FAudioSubmixVoice* reverbVoice;
					FAudio.FAudio_CreateSubmixVoice(
						(FAudio.FAudioSystem*)Handle,
						&reverbVoice,
						1, /* Reverb will be omnidirectional */
						DeviceDetails.OutputFormat.Format.nSamplesPerSec,
						0,
						0,
						(FAudio.FAudioVoiceSends*)IntPtr.Zero,
						(FAudio.FAudioEffectChain*)chainPtr);
					ReverbVoice = (IntPtr)reverbVoice;

					FAudio.FAPOBase_Release((FAudio.FAPOBase*)reverb);

					Marshal.FreeHGlobal((IntPtr)reverbChain->pEffectDescriptors);
					Marshal.FreeHGlobal(chainPtr);

					// ReSharper disable once CommentTypo
					// Defaults based on FAUDIOFX_I3DL2_PRESET_GENERIC
					var rvbParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FAudio.FAudioFXReverbParameters)));
					var rvbParams = (FAudio.FAudioFXReverbParameters*)rvbParamsPtr;
					rvbParams->WetDryMix = 100.0f;
					rvbParams->ReflectionsDelay = 7;
					rvbParams->ReverbDelay = 11;
					rvbParams->RearDelay = FAudio.FAUDIOFX_REVERB_DEFAULT_REAR_DELAY;
					rvbParams->PositionLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION;
					rvbParams->PositionRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION;
					rvbParams->PositionMatrixLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX;
					rvbParams->PositionMatrixRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX;
					rvbParams->EarlyDiffusion = 15;
					rvbParams->LateDiffusion = 15;
					rvbParams->LowEQGain = 8;
					rvbParams->LowEQCutoff = 4;
					rvbParams->HighEQGain = 8;
					rvbParams->HighEQCutoff = 6;
					rvbParams->RoomFilterFreq = 5000f;
					rvbParams->RoomFilterMain = -10f;
					rvbParams->RoomFilterHF = -1f;
					rvbParams->ReflectionsGain = -26.0200005f;
					rvbParams->ReverbGain = 10.0f;
					rvbParams->DecayTime = 1.49000001f;
					rvbParams->Density = 100.0f;
					rvbParams->RoomSize = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_SIZE;
					FAudio.FAudioVoice_SetEffectParameters(
						(FAudio.FAudioVoice*)ReverbVoice,
						0,
						(void*)rvbParamsPtr,
						(uint)Marshal.SizeOf(typeof(FAudio.FAudioFXReverbParameters)),
						0);

					Marshal.FreeHGlobal(rvbParamsPtr);

					_reverbSends = default;
					_reverbSends.SendCount = 2;
					_reverbSends.pSends = (FAudio.FAudioSendDescriptor*)Marshal.AllocHGlobal(
						2 * Marshal.SizeOf(typeof(FAudio.FAudioSendDescriptor)));

					var sendDesc = (FAudio.FAudioSendDescriptor*)_reverbSends.pSends;
					sendDesc[0].Flags = 0;
					sendDesc[0].pOutputVoice = (FAudio.FAudioVoice*)MasterVoice;
					sendDesc[1].Flags = 0;
					sendDesc[1].pOutputVoice = (FAudio.FAudioVoice*)ReverbVoice;
				}

				// Oh hey here's where we actually attach it
				FAudio.FAudioVoice_SetOutputVoices((FAudio.FAudioVoice*)voice, (FAudio.FAudioVoiceSends*)Unsafe.AsPointer(ref _reverbSends));
			}

			public static void Create()
			{
				FAudio.FAudioSystem* ctx;
				try
				{
					FAudio.FAudioCreate(&ctx, 0, FAudio.FAUDIO_DEFAULT_PROCESSOR);
				}
				catch
				{
					/* FAudio is missing, bail! */
					return;
				}

				uint devices;
				FAudio.FAudio_GetDeviceCount(ctx, &devices);
				if (devices == 0)
				{
					/* No sound cards, bail! */
					FAudio.FAudio_Release(ctx);
					return;
				}

				FAudioContext context = new((IntPtr)ctx, devices);

				if (context.Handle == IntPtr.Zero)
				{
					/* Sound card failed to configure, bail! */
					context.Dispose();
					return;
				}

				Context = context;
			}
		}
	}
}
