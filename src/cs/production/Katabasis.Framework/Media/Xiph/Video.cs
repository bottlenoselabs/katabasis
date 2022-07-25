// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
	public sealed class Video
	{
		internal double _fps;
		internal bool _needsDurationHack;
		internal IntPtr _theora;
		internal int _uvHeight;
		internal int _uvWidth;
		internal int _yHeight;
		internal int _yWidth;

		public Video(string fileName)
		{
			Theorafile.tf_fopen(fileName, out _theora);
			Theorafile.tf_videoinfo(_theora, out _yWidth, out _yHeight, out _fps, out var pixelFormat);
			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (pixelFormat)
			{
				case Theorafile.th_pixel_fmt.TH_PF_420:
					_uvWidth = _yWidth / 2;
					_uvHeight = _yHeight / 2;
					break;
				case Theorafile.th_pixel_fmt.TH_PF_422:
					_uvWidth = _yWidth / 2;
					_uvHeight = _yHeight;
					break;
				case Theorafile.th_pixel_fmt.TH_PF_444:
					_uvWidth = _yWidth;
					_uvHeight = _yHeight;
					break;
				default:
					throw new NotSupportedException("Unrecognized YUV format!");
			}

			// FIXME: This is a part of the Duration hack!
			Duration = TimeSpan.MaxValue;
			_needsDurationHack = true;
		}

		internal Video(string fileName, int durationMilliseconds, int width, int height, float framesPerSecond, VideoSoundtrackType soundtrackType)
			: this(fileName)
		{
			/* If you got here, you've still got the XNB file! Well done!
			 * Except if you're running FNA, you're not using the WMV anymore.
			 * But surely it's the same video, right...?
			 * Well, consider this a check more than anything. If this bothers
			 * you, just remove the XNB file and we'll read the OGV straight up.
			 * -flibit
			 */
			if (width != Width || height != Height)
			{
				throw new InvalidOperationException("XNB/OGV width/height mismatch! Width: " + Width + " Height: " + Height);
			}

			if (Math.Abs(FramesPerSecond - framesPerSecond) >= 1.0f)
			{
				throw new InvalidOperationException("XNB/OGV framesPerSecond mismatch! FPS: " + FramesPerSecond);
			}

			// FIXME: Oh, hey! I wish we had this info in Theora!
			Duration = TimeSpan.FromMilliseconds(durationMilliseconds);
			_needsDurationHack = false;

			VideoSoundtrackType = soundtrackType;
		}

		public int Width => _yWidth;

		public int Height => _yHeight;

		public float FramesPerSecond => (float)_fps;

		public VideoSoundtrackType VideoSoundtrackType { get; }

		// FIXME: This is hacked, look up "This is a part of the Duration hack!"
		public TimeSpan Duration { get; internal set; }

		~Video()
		{
			if (_theora != IntPtr.Zero)
			{
				Theorafile.tf_close(ref _theora);
			}
		}

		// ReSharper disable once InconsistentNaming
		public void SetAudioTrackEXT(int track)
		{
			if (_theora == IntPtr.Zero)
			{
				return;
			}

			Theorafile.tf_setaudiotrack(_theora, track);
		}
	}
}
