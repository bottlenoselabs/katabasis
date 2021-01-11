// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Katabasis
{
	public sealed class SongCollection : IEnumerable<Song>, IDisposable
	{
		private readonly List<Song> _list;

		internal SongCollection(List<Song> songs)
		{
			_list = songs;
			IsDisposed = false;
		}

		public Song this[int index] => _list[index];

		public int Count => _list.Count;

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			_list.Clear();
			IsDisposed = true;
		}

		public IEnumerator<Song> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
	}
}
