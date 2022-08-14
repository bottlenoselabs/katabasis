// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace bottlenoselabs.Katabasis
{
	public sealed class EffectAnnotationCollection : IEnumerable<EffectAnnotation>
	{
		private readonly List<EffectAnnotation> _elements;

		internal EffectAnnotationCollection(List<EffectAnnotation> value) => _elements = value;

		public int Count => _elements.Count;

		public EffectAnnotation? this[int index] => _elements[index];

		public EffectAnnotation? this[string name]
		{
			get
			{
				// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
				foreach (EffectAnnotation elem in _elements)
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

		IEnumerator<EffectAnnotation> IEnumerable<EffectAnnotation>.GetEnumerator() => _elements.GetEnumerator();

		public List<EffectAnnotation>.Enumerator GetEnumerator() => _elements.GetEnumerator();
	}
}
