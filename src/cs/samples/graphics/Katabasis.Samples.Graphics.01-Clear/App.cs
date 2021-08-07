// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Numerics;

namespace Katabasis.Samples
{
	public class App : Game
	{
		private Vector4 _clearColor;

		public App()
		{
			Window.Title = "Katabasis Samples; Graphics: Clear";

			_clearColor = Color.Red.ToVector4();
		}

		protected override void Draw(GameTime gameTime)
		{
			_clearColor.Y = _clearColor.Y > 1.0f ? 0.0f : _clearColor.Y + 0.01f;

			// Clear the default framebuffer with the specified color
			GraphicsDevice.Clear(new Color(_clearColor));
		}
	}
}
