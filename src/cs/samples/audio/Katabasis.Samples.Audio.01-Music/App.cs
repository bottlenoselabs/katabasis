// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Katabasis.ImGui;
using static bottlenoselabs.imgui;
using static bottlenoselabs.imgui.Runtime;


namespace Katabasis.Samples
{
	public class App : Game
	{
		private Song[] _songs = null!;
		private int _activeSongIndex;
		private Song? _activeSong;
		private MediaState _musicState;

		private ImGuiRenderer _imGuiRenderer = null!;
		private bool _isRepeating;

		public App()
		{
			Window.Title = "Katabasis Samples; Audio: Music";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
		}

		protected override void Initialize()
		{
			_imGuiRenderer = new ImGuiRenderer();
			_imGuiRenderer.BuildFontAtlas();

			var musicFilePaths = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Assets", "Music"));
			var songs = new List<Song>();
			foreach (var musicFilePath in musicFilePaths)
			{
				var songName = Path.GetFileName(musicFilePath);
				var song = Song.FromFile(songName, musicFilePath);
				songs.Add(song);
			}

			_songs = songs.ToArray();

			MediaPlayer.MediaStateChanged += MediaPlayerOnMediaStateChanged;
		}

		private void MediaPlayerOnMediaStateChanged(object? sender, EventArgs e)
		{
			_musicState = MediaPlayer.State;
		}

		protected override void Update(GameTime gameTime)
		{
			_activeSong = _activeSongIndex == -1 ? null : _songs[_activeSongIndex];
			
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_imGuiRenderer.Begin(gameTime);
			DrawUserInterface();
			_imGuiRenderer.End();
		}
		
		protected virtual unsafe void DrawUserInterface()
		{
			igSetWindowSize_Vec2(new Vector2(400), default);
			igBegin(Window.Title, default, default);
			{
				var songName = _activeSong == null ? string.Empty : _activeSong.Name;
				igText(@$"
Music by Eric Matyas
www.soundimage.org

Current song: {songName}
".Trim());

				var activeSongDuration = _activeSong?.Duration ?? TimeSpan.Zero;
				igText($"{MediaPlayer.PlayPosition.TotalSeconds:F2}s / {activeSongDuration.TotalSeconds:F2}s");
				var progress = (float)(MediaPlayer.PlayPosition.Ticks / (double)activeSongDuration.Ticks);
				igProgressBar(progress, new Vector2(-1, 20), "%");

				var canMovePrevious = _songs.Length > 1;
				igBeginDisabled(!canMovePrevious);
				var movePreviousClicked = igButton("<", new Vector2(50));
				if (movePreviousClicked)
				{
					_activeSongIndex = (_activeSongIndex - 1 + _songs.Length) % _songs.Length;
					_activeSong = _songs[_activeSongIndex];
					MediaPlayer.Play(_activeSong);
				}
				igEndDisabled();
				
				igSameLine(default, -1);

				var canPlay = _musicState != MediaState.Playing && _activeSong != null;
				igBeginDisabled(!canPlay);
				var playButtonClicked = igButton("Play", new Vector2(50));
				if (playButtonClicked)
				{
					MediaPlayer.Play(_activeSong!);
				}
				igEndDisabled();
				
				igSameLine(default, -1);

				var canPause = _musicState == MediaState.Playing;
				igBeginDisabled(!canPause);
				var pauseButtonClicked = igButton("Pause", new Vector2(50));
				if (pauseButtonClicked)
				{
					MediaPlayer.Pause();
				}
				igEndDisabled();
				
				igSameLine(default, -1);

				var canResume = _musicState == MediaState.Paused;
				igBeginDisabled(!canResume);
				var resumeButtonClicked = igButton("Resume", new Vector2(50));
				if (resumeButtonClicked)
				{
					MediaPlayer.Resume();
				}
				igEndDisabled();
				
				igSameLine(default, -1);
				
				var canMoveNext = _songs.Length > 1;
				igBeginDisabled(!canMoveNext);
				var moveNextClicked = igButton(">", new Vector2(50));
				if (moveNextClicked)
				{
					_activeSongIndex = (_activeSongIndex + 1 + _songs.Length) % _songs.Length;
					_activeSong = _songs[_activeSongIndex];
					MediaPlayer.Play(_activeSong);
				}
				igEndDisabled();
				
				igSameLine(default, -1);
				
				igCheckbox("Repeat", (CBool*)Unsafe.AsPointer(ref _isRepeating));
				MediaPlayer.IsRepeating = _isRepeating;

				igText($"Average {1000f / igGetIO()->Framerate:F3} ms/frame ({igGetIO()->Framerate:F1} FPS)");
			}
			igEnd();
		}
	}
}
