// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections;
using System.Collections.Generic;

namespace Ankura
{
    public sealed class EffectAnnotationCollection : IEnumerable<EffectAnnotation>
    {
        private readonly List<EffectAnnotation> _elements;

        internal EffectAnnotationCollection(List<EffectAnnotation> value)
        {
            _elements = value;
        }

        public List<EffectAnnotation>.Enumerator GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        public int Count => _elements.Count;

        public EffectAnnotation? this[int index] => _elements[index];

        public EffectAnnotation? this[string name]
        {
            get
            {
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (EffectAnnotation elem in _elements)
                {
                    if (name.Equals(elem.Name))
                    {
                        return elem;
                    }
                }

                return null; // FIXME: ArrayIndexOutOfBounds? -flibit
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator<EffectAnnotation> IEnumerable<EffectAnnotation>.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}
