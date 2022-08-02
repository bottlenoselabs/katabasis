// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;

namespace bottlenoselabs.Katabasis
{
	[Flags]
	public enum Buttons
	{
		DPadUp = 0x00000001,
		DPadDown = 0x00000002,
		DPadLeft = 0x00000004,
		DPadRight = 0x00000008,
		Start = 0x00000010,
		Back = 0x00000020,
		LeftStick = 0x00000040,
		RightStick = 0x00000080,
		LeftShoulder = 0x00000100,
		RightShoulder = 0x00000200,
		BigButton = 0x00000800,
		A = 0x00001000,
		B = 0x00002000,
		X = 0x00004000,
		Y = 0x00008000,
		LeftThumbstickLeft = 0x00200000,
		RightTrigger = 0x00400000,
		LeftTrigger = 0x00800000,
		RightThumbstickUp = 0x01000000,
		RightThumbstickDown = 0x02000000,
		RightThumbstickRight = 0x04000000,
		RightThumbstickLeft = 0x08000000,
		LeftThumbstickUp = 0x10000000,
		LeftThumbstickDown = 0x20000000,
		LeftThumbstickRight = 0x40000000,

		// Extensions
		Misc1EXT = 0x00000400,
		Paddle1EXT = 0x00010000,
		Paddle2EXT = 0x00020000,
		Paddle3EXT = 0x00040000,
		Paddle4EXT = 0x00080000,
		TouchPadEXT = 0x00100000
	}
}
