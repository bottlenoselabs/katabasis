// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Numerics;

namespace Katabasis.Extended.Graphics
{
    internal sealed class RendererSpriteEffect : Effect
    {
        private EffectParameter _parameterMatrix = null!;

        public RendererSpriteEffect(byte[] effectCode)
            : base(effectCode)
        {
            CacheEffectParameters();
        }

        public RendererSpriteEffect(Effect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters();
        }

        public override Effect Clone() => new RendererSpriteEffect(this);

        private void CacheEffectParameters()
        {
            _parameterMatrix = Parameters!["MatrixTransform"]!;
        }

        protected override void OnApply()
        {
            var viewport = GraphicsDevice.Viewport;

            var projection = Matrix4x4.CreateOrthographicOffCenter(
                0, viewport.Width, viewport.Height, 0, 0, 1);
            var halfPixelOffset = Matrix4x4.CreateTranslation(
                -0.5f, -0.5f, 0);

            _parameterMatrix.SetValue(halfPixelOffset * projection);
        }
    }
}
