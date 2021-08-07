// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Collections.Generic;

namespace Katabasis
{
	public sealed class MediaQueue
	{
		private readonly List<Song> _songs = new();

		internal MediaQueue() => ActiveSongIndex = -1;

		public Song? ActiveSong
		{
			get
			{
				if (_songs.Count == 0 || ActiveSongIndex < 0)
				{
					return null;
				}

				return _songs[ActiveSongIndex];
			}
		}

		public int ActiveSongIndex { get; set; }

		public int Count => _songs.Count;

		public Song this[int index] => _songs[index];

		internal void Add(Song song) => _songs.Add(song);

		internal void Clear() => _songs.Clear();
	}
}
