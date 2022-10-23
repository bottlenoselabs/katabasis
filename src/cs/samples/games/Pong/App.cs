// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;

namespace bottlenoselabs.Katabasis.Samples
{
	public enum WaveType
	{
		Sin,
		Tan,
		Square,
		Noise
	}

	public class App : Game
	{
		public App()
		{
			Window.Title = "Katabasis Samples; Pong";
			Window.AllowUserResizing = true;
			IsFixedTimeStep = true;

			GraphicsDeviceManager.Instance.PreferredBackBufferWidth = Pong.Viewport.Width;
			GraphicsDeviceManager.Instance.PreferredBackBufferHeight = Pong.Viewport.Height;

			Window.ClientSizeChanged += WindowOnClientSizeChanged;
		}

		private void WindowOnClientSizeChanged(object? sender, EventArgs e)
		{
			Pong.Viewport = GraphicsDevice.Viewport;
			Pong.ViewportChanged();
		}

		protected override void LoadContent()
		{
			IsMouseVisible = true;

			Pong.LoadContent();
			Pong.Reset();
		}

		protected override void Update(GameTime gameTime)
		{
			Pong.UpdateGame(gameTime);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			Pong.Draw();
		}

	}
}
