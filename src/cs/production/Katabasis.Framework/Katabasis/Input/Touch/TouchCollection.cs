// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Katabasis
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchcollection.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public readonly struct TouchCollection : IList<TouchLocation>
	{
		private readonly List<TouchLocation> _touches;

		public int Count => _touches.Count;

		public bool IsConnected => TouchPanel.TouchDeviceExists;

		public bool IsReadOnly => true;

		public TouchLocation this[int index]
		{
			get => _touches[index];
			set =>
				// This will cause a runtime exception
				_touches[index] = value;
		}

		public TouchCollection(TouchLocation[] touches) => _touches = new List<TouchLocation>(touches);

		/* Since the collection is always readonly, using any
		 * method that attempts to modify touches will result
		 * in a System.NotSupportedException at runtime.
		 */

		public void Add(TouchLocation item) => _touches.Add(item);

		public void Clear() => _touches.Clear();

		public bool Contains(TouchLocation item) => _touches.Contains(item);

		public void CopyTo(TouchLocation[] array, int arrayIndex) => _touches.CopyTo(array, arrayIndex);

		public bool FindById(int id, out TouchLocation touchLocation)
		{
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (var touch in _touches)
			{
				if (touch.Id == id)
				{
					touchLocation = touch;
					return true;
				}
			}

			touchLocation = new TouchLocation(
				-1,
				TouchLocationState.Invalid,
				Vector2.Zero);

			return false;
		}

		public Enumerator GetEnumerator() => new(this);

		public int IndexOf(TouchLocation item) => _touches.IndexOf(item);

		public void Insert(int index, TouchLocation item) => _touches.Insert(index, item);

		public bool Remove(TouchLocation item) => _touches.Remove(item);

		public void RemoveAt(int index) => _touches.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

		IEnumerator<TouchLocation> IEnumerable<TouchLocation>.GetEnumerator() => new Enumerator(this);

		// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchcollection.enumerator.aspx
		public struct Enumerator : IEnumerator<TouchLocation>
		{
			private readonly TouchCollection _collection;
			private int _position;

			internal Enumerator(TouchCollection collection)
			{
				_collection = collection;
				_position = -1;
			}

			public TouchLocation Current => _collection[_position];

			public bool MoveNext()
			{
				_position += 1;
				return _position < _collection.Count;
			}

			public void Dispose()
			{
			}

			object IEnumerator.Current => _collection[_position];

			void IEnumerator.Reset() => _position = -1;
		}
	}
}
