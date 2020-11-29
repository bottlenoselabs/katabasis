// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Ankura
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Not used?")]
    public class VisualizationData
    {
        internal const int Size = 256;

        internal float[] _frequencies;
        internal float[] _samples;

        public ReadOnlyCollection<float> Frequencies => new ReadOnlyCollection<float>(_frequencies);

        public ReadOnlyCollection<float> Samples => new ReadOnlyCollection<float>(_samples);

        public VisualizationData()
        {
            _frequencies = new float[Size];
            _samples = new float[Size];
        }
    }
}
