// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[PublicAPI]
	public enum GamePadType
	{
		Unknown,
		GamePad,
		Wheel,
		ArcadeStick,
		FlightStick,
		DancePad,
		Guitar,
		AlternateGuitar,
		DrumKit,
		BigButtonPad
	}
}
