#region File Description
//-----------------------------------------------------------------------------
// SpriteEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The default effect used by SpriteBatch.
    /// </summary>
    internal class SpriteEffect : Effect
    {
        #region Effect Parameters

        EffectParameter matrixParam;

        #endregion

        #region Methods


        /// <summary>
        /// Creates a new SpriteEffect.
        /// </summary>
        public SpriteEffect()
            : base(Resources.SpriteEffect)
        {
            CacheEffectParameters();
        }


        /// <summary>
        /// Creates a new SpriteEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected SpriteEffect(SpriteEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters();
        }


        /// <summary>
        /// Creates a clone of the current SpriteEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new SpriteEffect(this);
        }


        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters()
        {
            matrixParam = Parameters["MatrixTransform"];
        }


        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected internal override void OnApply()
        {
            Viewport viewport = GraphicsDevice.Viewport;

            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            Matrix4x4 halfPixelOffset = Matrix4x4.CreateTranslation(-0.5f, -0.5f, 0);

            matrixParam.SetValue(halfPixelOffset * projection);
        }


        #endregion
    }
}
