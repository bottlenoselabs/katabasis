// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;

namespace Katabasis
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.gesturetype.aspx
	[Flags]
	public enum GestureType
	{
		None = 0x000,
		Tap = 0x001,
		DoubleTap = 0x002,
		Hold = 0x004,
		HorizontalDrag = 0x008,
		VerticalDrag = 0x010,
		FreeDrag = 0x020,
		Pinch = 0x040,
		Flick = 0x080,
		DragComplete = 0x100,
		PinchComplete = 0x200
	}
}
