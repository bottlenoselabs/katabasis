// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Generic;

namespace Ankura
{
    public sealed class MediaQueue
    {
        private readonly List<Song> _songs = new List<Song>();

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

        internal MediaQueue()
        {
            ActiveSongIndex = -1;
        }

        internal void Add(Song song)
        {
            _songs.Add(song);
        }

        internal void Clear()
        {
            _songs.Clear();
        }
    }
}
