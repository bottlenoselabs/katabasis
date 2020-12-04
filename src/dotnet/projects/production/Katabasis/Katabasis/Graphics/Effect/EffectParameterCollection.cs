// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Will gut Mojo shader soon.")]
    public sealed class EffectParameterCollection : IEnumerable<EffectParameter>
    {
        private readonly List<EffectParameter> _elements;

        public int Count => _elements.Count;

        public EffectParameter? this[int index] => _elements[index];

        public EffectParameter? this[string name]
        {
            get
            {
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (EffectParameter elem in _elements)
                {
                    if (name.Equals(elem.Name))
                    {
                        return elem;
                    }
                }

                return null; // FIXME: ArrayIndexOutOfBounds? -flibit
            }
        }

        internal EffectParameterCollection(List<EffectParameter> value)
        {
            _elements = value;
        }

        public List<EffectParameter>.Enumerator GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        public EffectParameter? GetParameterBySemantic(string semantic)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (EffectParameter elem in _elements)
            {
                if (semantic.Equals(elem.Semantic))
                {
                    return elem;
                }
            }

            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator<EffectParameter> IEnumerable<EffectParameter>.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}
