// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Collections.Generic;

namespace Katabasis
{
	public static class Keyboard
	{
		internal static List<Keys> _keys = new();

		public static Keys GetKeyFromScancodeEXT(Keys scancode) => FNAPlatform.GetKeyFromScancode(scancode);

		public static KeyboardState GetState() => new(_keys);

		public static KeyboardState GetState(PlayerIndex playerIndex) => new(_keys);
	}
}
