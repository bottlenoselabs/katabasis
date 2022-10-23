// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[PublicAPI]
	public class GameTime
	{
		public GameTime()
		{
			TotalGameTime = TimeSpan.Zero;
			ElapsedGameTime = TimeSpan.Zero;
			IsRunningSlowly = false;
		}

		public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
		{
			TotalGameTime = totalGameTime;
			ElapsedGameTime = elapsedGameTime;
			IsRunningSlowly = false;
		}

		public GameTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, bool isRunningSlowly)
		{
			TotalGameTime = totalRealTime;
			ElapsedGameTime = elapsedRealTime;
			IsRunningSlowly = isRunningSlowly;
		}

		public TimeSpan TotalGameTime { get; internal set; }

		public TimeSpan ElapsedGameTime { get; internal set; }

		public bool IsRunningSlowly { get; internal set; }
	}
}
