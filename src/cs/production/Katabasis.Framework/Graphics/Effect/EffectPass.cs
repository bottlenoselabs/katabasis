// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;

namespace bottlenoselabs.Katabasis
{
	public sealed class EffectPass
	{
		private readonly Effect _parentEffect;
		private readonly IntPtr _parentTechnique;
		private readonly uint _pass;

		internal EffectPass(
			string? name,
			EffectAnnotationCollection annotations,
			Effect parent,
			IntPtr technique,
			uint passIndex)
		{
			Name = name ?? string.Empty;
			Annotations = annotations;
			_parentEffect = parent;
			_parentTechnique = technique;
			_pass = passIndex;
		}

		public string Name { get; }

		public EffectAnnotationCollection Annotations { get; }

		public void Apply()
		{
			if (_parentTechnique != _parentEffect.CurrentTechnique!.TechniquePointer)
			{
				throw new InvalidOperationException("Applied a pass not in the current technique!");
			}

			_parentEffect.OnApply();
			_parentEffect.INTERNAL_applyEffect(_pass);
		}
	}
}
