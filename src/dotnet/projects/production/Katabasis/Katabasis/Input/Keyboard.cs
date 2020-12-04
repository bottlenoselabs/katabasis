// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Generic;

namespace Katabasis
{
    public static class Keyboard
    {
        internal static List<Keys> _keys = new List<Keys>();

        public static Keys GetKeyFromScancodeEXT(Keys scancode)
        {
            return FNAPlatform.GetKeyFromScancode(scancode);
        }

        public static KeyboardState GetState()
        {
            return new KeyboardState(_keys);
        }

        public static KeyboardState GetState(PlayerIndex playerIndex)
        {
            return new KeyboardState(_keys);
        }
    }
}
