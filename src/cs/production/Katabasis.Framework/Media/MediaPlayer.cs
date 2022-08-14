// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using static bottlenoselabs.FAudio;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Not used?")]
	public static class MediaPlayer
	{
		private static bool _isMuted;
		private static MediaState _state = MediaState.Stopped;
		private static float _volume = 1.0f;

		private static bool _initialized;

		/* Need to hold onto this to keep track of how many songs
		 * have played when in shuffle mode.
		 */
		private static int _songsCountInQueuePlayed;

		/* FIXME: Ideally we'd be using the stream offset to track position,
		 * but usually you end up with a bit of stair stepping...
		 *
		 * For now, just use a timer. It's not 100% accurate, but it'll at
		 * least be consistent.
		 * -flibit
		 */
		private static readonly Stopwatch _timer = new();

		private static readonly Random _random = new();

		static MediaPlayer() => Queue = new MediaQueue();

		public static bool GameHasControl =>
			/* This is based on whether or not the player is playing custom
			     * music, rather than yours.
			     * -flibit
			     */
			true;

		public static bool IsMuted
		{
			get => _isMuted;

			set
			{
				_isMuted = value;
				XNA_SetSongVolume(_isMuted ? 0.0f : _volume);
			}
		}

		public static bool IsRepeating { get; set; }

		public static bool IsShuffled { get; set; }

		public static TimeSpan PlayPosition => _timer.Elapsed;

		public static MediaQueue Queue { get; }

		public static MediaState State
		{
			get => _state;

			private set
			{
				if (_state != value)
				{
					_state = value;
					FrameworkDispatcher.MediaStateChanged = true;
				}
			}
		}

		public static float Volume
		{
			get => _volume;
			set
			{
				_volume = MathHelper.Clamp(
					value,
					0.0f,
					1.0f);

				XNA_SetSongVolume(IsMuted ? 0.0f : _volume);
			}
		}

		public static bool IsVisualizationEnabled
		{
			get => XNA_VisualizationEnabled() == 1;
			set => XNA_EnableVisualization((byte)(value ? 1 : 0));
		}

		public static event EventHandler<EventArgs>? ActiveSongChanged;

		public static event EventHandler<EventArgs>? MediaStateChanged;

		public static void MoveNext() => NextSong(1);

		public static void MovePrevious() => NextSong(-1);

		public static void Pause()
		{
			if (State != MediaState.Playing || Queue.ActiveSong == null)
			{
				return;
			}

			XNA_PauseSong();
			_timer.Stop();

			State = MediaState.Paused;
		}

		public static void Play(Song song)
		{
			Song? previousSong = Queue.Count > 0 ? Queue[0] : null;

			Queue.Clear();
			_songsCountInQueuePlayed = 0;
			LoadSong(song);
			Queue.ActiveSongIndex = 0;

			PlaySong(song);

			Queue[0].Duration = song.Duration;

			if (previousSong != song)
			{
				FrameworkDispatcher.ActiveSongChanged = true;
			}
		}

		public static void Play(SongCollection songs) => Play(songs, 0);

		public static void Play(SongCollection songs, int index)
		{
			Queue.Clear();
			_songsCountInQueuePlayed = 0;

			foreach (Song song in songs)
			{
				LoadSong(song);
			}

			Queue.ActiveSongIndex = index;

			PlaySong(Queue.ActiveSong);
		}

		public static void Resume()
		{
			if (State != MediaState.Paused)
			{
				return;
			}

			XNA_ResumeSong();
			_timer.Start();
			State = MediaState.Playing;
		}

		public static void Stop()
		{
			if (State == MediaState.Stopped)
			{
				return;
			}

			XNA_StopSong();
			_timer.Stop();
			_timer.Reset();

			for (var i = 0; i < Queue.Count; i += 1)
			{
				Queue[i].PlayCount = 0;
			}

			State = MediaState.Stopped;
		}

		public static void GetVisualizationData(VisualizationData data) =>
			XNA_GetSongVisualizationData(
				data._frequencies,
				data._samples,
				VisualizationData.Size);

		internal static void Update()
		{
			if (Queue.ActiveSong == null ||
			    State != MediaState.Playing ||
			    XNA_GetSongEnded() == 0)
			{
				// Nothing to do... yet...
				return;
			}

			_songsCountInQueuePlayed += 1;

			if (_songsCountInQueuePlayed >= Queue.Count)
			{
				_songsCountInQueuePlayed = 0;
				if (!IsRepeating)
				{
					Stop();

					FrameworkDispatcher.ActiveSongChanged = true;

					return;
				}
			}

			MoveNext();
		}

		internal static void DisposeIfNecessary()
		{
			if (_initialized)
			{
				XNA_SongQuit();
				_initialized = false;
			}
		}

		internal static void OnActiveSongChanged() => ActiveSongChanged?.Invoke(null, EventArgs.Empty);

		internal static void OnMediaStateChanged() => MediaStateChanged?.Invoke(null, EventArgs.Empty);

		private static void LoadSong(Song song) =>
			/* Believe it or not, XNA duplicates the Song object
	         * and then assigns a bunch of stuff to it at Play time.
	         * -flibit
	         */
			Queue.Add(new Song(song._handle, song.Name));

		private static void NextSong(int direction)
		{
			Stop();
			if (IsRepeating && Queue.ActiveSongIndex >= Queue.Count - 1)
			{
				Queue.ActiveSongIndex = 0;

				/* Setting direction to 0 will force the first song
				 * in the queue to be played.
				 * if we're on "shuffle", then it'll pick a random one
				 * anyway, regardless of the "direction".
				 */
				direction = 0;
			}

			if (IsShuffled)
			{
				Queue.ActiveSongIndex = _random.Next(Queue.Count);
			}
			else
			{
				Queue.ActiveSongIndex = MathHelper.Clamp(
					Queue.ActiveSongIndex + direction,
					0,
					Queue.Count - 1);
			}

			Song nextSong = Queue[Queue.ActiveSongIndex];
			if (nextSong != null)
			{
				PlaySong(nextSong);
			}

			FrameworkDispatcher.ActiveSongChanged = true;
		}

		private static void PlaySong(Song? song)
		{
			if (!_initialized)
			{
				XNA_SongInit();
				_initialized = true;
			}

			if (song == null)
			{
				return;
			}

			var durationSeconds = XNA_PlaySong(song._handle);
			song.Duration = TimeSpan.FromSeconds(durationSeconds);
			_timer.Restart();
			State = MediaState.Playing;
		}
	}
}
