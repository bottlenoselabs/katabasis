// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public struct KeyboardState
	{
		public KeyState this[Keys key] => InternalGetKey(key) ? KeyState.Down : KeyState.Up;

		// Array of 256 bits:
		private uint _keys0;
		private uint _keys1;
		private uint _keys2;
		private uint _keys3;
		private uint _keys4;
		private uint _keys5;
		private uint _keys6;
		private uint _keys7;

		public KeyboardState(params Keys[]? keys)
		{
			_keys0 = 0;
			_keys1 = 0;
			_keys2 = 0;
			_keys3 = 0;
			_keys4 = 0;
			_keys5 = 0;
			_keys6 = 0;
			_keys7 = 0;

			if (keys == null)
			{
				return;
			}

			foreach (var k in keys)
			{
				InternalSetKey(k);
			}
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		internal KeyboardState(List<Keys>? keys)
		{
			_keys0 = 0;
			_keys1 = 0;
			_keys2 = 0;
			_keys3 = 0;
			_keys4 = 0;
			_keys5 = 0;
			_keys6 = 0;
			_keys7 = 0;

			if (keys == null)
			{
				return;
			}

			foreach (var k in keys)
			{
				InternalSetKey(k);
			}
		}

		public bool IsKeyDown(Keys key) => InternalGetKey(key);

		public bool IsKeyUp(Keys key) => !InternalGetKey(key);

		public Keys[] GetPressedKeys()
		{
			var count = CountBits(_keys0) +
			            CountBits(_keys1) +
			            CountBits(_keys2) +
			            CountBits(_keys3) +
			            CountBits(_keys4) +
			            CountBits(_keys5) +
			            CountBits(_keys6) +
			            CountBits(_keys7);

			if (count == 0)
			{
				return Array.Empty<Keys>();
			}

			Keys[] keys = new Keys[count];

			var index = 0;
			if (_keys0 != 0)
			{
				index = AddKeysToArray(_keys0, 0 * 32, keys, index);
			}

			if (_keys1 != 0)
			{
				index = AddKeysToArray(_keys1, 1 * 32, keys, index);
			}

			if (_keys2 != 0)
			{
				index = AddKeysToArray(_keys2, 2 * 32, keys, index);
			}

			if (_keys3 != 0)
			{
				index = AddKeysToArray(_keys3, 3 * 32, keys, index);
			}

			if (_keys4 != 0)
			{
				index = AddKeysToArray(_keys4, 4 * 32, keys, index);
			}

			if (_keys5 != 0)
			{
				index = AddKeysToArray(_keys5, 5 * 32, keys, index);
			}

			if (_keys6 != 0)
			{
				index = AddKeysToArray(_keys6, 6 * 32, keys, index);
			}

			if (_keys7 != 0)
			{
				AddKeysToArray(_keys7, 7 * 32, keys, index);
			}

			return keys;
		}

		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
		public override int GetHashCode() => (int)(_keys0 ^ _keys1 ^ _keys2 ^ _keys3 ^ _keys4 ^ _keys5 ^ _keys6 ^ _keys7);

		public static bool operator ==(KeyboardState a, KeyboardState b) =>
			a._keys0 == b._keys0 &&
			a._keys1 == b._keys1 &&
			a._keys2 == b._keys2 &&
			a._keys3 == b._keys3 &&
			a._keys4 == b._keys4 &&
			a._keys5 == b._keys5 &&
			a._keys6 == b._keys6 &&
			a._keys7 == b._keys7;

		public static bool operator !=(KeyboardState a, KeyboardState b) => !(a == b);

		public override bool Equals(object? obj) => obj is KeyboardState state && this == state;

		private static uint CountBits(uint v)
		{
			// http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
			v -= (v >> 1) & 0x55555555; // reuse input as temporary
			v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
			return (((v + (v >> 4)) & 0xF0F0F0F) * 0x1010101) >> 24; // count
		}

		private static int AddKeysToArray(uint keys, int offset, Keys[] pressedKeys, int index)
		{
			for (var i = 0; i < 32; i += 1)
			{
				if ((keys & (1 << i)) != 0)
				{
					pressedKeys[index++] = (Keys)(offset + i);
				}
			}

			return index;
		}

		private bool InternalGetKey(Keys key)
		{
			var mask = 1U << ((int)key & 0x1f);

			uint element;
			switch ((int)key >> 5)
			{
				case 0:
					element = _keys0;
					break;
				case 1:
					element = _keys1;
					break;
				case 2:
					element = _keys2;
					break;
				case 3:
					element = _keys3;
					break;
				case 4:
					element = _keys4;
					break;
				case 5:
					element = _keys5;
					break;
				case 6:
					element = _keys6;
					break;
				case 7:
					element = _keys7;
					break;
				default:
					element = 0;
					break;
			}

			return (element & mask) != 0;
		}

		private void InternalSetKey(Keys key)
		{
			var mask = 1U << ((int)key & 0x1f);
			switch ((int)key >> 5)
			{
				case 0:
					_keys0 |= mask;
					break;
				case 1:
					_keys1 |= mask;
					break;
				case 2:
					_keys2 |= mask;
					break;
				case 3:
					_keys3 |= mask;
					break;
				case 4:
					_keys4 |= mask;
					break;
				case 5:
					_keys5 |= mask;
					break;
				case 6:
					_keys6 |= mask;
					break;
				case 7:
					_keys7 |= mask;
					break;
			}
		}
	}
}
