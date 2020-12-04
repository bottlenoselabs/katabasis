// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

namespace Katabasis
{
    public sealed class EffectTechnique
    {
        public string Name { get; }

        public EffectPassCollection Passes { get; }

        public EffectAnnotationCollection Annotations { get; }

        internal IntPtr TechniquePointer { get; }

        internal EffectTechnique(
            string? name,
            IntPtr pointer,
            EffectPassCollection passes,
            EffectAnnotationCollection annotations)
        {
            Name = name ?? string.Empty;
            Passes = passes;
            Annotations = annotations;
            TechniquePointer = pointer;
        }
    }
}
