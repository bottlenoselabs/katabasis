// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[PublicAPI]
	public sealed class TextureCollection
	{
		private readonly bool[] _modifiedSamplers;
		private readonly Texture?[] _textures;

		internal bool _ignoreTargets;

		internal TextureCollection(int slots, bool[] modSamplers)
		{
			_textures = new Texture[slots];
			_modifiedSamplers = modSamplers;
			if (!_ignoreTargets)
			{
				for (var i = 0; i < _textures.Length; i += 1)
				{
					_textures[i] = null;
				}
			}

			_ignoreTargets = false;
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

					for (var i = 0; i < value.GraphicsDevice.RenderTargetCount; i += 1)
					{
						if (value == value.GraphicsDevice.RenderTargetBindings[i].RenderTarget)
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
