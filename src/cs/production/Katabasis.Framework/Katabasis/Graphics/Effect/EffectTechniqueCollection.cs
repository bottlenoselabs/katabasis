// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Katabasis
{
	public sealed class EffectTechniqueCollection : IEnumerable<EffectTechnique>, IEnumerable
	{
		private readonly List<EffectTechnique> _elements;

		internal EffectTechniqueCollection(List<EffectTechnique> value) => _elements = value;

		public int Count => _elements.Count;

		public EffectTechnique? this[int index] => _elements[index];

		public EffectTechnique? this[string name]
		{
			get
			{
				foreach (EffectTechnique elem in _elements)
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

		IEnumerator<EffectTechnique> IEnumerable<EffectTechnique>.GetEnumerator() => _elements.GetEnumerator();

		public List<EffectTechnique>.Enumerator GetEnumerator() => _elements.GetEnumerator();
	}
}
