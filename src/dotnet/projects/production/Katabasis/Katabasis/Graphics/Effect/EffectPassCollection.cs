// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Katabasis
{
	public sealed class EffectPassCollection : IEnumerable<EffectPass>
	{
		private readonly List<EffectPass> _elements;

		internal EffectPassCollection(List<EffectPass> value) => _elements = value;

		public int Count => _elements.Count;

		public EffectPass? this[int index] => _elements[index];

		public EffectPass? this[string name]
		{
			get
			{
				// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
				foreach (EffectPass elem in _elements)
				{
					if (name.Equals(elem.Name, StringComparison.Ordinal))
					{
						return elem;
					}
				}

				return null; // FIXME: ArrayIndexOutOfBounds? -flibit
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => _elements.GetEnumerator();

		IEnumerator<EffectPass> IEnumerable<EffectPass>.GetEnumerator() => _elements.GetEnumerator();

		public List<EffectPass>.Enumerator GetEnumerator() => _elements.GetEnumerator();
	}
}
