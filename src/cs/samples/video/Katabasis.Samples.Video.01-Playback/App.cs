// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.IO;

namespace bottlenoselabs.Katabasis.Samples
{
	public class App : Game
	{
		private Video _video = null!;
		private VideoPlayer _videoPlayer = null!;
		private SpriteBatch _spriteBatch = null!;

		public App()
		{
			Window.Title = "Katabasis Samples; Video: Playback";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
		}

		protected override void Initialize()
		{
			_spriteBatch = new SpriteBatch();
			_videoPlayer = new VideoPlayer();
			_video = new Video(Path.Combine(AppContext.BaseDirectory, "Assets", "Video", "videoplayback.ogv"));
			_videoPlayer.Play(_video);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			var videoFrameTexture = _videoPlayer.GetTexture();
			_spriteBatch.Begin();
			_spriteBatch.Draw(videoFrameTexture, GraphicsDevice.Viewport.Bounds, Color.White);
			_spriteBatch.End();
		}
	}
}
