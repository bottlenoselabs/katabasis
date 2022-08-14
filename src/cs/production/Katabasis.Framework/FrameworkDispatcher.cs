// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.Collections.Generic;

namespace bottlenoselabs.Katabasis
{
	public static class FrameworkDispatcher
	{
		internal static bool ActiveSongChanged;
		internal static bool MediaStateChanged;
		internal static List<DynamicSoundEffectInstance> Streams = new();

		public static void Update()
		{
			/* Updates the status of various framework components
			 * (such as power state and media), and raises related events.
			 */
			lock (Streams)
			{
				for (var i = 0; i < Streams.Count; i += 1)
				{
					DynamicSoundEffectInstance soundEffect = Streams[i];
					soundEffect.Update();
					if (soundEffect.IsDisposed)
					{
						i -= 1;
					}
				}
			}

			if (Microphone.MicList != null)
			{
				for (var i = 0; i < Microphone.MicList.Count; i += 1)
				{
					Microphone.MicList[i].CheckBuffer();
				}
			}

			MediaPlayer.Update();
			if (ActiveSongChanged)
			{
				MediaPlayer.OnActiveSongChanged();
				ActiveSongChanged = false;
			}

			if (MediaStateChanged)
			{
				MediaPlayer.OnMediaStateChanged();
				MediaStateChanged = false;
			}

			if (TouchPanel.TouchDeviceExists)
			{
				TouchPanel.Update();
			}
		}
	}
}
