// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;

namespace bottlenoselabs.Katabasis
{
	public sealed class EffectTechnique
	{
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

		public string Name { get; }

		public EffectPassCollection Passes { get; }

		public EffectAnnotationCollection Annotations { get; }

		internal IntPtr TechniquePointer { get; }
	}
}
