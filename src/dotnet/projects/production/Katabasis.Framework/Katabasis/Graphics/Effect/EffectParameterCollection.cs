// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Will gut Mojo shader soon.")]
	public sealed class EffectParameterCollection : IEnumerable<EffectParameter>
	{
		private readonly List<EffectParameter> _elements;

		internal EffectParameterCollection(List<EffectParameter> value) => _elements = value;

		public int Count => _elements.Count;

		public EffectParameter? this[int index] => _elements[index];

		public EffectParameter? this[string name]
		{
			get
			{
				// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
				foreach (EffectParameter elem in _elements)
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

		IEnumerator<EffectParameter> IEnumerable<EffectParameter>.GetEnumerator() => _elements.GetEnumerator();

		public List<EffectParameter>.Enumerator GetEnumerator() => _elements.GetEnumerator();

		public EffectParameter? GetParameterBySemantic(string semantic)
		{
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (EffectParameter elem in _elements)
			{
				if (semantic.Equals(elem.Semantic, StringComparison.Ordinal))
				{
					return elem;
				}
			}

			return null;
		}
	}
}
