// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

namespace Ankura
{
    public sealed class SamplerStateCollection
    {
        private readonly bool[] _modifiedSamplers;
        private readonly SamplerState?[] _samplers;

        internal SamplerStateCollection(int slots, bool[] modSamplers)
        {
            _samplers = new SamplerState[slots];
            _modifiedSamplers = modSamplers;
            for (var i = 0; i < _samplers.Length; i += 1)
            {
                _samplers[i] = SamplerState.LinearWrap;
            }
        }

        public SamplerState? this[int index]
        {
            get => _samplers[index];
            set
            {
                _samplers[index] = value;
                _modifiedSamplers[index] = true;
            }
        }
    }
}
