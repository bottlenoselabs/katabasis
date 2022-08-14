// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.Numerics;

namespace bottlenoselabs.Katabasis
{
	internal class SpriteEffect : Effect
	{
		private EffectParameter _matrixParam = null!;

		public SpriteEffect()
			: base(Resources.SpriteEffect) =>
			CacheEffectParameters();

		protected SpriteEffect(SpriteEffect cloneSource)
			: base(cloneSource) =>
			CacheEffectParameters();

		public override Effect Clone() => new SpriteEffect(this);

		protected internal override void OnApply()
		{
			var viewport = GraphicsDevice.Viewport;

			var projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
			var halfPixelOffset = Matrix4x4.CreateTranslation(-0.5f, -0.5f, 0);

			_matrixParam?.SetValue(halfPixelOffset * projection);
		}

		private void CacheEffectParameters() => _matrixParam = Parameters!["MatrixTransform"]!;
	}
}
