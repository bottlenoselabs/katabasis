// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections;
using System.Collections.Generic;

namespace Ankura
{
    public sealed class EffectTechniqueCollection : IEnumerable<EffectTechnique>, IEnumerable
    {
        private readonly List<EffectTechnique> _elements;

        public int Count => _elements.Count;

        public EffectTechnique? this[int index] => _elements[index];

        internal EffectTechniqueCollection(List<EffectTechnique> value)
        {
            _elements = value;
        }

        public List<EffectTechnique>.Enumerator GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        public EffectTechnique? this[string name]
        {
            get
            {
                foreach (EffectTechnique elem in _elements)
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

        IEnumerator<EffectTechnique> IEnumerable<EffectTechnique>.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}
