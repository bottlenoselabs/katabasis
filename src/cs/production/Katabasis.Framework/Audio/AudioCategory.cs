// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using static bottlenoselabs.FAudio;

namespace bottlenoselabs.Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.audiocategory.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public readonly unsafe struct AudioCategory : IEquatable<AudioCategory>
	{
		private readonly AudioEngine _parent;
		private readonly ushort _index;

		public string Name { get; }

		internal AudioCategory(
			AudioEngine engine,
			ushort category,
			string name)
		{
			_parent = engine;
			_index = category;
			Name = name;
		}

		public void Pause()
		{
			lock (_parent._gcSync)
			{
				if (_parent.IsDisposed)
				{
					return;
				}

				FACTAudioEngine_Pause(_parent._handle, _index, 1);
			}
		}

		public void Resume()
		{
			lock (_parent._gcSync)
			{
				if (_parent.IsDisposed)
				{
					return;
				}

				FACTAudioEngine_Pause(_parent._handle, _index, 0);
			}
		}

		public void SetVolume(float volume)
		{
			lock (_parent._gcSync)
			{
				if (_parent.IsDisposed)
				{
					return;
				}

				FACTAudioEngine_SetVolume(_parent._handle, _index, volume);
			}
		}

		public void Stop(AudioStopOptions options)
		{
			lock (_parent._gcSync)
			{
				if (_parent.IsDisposed)
				{
					return;
				}

				FACTAudioEngine_Stop(
					_parent._handle,
					_index,
					options == AudioStopOptions.Immediate ? FACT_FLAG_STOP_IMMEDIATE : FACT_FLAG_STOP_RELEASE);
			}
		}

		public override int GetHashCode() => Name.GetHashCode();

		public bool Equals(AudioCategory other) => GetHashCode() == other.GetHashCode();

		public override bool Equals(object? obj)
		{
			if (obj is AudioCategory category)
			{
				return Equals(category);
			}

			return false;
		}

		public static bool operator ==(AudioCategory value1, AudioCategory value2) => value1.Equals(value2);

		public static bool operator !=(AudioCategory value1, AudioCategory value2) => !value1.Equals(value2);
	}
}
