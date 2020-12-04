// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

namespace Katabasis
{
    public sealed class TextureCollection
    {
        private readonly Texture?[] _textures;
        private readonly bool[] _modifiedSamplers;

        internal TextureCollection(int slots, bool[] modSamplers)
        {
            _textures = new Texture[slots];
            _modifiedSamplers = modSamplers;
            for (var i = 0; i < _textures.Length; i += 1)
            {
                _textures[i] = null;
            }
        }

        public Texture? this[int index]
        {
            get
            {
                return _textures[index];
            }

            set
            {
#if DEBUG
                // XNA checks for disposed textures here! -flibit
                if (value != null)
                {
                    if (value.IsDisposed)
                    {
                        throw new ObjectDisposedException(value.GetType().ToString());
                    }

                    for (var i = 0; i < value.GraphicsDevice._renderTargetCount; i += 1)
                    {
                        if (value == value.GraphicsDevice._renderTargetBindings[i].RenderTarget)
                        {
                            throw new InvalidOperationException("The render target must not be set on the device when it is used as a texture.");
                        }
                    }
                }
#endif
                _textures[index] = value;
                _modifiedSamplers[index] = true;
            }
        }

        internal void RemoveDisposedTexture(Texture tex)
        {
            for (var i = 0; i < _textures.Length; i += 1)
            {
                if (tex == _textures[i])
                {
                    this[i] = null;
                }
            }
        }
    }
}
