// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Numerics;

namespace Katabasis.Samples
{
	public class App : Game
	{
		private Vector4 _clearColor;
		private MouseState _currentMouseState;
		private MouseState _previousMouseState;

		public App()
		{
			Window.Title = "Katabasis Samples; Input: Mouse";
			IsMouseVisible = true;
		}

		protected override void Update(GameTime gameTime)
		{
			_previousMouseState = _currentMouseState;
			_currentMouseState = Mouse.GetState();

			var viewport = GraphicsDevice.Viewport;
			var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

			var redPrevious = (float)_previousMouseState.X / viewport.Width;
			var redCurrent = (float)_currentMouseState.X / viewport.Width;
			_clearColor.X = MathHelper.Lerp(redPrevious, redCurrent, deltaSeconds);

			var greenPrevious = (float)_previousMouseState.Y / viewport.Height;
			var greenCurrent = (float)_currentMouseState.Y / viewport.Height;
			_clearColor.Y = MathHelper.Lerp(greenPrevious, greenCurrent, deltaSeconds);

			var isClickedLeft = _currentMouseState.LeftButton == ButtonState.Pressed;
			if (isClickedLeft)
			{
				_clearColor.Z = _clearColor.Z >= 1.0f ? 0.0f : _clearColor.Z + 0.04f;
			}

			var isClickedRight = _currentMouseState.RightButton == ButtonState.Pressed;
			if (isClickedRight)
			{
				_clearColor.Z = _clearColor.Z <= 0.0f ? 1.0f : _clearColor.Z - 0.04f;
			}

			var deltaScrollValue = _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
			_clearColor.Z = MathHelper.Clamp(_clearColor.Z + (deltaScrollValue * 0.04f), 0, 1);

			var isClickedMiddle = _currentMouseState.MiddleButton == ButtonState.Pressed;
			if (isClickedMiddle)
			{
				_clearColor.Z = 0.0f;
			}
		}

		protected override void Draw(GameTime gameTime) => GraphicsDevice.Clear(new Color(_clearColor));
	}
}
