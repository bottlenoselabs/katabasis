// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Ankura
{
    public sealed class SongCollection : IEnumerable<Song>, IDisposable
    {
        private readonly List<Song> _list;

        public Song this[int index] => _list[index];

        public int Count => _list.Count;

        public bool IsDisposed { get; private set; }

        public IEnumerator<Song> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        internal SongCollection(List<Song> songs)
        {
            _list = songs;
            IsDisposed = false;
        }

        public void Dispose()
        {
            _list.Clear();
            IsDisposed = true;
        }
    }
}
