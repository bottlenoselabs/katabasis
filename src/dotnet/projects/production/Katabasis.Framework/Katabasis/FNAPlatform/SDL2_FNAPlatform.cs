// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ObjCRuntime;
using SDL2;
using static SDL2.SDL;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Native naming conventions.")]
	internal static class SDL2_FNAPlatform
	{
		/* This is needed for asynchronous window events */
		private static readonly List<Game> _activeGames = new();

		private static string _osVersion = null!;

		private static readonly bool UseScancodes =
			Environment.GetEnvironmentVariable("FNA_KEYBOARD_USE_SCANCODES") == "1";

		private static bool _supportsGlobalMouse;

		// For iOS high dpi support
		private static int _retinaWidth;
		private static int _retinaHeight;

		private static readonly bool OSXUseSpaces =
			SDL_GetPlatform().Equals("Mac OS X", StringComparison.Ordinal) && // Prevents race with OSVersion
			SDL_GetHintBoolean(SDL_HINT_VIDEO_MAC_FULLSCREEN_SPACES, SDL_bool.SDL_TRUE) ==
			SDL_bool.SDL_TRUE;

		private static readonly SDL_EventFilter _win32OnPaint = Win32OnPaint;
		private static SDL_EventFilter? _prevEventFilter;

		// Controller device information
		private static readonly IntPtr[] _devices = new IntPtr[GamePad.GamePadCount];
		private static readonly Dictionary<int, int> _instanceList = new();
		private static readonly string[] _guids = GenStringArray();

		/* Microphone is almost never used, so we give this subsystem
		* special treatment and init only when we start calling these
		* functions.
		* -flibit
		*/
		private static bool _micInit;

		// Cached GamePadStates/Capabilities
		private static readonly GamePadState[] _states = new GamePadState[GamePad.GamePadCount];

		private static readonly GamePadCapabilities[] _capabilities = new GamePadCapabilities[GamePad.GamePadCount];

		private static readonly GamePadType[] _gamepadType =
		{
			GamePadType.Unknown, GamePadType.GamePad, GamePadType.Wheel, GamePadType.ArcadeStick, GamePadType.FlightStick, GamePadType.DancePad,
			GamePadType.Guitar, GamePadType.DrumKit, GamePadType.BigButtonPad
		};

		private static Game? _emscriptenGame;

		/* From: http://blogs.msdn.com/b/shawnhar/archive/2007/07/02/twin-paths-to-garbage-collector-nirvana.aspx
		 * "If you use an enum type as a dictionary key, internal dictionary operations will cause boxing.
		 * You can avoid this by using integer keys, and casting your enum values to integers before adding
		 * them to the dictionary."
		 */
		private static readonly Dictionary<int, Keys> _keyMap = new()
		{
			{(int)SDL_Keycode.SDLK_a, Keys.A},
			{(int)SDL_Keycode.SDLK_b, Keys.B},
			{(int)SDL_Keycode.SDLK_c, Keys.C},
			{(int)SDL_Keycode.SDLK_d, Keys.D},
			{(int)SDL_Keycode.SDLK_e, Keys.E},
			{(int)SDL_Keycode.SDLK_f, Keys.F},
			{(int)SDL_Keycode.SDLK_g, Keys.G},
			{(int)SDL_Keycode.SDLK_h, Keys.H},
			{(int)SDL_Keycode.SDLK_i, Keys.I},
			{(int)SDL_Keycode.SDLK_j, Keys.J},
			{(int)SDL_Keycode.SDLK_k, Keys.K},
			{(int)SDL_Keycode.SDLK_l, Keys.L},
			{(int)SDL_Keycode.SDLK_m, Keys.M},
			{(int)SDL_Keycode.SDLK_n, Keys.N},
			{(int)SDL_Keycode.SDLK_o, Keys.O},
			{(int)SDL_Keycode.SDLK_p, Keys.P},
			{(int)SDL_Keycode.SDLK_q, Keys.Q},
			{(int)SDL_Keycode.SDLK_r, Keys.R},
			{(int)SDL_Keycode.SDLK_s, Keys.S},
			{(int)SDL_Keycode.SDLK_t, Keys.T},
			{(int)SDL_Keycode.SDLK_u, Keys.U},
			{(int)SDL_Keycode.SDLK_v, Keys.V},
			{(int)SDL_Keycode.SDLK_w, Keys.W},
			{(int)SDL_Keycode.SDLK_x, Keys.X},
			{(int)SDL_Keycode.SDLK_y, Keys.Y},
			{(int)SDL_Keycode.SDLK_z, Keys.Z},
			{(int)SDL_Keycode.SDLK_0, Keys.D0},
			{(int)SDL_Keycode.SDLK_1, Keys.D1},
			{(int)SDL_Keycode.SDLK_2, Keys.D2},
			{(int)SDL_Keycode.SDLK_3, Keys.D3},
			{(int)SDL_Keycode.SDLK_4, Keys.D4},
			{(int)SDL_Keycode.SDLK_5, Keys.D5},
			{(int)SDL_Keycode.SDLK_6, Keys.D6},
			{(int)SDL_Keycode.SDLK_7, Keys.D7},
			{(int)SDL_Keycode.SDLK_8, Keys.D8},
			{(int)SDL_Keycode.SDLK_9, Keys.D9},
			{(int)SDL_Keycode.SDLK_KP_0, Keys.NumPad0},
			{(int)SDL_Keycode.SDLK_KP_1, Keys.NumPad1},
			{(int)SDL_Keycode.SDLK_KP_2, Keys.NumPad2},
			{(int)SDL_Keycode.SDLK_KP_3, Keys.NumPad3},
			{(int)SDL_Keycode.SDLK_KP_4, Keys.NumPad4},
			{(int)SDL_Keycode.SDLK_KP_5, Keys.NumPad5},
			{(int)SDL_Keycode.SDLK_KP_6, Keys.NumPad6},
			{(int)SDL_Keycode.SDLK_KP_7, Keys.NumPad7},
			{(int)SDL_Keycode.SDLK_KP_8, Keys.NumPad8},
			{(int)SDL_Keycode.SDLK_KP_9, Keys.NumPad9},
			{(int)SDL_Keycode.SDLK_KP_CLEAR, Keys.OemClear},
			{(int)SDL_Keycode.SDLK_KP_DECIMAL, Keys.Decimal},
			{(int)SDL_Keycode.SDLK_KP_DIVIDE, Keys.Divide},
			{(int)SDL_Keycode.SDLK_KP_ENTER, Keys.Enter},
			{(int)SDL_Keycode.SDLK_KP_MINUS, Keys.Subtract},
			{(int)SDL_Keycode.SDLK_KP_MULTIPLY, Keys.Multiply},
			{(int)SDL_Keycode.SDLK_KP_PERIOD, Keys.OemPeriod},
			{(int)SDL_Keycode.SDLK_KP_PLUS, Keys.Add},
			{(int)SDL_Keycode.SDLK_F1, Keys.F1},
			{(int)SDL_Keycode.SDLK_F2, Keys.F2},
			{(int)SDL_Keycode.SDLK_F3, Keys.F3},
			{(int)SDL_Keycode.SDLK_F4, Keys.F4},
			{(int)SDL_Keycode.SDLK_F5, Keys.F5},
			{(int)SDL_Keycode.SDLK_F6, Keys.F6},
			{(int)SDL_Keycode.SDLK_F7, Keys.F7},
			{(int)SDL_Keycode.SDLK_F8, Keys.F8},
			{(int)SDL_Keycode.SDLK_F9, Keys.F9},
			{(int)SDL_Keycode.SDLK_F10, Keys.F10},
			{(int)SDL_Keycode.SDLK_F11, Keys.F11},
			{(int)SDL_Keycode.SDLK_F12, Keys.F12},
			{(int)SDL_Keycode.SDLK_F13, Keys.F13},
			{(int)SDL_Keycode.SDLK_F14, Keys.F14},
			{(int)SDL_Keycode.SDLK_F15, Keys.F15},
			{(int)SDL_Keycode.SDLK_F16, Keys.F16},
			{(int)SDL_Keycode.SDLK_F17, Keys.F17},
			{(int)SDL_Keycode.SDLK_F18, Keys.F18},
			{(int)SDL_Keycode.SDLK_F19, Keys.F19},
			{(int)SDL_Keycode.SDLK_F20, Keys.F20},
			{(int)SDL_Keycode.SDLK_F21, Keys.F21},
			{(int)SDL_Keycode.SDLK_F22, Keys.F22},
			{(int)SDL_Keycode.SDLK_F23, Keys.F23},
			{(int)SDL_Keycode.SDLK_F24, Keys.F24},
			{(int)SDL_Keycode.SDLK_SPACE, Keys.Space},
			{(int)SDL_Keycode.SDLK_UP, Keys.Up},
			{(int)SDL_Keycode.SDLK_DOWN, Keys.Down},
			{(int)SDL_Keycode.SDLK_LEFT, Keys.Left},
			{(int)SDL_Keycode.SDLK_RIGHT, Keys.Right},
			{(int)SDL_Keycode.SDLK_LALT, Keys.LeftAlt},
			{(int)SDL_Keycode.SDLK_RALT, Keys.RightAlt},
			{(int)SDL_Keycode.SDLK_LCTRL, Keys.LeftControl},
			{(int)SDL_Keycode.SDLK_RCTRL, Keys.RightControl},
			{(int)SDL_Keycode.SDLK_LGUI, Keys.LeftWindows},
			{(int)SDL_Keycode.SDLK_RGUI, Keys.RightWindows},
			{(int)SDL_Keycode.SDLK_LSHIFT, Keys.LeftShift},
			{(int)SDL_Keycode.SDLK_RSHIFT, Keys.RightShift},
			{(int)SDL_Keycode.SDLK_APPLICATION, Keys.Apps},
			{(int)SDL_Keycode.SDLK_MENU, Keys.Apps},
			{(int)SDL_Keycode.SDLK_SLASH, Keys.OemQuestion},
			{(int)SDL_Keycode.SDLK_BACKSLASH, Keys.OemPipe},
			{(int)SDL_Keycode.SDLK_LEFTBRACKET, Keys.OemOpenBrackets},
			{(int)SDL_Keycode.SDLK_RIGHTBRACKET, Keys.OemCloseBrackets},
			{(int)SDL_Keycode.SDLK_CAPSLOCK, Keys.CapsLock},
			{(int)SDL_Keycode.SDLK_COMMA, Keys.OemComma},
			{(int)SDL_Keycode.SDLK_DELETE, Keys.Delete},
			{(int)SDL_Keycode.SDLK_END, Keys.End},
			{(int)SDL_Keycode.SDLK_BACKSPACE, Keys.Back},
			{(int)SDL_Keycode.SDLK_RETURN, Keys.Enter},
			{(int)SDL_Keycode.SDLK_ESCAPE, Keys.Escape},
			{(int)SDL_Keycode.SDLK_HOME, Keys.Home},
			{(int)SDL_Keycode.SDLK_INSERT, Keys.Insert},
			{(int)SDL_Keycode.SDLK_MINUS, Keys.OemMinus},
			{(int)SDL_Keycode.SDLK_NUMLOCKCLEAR, Keys.NumLock},
			{(int)SDL_Keycode.SDLK_PAGEUP, Keys.PageUp},
			{(int)SDL_Keycode.SDLK_PAGEDOWN, Keys.PageDown},
			{(int)SDL_Keycode.SDLK_PAUSE, Keys.Pause},
			{(int)SDL_Keycode.SDLK_PERIOD, Keys.OemPeriod},
			{(int)SDL_Keycode.SDLK_EQUALS, Keys.OemPlus},
			{(int)SDL_Keycode.SDLK_PRINTSCREEN, Keys.PrintScreen},
			{(int)SDL_Keycode.SDLK_QUOTE, Keys.OemQuotes},
			{(int)SDL_Keycode.SDLK_SCROLLLOCK, Keys.Scroll},
			{(int)SDL_Keycode.SDLK_SEMICOLON, Keys.OemSemicolon},
			{(int)SDL_Keycode.SDLK_SLEEP, Keys.Sleep},
			{(int)SDL_Keycode.SDLK_TAB, Keys.Tab},
			{(int)SDL_Keycode.SDLK_BACKQUOTE, Keys.OemTilde},
			{(int)SDL_Keycode.SDLK_VOLUMEUP, Keys.VolumeUp},
			{(int)SDL_Keycode.SDLK_VOLUMEDOWN, Keys.VolumeDown},
			// ReSharper disable once CommentTypo
			{'²' /* FIXME: AZERTY SDL2? -flibit */, Keys.OemTilde},
			// ReSharper disable once CommentTypo
			{'é' /* FIXME: BEPO SDL2? -flibit */, Keys.None},
			{'|' /* FIXME: Norwegian SDL2? -flibit */, Keys.OemPipe},
			{'+' /* FIXME: Norwegian SDL2? -flibit */, Keys.OemPlus},
			{'ø' /* FIXME: Norwegian SDL2? -flibit */, Keys.OemSemicolon},
			{'æ' /* FIXME: Norwegian SDL2? -flibit */, Keys.OemQuotes},
			{(int)SDL_Keycode.SDLK_UNKNOWN, Keys.None}
		};

		private static readonly Dictionary<int, Keys> _scanMap = new()
		{
			{(int)SDL_Scancode.SDL_SCANCODE_A, Keys.A},
			{(int)SDL_Scancode.SDL_SCANCODE_B, Keys.B},
			{(int)SDL_Scancode.SDL_SCANCODE_C, Keys.C},
			{(int)SDL_Scancode.SDL_SCANCODE_D, Keys.D},
			{(int)SDL_Scancode.SDL_SCANCODE_E, Keys.E},
			{(int)SDL_Scancode.SDL_SCANCODE_F, Keys.F},
			{(int)SDL_Scancode.SDL_SCANCODE_G, Keys.G},
			{(int)SDL_Scancode.SDL_SCANCODE_H, Keys.H},
			{(int)SDL_Scancode.SDL_SCANCODE_I, Keys.I},
			{(int)SDL_Scancode.SDL_SCANCODE_J, Keys.J},
			{(int)SDL_Scancode.SDL_SCANCODE_K, Keys.K},
			{(int)SDL_Scancode.SDL_SCANCODE_L, Keys.L},
			{(int)SDL_Scancode.SDL_SCANCODE_M, Keys.M},
			{(int)SDL_Scancode.SDL_SCANCODE_N, Keys.N},
			{(int)SDL_Scancode.SDL_SCANCODE_O, Keys.O},
			{(int)SDL_Scancode.SDL_SCANCODE_P, Keys.P},
			{(int)SDL_Scancode.SDL_SCANCODE_Q, Keys.Q},
			{(int)SDL_Scancode.SDL_SCANCODE_R, Keys.R},
			{(int)SDL_Scancode.SDL_SCANCODE_S, Keys.S},
			{(int)SDL_Scancode.SDL_SCANCODE_T, Keys.T},
			{(int)SDL_Scancode.SDL_SCANCODE_U, Keys.U},
			{(int)SDL_Scancode.SDL_SCANCODE_V, Keys.V},
			{(int)SDL_Scancode.SDL_SCANCODE_W, Keys.W},
			{(int)SDL_Scancode.SDL_SCANCODE_X, Keys.X},
			{(int)SDL_Scancode.SDL_SCANCODE_Y, Keys.Y},
			{(int)SDL_Scancode.SDL_SCANCODE_Z, Keys.Z},
			{(int)SDL_Scancode.SDL_SCANCODE_0, Keys.D0},
			{(int)SDL_Scancode.SDL_SCANCODE_1, Keys.D1},
			{(int)SDL_Scancode.SDL_SCANCODE_2, Keys.D2},
			{(int)SDL_Scancode.SDL_SCANCODE_3, Keys.D3},
			{(int)SDL_Scancode.SDL_SCANCODE_4, Keys.D4},
			{(int)SDL_Scancode.SDL_SCANCODE_5, Keys.D5},
			{(int)SDL_Scancode.SDL_SCANCODE_6, Keys.D6},
			{(int)SDL_Scancode.SDL_SCANCODE_7, Keys.D7},
			{(int)SDL_Scancode.SDL_SCANCODE_8, Keys.D8},
			{(int)SDL_Scancode.SDL_SCANCODE_9, Keys.D9},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_0, Keys.NumPad0},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_1, Keys.NumPad1},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_2, Keys.NumPad2},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_3, Keys.NumPad3},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_4, Keys.NumPad4},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_5, Keys.NumPad5},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_6, Keys.NumPad6},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_7, Keys.NumPad7},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_8, Keys.NumPad8},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_9, Keys.NumPad9},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_CLEAR, Keys.OemClear},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_DECIMAL, Keys.Decimal},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_DIVIDE, Keys.Divide},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_ENTER, Keys.Enter},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_MINUS, Keys.Subtract},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY, Keys.Multiply},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_PERIOD, Keys.OemPeriod},
			{(int)SDL_Scancode.SDL_SCANCODE_KP_PLUS, Keys.Add},
			{(int)SDL_Scancode.SDL_SCANCODE_F1, Keys.F1},
			{(int)SDL_Scancode.SDL_SCANCODE_F2, Keys.F2},
			{(int)SDL_Scancode.SDL_SCANCODE_F3, Keys.F3},
			{(int)SDL_Scancode.SDL_SCANCODE_F4, Keys.F4},
			{(int)SDL_Scancode.SDL_SCANCODE_F5, Keys.F5},
			{(int)SDL_Scancode.SDL_SCANCODE_F6, Keys.F6},
			{(int)SDL_Scancode.SDL_SCANCODE_F7, Keys.F7},
			{(int)SDL_Scancode.SDL_SCANCODE_F8, Keys.F8},
			{(int)SDL_Scancode.SDL_SCANCODE_F9, Keys.F9},
			{(int)SDL_Scancode.SDL_SCANCODE_F10, Keys.F10},
			{(int)SDL_Scancode.SDL_SCANCODE_F11, Keys.F11},
			{(int)SDL_Scancode.SDL_SCANCODE_F12, Keys.F12},
			{(int)SDL_Scancode.SDL_SCANCODE_F13, Keys.F13},
			{(int)SDL_Scancode.SDL_SCANCODE_F14, Keys.F14},
			{(int)SDL_Scancode.SDL_SCANCODE_F15, Keys.F15},
			{(int)SDL_Scancode.SDL_SCANCODE_F16, Keys.F16},
			{(int)SDL_Scancode.SDL_SCANCODE_F17, Keys.F17},
			{(int)SDL_Scancode.SDL_SCANCODE_F18, Keys.F18},
			{(int)SDL_Scancode.SDL_SCANCODE_F19, Keys.F19},
			{(int)SDL_Scancode.SDL_SCANCODE_F20, Keys.F20},
			{(int)SDL_Scancode.SDL_SCANCODE_F21, Keys.F21},
			{(int)SDL_Scancode.SDL_SCANCODE_F22, Keys.F22},
			{(int)SDL_Scancode.SDL_SCANCODE_F23, Keys.F23},
			{(int)SDL_Scancode.SDL_SCANCODE_F24, Keys.F24},
			{(int)SDL_Scancode.SDL_SCANCODE_SPACE, Keys.Space},
			{(int)SDL_Scancode.SDL_SCANCODE_UP, Keys.Up},
			{(int)SDL_Scancode.SDL_SCANCODE_DOWN, Keys.Down},
			{(int)SDL_Scancode.SDL_SCANCODE_LEFT, Keys.Left},
			{(int)SDL_Scancode.SDL_SCANCODE_RIGHT, Keys.Right},
			{(int)SDL_Scancode.SDL_SCANCODE_LALT, Keys.LeftAlt},
			{(int)SDL_Scancode.SDL_SCANCODE_RALT, Keys.RightAlt},
			{(int)SDL_Scancode.SDL_SCANCODE_LCTRL, Keys.LeftControl},
			{(int)SDL_Scancode.SDL_SCANCODE_RCTRL, Keys.RightControl},
			{(int)SDL_Scancode.SDL_SCANCODE_LGUI, Keys.LeftWindows},
			{(int)SDL_Scancode.SDL_SCANCODE_RGUI, Keys.RightWindows},
			{(int)SDL_Scancode.SDL_SCANCODE_LSHIFT, Keys.LeftShift},
			{(int)SDL_Scancode.SDL_SCANCODE_RSHIFT, Keys.RightShift},
			{(int)SDL_Scancode.SDL_SCANCODE_APPLICATION, Keys.Apps},
			{(int)SDL_Scancode.SDL_SCANCODE_MENU, Keys.Apps},
			{(int)SDL_Scancode.SDL_SCANCODE_SLASH, Keys.OemQuestion},
			{(int)SDL_Scancode.SDL_SCANCODE_BACKSLASH, Keys.OemPipe},
			{(int)SDL_Scancode.SDL_SCANCODE_LEFTBRACKET, Keys.OemOpenBrackets},
			{(int)SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET, Keys.OemCloseBrackets},
			{(int)SDL_Scancode.SDL_SCANCODE_CAPSLOCK, Keys.CapsLock},
			{(int)SDL_Scancode.SDL_SCANCODE_COMMA, Keys.OemComma},
			{(int)SDL_Scancode.SDL_SCANCODE_DELETE, Keys.Delete},
			{(int)SDL_Scancode.SDL_SCANCODE_END, Keys.End},
			{(int)SDL_Scancode.SDL_SCANCODE_BACKSPACE, Keys.Back},
			{(int)SDL_Scancode.SDL_SCANCODE_RETURN, Keys.Enter},
			{(int)SDL_Scancode.SDL_SCANCODE_ESCAPE, Keys.Escape},
			{(int)SDL_Scancode.SDL_SCANCODE_HOME, Keys.Home},
			{(int)SDL_Scancode.SDL_SCANCODE_INSERT, Keys.Insert},
			{(int)SDL_Scancode.SDL_SCANCODE_MINUS, Keys.OemMinus},
			{(int)SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR, Keys.NumLock},
			{(int)SDL_Scancode.SDL_SCANCODE_PAGEUP, Keys.PageUp},
			{(int)SDL_Scancode.SDL_SCANCODE_PAGEDOWN, Keys.PageDown},
			{(int)SDL_Scancode.SDL_SCANCODE_PAUSE, Keys.Pause},
			{(int)SDL_Scancode.SDL_SCANCODE_PERIOD, Keys.OemPeriod},
			{(int)SDL_Scancode.SDL_SCANCODE_EQUALS, Keys.OemPlus},
			{(int)SDL_Scancode.SDL_SCANCODE_PRINTSCREEN, Keys.PrintScreen},
			{(int)SDL_Scancode.SDL_SCANCODE_APOSTROPHE, Keys.OemQuotes},
			{(int)SDL_Scancode.SDL_SCANCODE_SCROLLLOCK, Keys.Scroll},
			{(int)SDL_Scancode.SDL_SCANCODE_SEMICOLON, Keys.OemSemicolon},
			{(int)SDL_Scancode.SDL_SCANCODE_SLEEP, Keys.Sleep},
			{(int)SDL_Scancode.SDL_SCANCODE_TAB, Keys.Tab},
			{(int)SDL_Scancode.SDL_SCANCODE_GRAVE, Keys.OemTilde},
			{(int)SDL_Scancode.SDL_SCANCODE_VOLUMEUP, Keys.VolumeUp},
			{(int)SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN, Keys.VolumeDown},
			{(int)SDL_Scancode.SDL_SCANCODE_UNKNOWN, Keys.None},
			/* FIXME: The following scancodes need verification! */
			{(int)SDL_Scancode.SDL_SCANCODE_NONUSHASH, Keys.None},
			{(int)SDL_Scancode.SDL_SCANCODE_NONUSBACKSLASH, Keys.None}
		};

		private static readonly Dictionary<int, SDL_Scancode> _xnaMap =
			new()
			{
				{(int)Keys.A, SDL_Scancode.SDL_SCANCODE_A},
				{(int)Keys.B, SDL_Scancode.SDL_SCANCODE_B},
				{(int)Keys.C, SDL_Scancode.SDL_SCANCODE_C},
				{(int)Keys.D, SDL_Scancode.SDL_SCANCODE_D},
				{(int)Keys.E, SDL_Scancode.SDL_SCANCODE_E},
				{(int)Keys.F, SDL_Scancode.SDL_SCANCODE_F},
				{(int)Keys.G, SDL_Scancode.SDL_SCANCODE_G},
				{(int)Keys.H, SDL_Scancode.SDL_SCANCODE_H},
				{(int)Keys.I, SDL_Scancode.SDL_SCANCODE_I},
				{(int)Keys.J, SDL_Scancode.SDL_SCANCODE_J},
				{(int)Keys.K, SDL_Scancode.SDL_SCANCODE_K},
				{(int)Keys.L, SDL_Scancode.SDL_SCANCODE_L},
				{(int)Keys.M, SDL_Scancode.SDL_SCANCODE_M},
				{(int)Keys.N, SDL_Scancode.SDL_SCANCODE_N},
				{(int)Keys.O, SDL_Scancode.SDL_SCANCODE_O},
				{(int)Keys.P, SDL_Scancode.SDL_SCANCODE_P},
				{(int)Keys.Q, SDL_Scancode.SDL_SCANCODE_Q},
				{(int)Keys.R, SDL_Scancode.SDL_SCANCODE_R},
				{(int)Keys.S, SDL_Scancode.SDL_SCANCODE_S},
				{(int)Keys.T, SDL_Scancode.SDL_SCANCODE_T},
				{(int)Keys.U, SDL_Scancode.SDL_SCANCODE_U},
				{(int)Keys.V, SDL_Scancode.SDL_SCANCODE_V},
				{(int)Keys.W, SDL_Scancode.SDL_SCANCODE_W},
				{(int)Keys.X, SDL_Scancode.SDL_SCANCODE_X},
				{(int)Keys.Y, SDL_Scancode.SDL_SCANCODE_Y},
				{(int)Keys.Z, SDL_Scancode.SDL_SCANCODE_Z},
				{(int)Keys.D0, SDL_Scancode.SDL_SCANCODE_0},
				{(int)Keys.D1, SDL_Scancode.SDL_SCANCODE_1},
				{(int)Keys.D2, SDL_Scancode.SDL_SCANCODE_2},
				{(int)Keys.D3, SDL_Scancode.SDL_SCANCODE_3},
				{(int)Keys.D4, SDL_Scancode.SDL_SCANCODE_4},
				{(int)Keys.D5, SDL_Scancode.SDL_SCANCODE_5},
				{(int)Keys.D6, SDL_Scancode.SDL_SCANCODE_6},
				{(int)Keys.D7, SDL_Scancode.SDL_SCANCODE_7},
				{(int)Keys.D8, SDL_Scancode.SDL_SCANCODE_8},
				{(int)Keys.D9, SDL_Scancode.SDL_SCANCODE_9},
				{(int)Keys.NumPad0, SDL_Scancode.SDL_SCANCODE_KP_0},
				{(int)Keys.NumPad1, SDL_Scancode.SDL_SCANCODE_KP_1},
				{(int)Keys.NumPad2, SDL_Scancode.SDL_SCANCODE_KP_2},
				{(int)Keys.NumPad3, SDL_Scancode.SDL_SCANCODE_KP_3},
				{(int)Keys.NumPad4, SDL_Scancode.SDL_SCANCODE_KP_4},
				{(int)Keys.NumPad5, SDL_Scancode.SDL_SCANCODE_KP_5},
				{(int)Keys.NumPad6, SDL_Scancode.SDL_SCANCODE_KP_6},
				{(int)Keys.NumPad7, SDL_Scancode.SDL_SCANCODE_KP_7},
				{(int)Keys.NumPad8, SDL_Scancode.SDL_SCANCODE_KP_8},
				{(int)Keys.NumPad9, SDL_Scancode.SDL_SCANCODE_KP_9},
				{(int)Keys.OemClear, SDL_Scancode.SDL_SCANCODE_KP_CLEAR},
				{(int)Keys.Decimal, SDL_Scancode.SDL_SCANCODE_KP_DECIMAL},
				{(int)Keys.Divide, SDL_Scancode.SDL_SCANCODE_KP_DIVIDE},
				{(int)Keys.Multiply, SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY},
				{(int)Keys.Subtract, SDL_Scancode.SDL_SCANCODE_KP_MINUS},
				{(int)Keys.Add, SDL_Scancode.SDL_SCANCODE_KP_PLUS},
				{(int)Keys.F1, SDL_Scancode.SDL_SCANCODE_F1},
				{(int)Keys.F2, SDL_Scancode.SDL_SCANCODE_F2},
				{(int)Keys.F3, SDL_Scancode.SDL_SCANCODE_F3},
				{(int)Keys.F4, SDL_Scancode.SDL_SCANCODE_F4},
				{(int)Keys.F5, SDL_Scancode.SDL_SCANCODE_F5},
				{(int)Keys.F6, SDL_Scancode.SDL_SCANCODE_F6},
				{(int)Keys.F7, SDL_Scancode.SDL_SCANCODE_F7},
				{(int)Keys.F8, SDL_Scancode.SDL_SCANCODE_F8},
				{(int)Keys.F9, SDL_Scancode.SDL_SCANCODE_F9},
				{(int)Keys.F10, SDL_Scancode.SDL_SCANCODE_F10},
				{(int)Keys.F11, SDL_Scancode.SDL_SCANCODE_F11},
				{(int)Keys.F12, SDL_Scancode.SDL_SCANCODE_F12},
				{(int)Keys.F13, SDL_Scancode.SDL_SCANCODE_F13},
				{(int)Keys.F14, SDL_Scancode.SDL_SCANCODE_F14},
				{(int)Keys.F15, SDL_Scancode.SDL_SCANCODE_F15},
				{(int)Keys.F16, SDL_Scancode.SDL_SCANCODE_F16},
				{(int)Keys.F17, SDL_Scancode.SDL_SCANCODE_F17},
				{(int)Keys.F18, SDL_Scancode.SDL_SCANCODE_F18},
				{(int)Keys.F19, SDL_Scancode.SDL_SCANCODE_F19},
				{(int)Keys.F20, SDL_Scancode.SDL_SCANCODE_F20},
				{(int)Keys.F21, SDL_Scancode.SDL_SCANCODE_F21},
				{(int)Keys.F22, SDL_Scancode.SDL_SCANCODE_F22},
				{(int)Keys.F23, SDL_Scancode.SDL_SCANCODE_F23},
				{(int)Keys.F24, SDL_Scancode.SDL_SCANCODE_F24},
				{(int)Keys.Space, SDL_Scancode.SDL_SCANCODE_SPACE},
				{(int)Keys.Up, SDL_Scancode.SDL_SCANCODE_UP},
				{(int)Keys.Down, SDL_Scancode.SDL_SCANCODE_DOWN},
				{(int)Keys.Left, SDL_Scancode.SDL_SCANCODE_LEFT},
				{(int)Keys.Right, SDL_Scancode.SDL_SCANCODE_RIGHT},
				{(int)Keys.LeftAlt, SDL_Scancode.SDL_SCANCODE_LALT},
				{(int)Keys.RightAlt, SDL_Scancode.SDL_SCANCODE_RALT},
				{(int)Keys.LeftControl, SDL_Scancode.SDL_SCANCODE_LCTRL},
				{(int)Keys.RightControl, SDL_Scancode.SDL_SCANCODE_RCTRL},
				{(int)Keys.LeftWindows, SDL_Scancode.SDL_SCANCODE_LGUI},
				{(int)Keys.RightWindows, SDL_Scancode.SDL_SCANCODE_RGUI},
				{(int)Keys.LeftShift, SDL_Scancode.SDL_SCANCODE_LSHIFT},
				{(int)Keys.RightShift, SDL_Scancode.SDL_SCANCODE_RSHIFT},
				{(int)Keys.Apps, SDL_Scancode.SDL_SCANCODE_APPLICATION},
				{(int)Keys.OemQuestion, SDL_Scancode.SDL_SCANCODE_SLASH},
				{(int)Keys.OemPipe, SDL_Scancode.SDL_SCANCODE_BACKSLASH},
				{(int)Keys.OemOpenBrackets, SDL_Scancode.SDL_SCANCODE_LEFTBRACKET},
				{(int)Keys.OemCloseBrackets, SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET},
				{(int)Keys.CapsLock, SDL_Scancode.SDL_SCANCODE_CAPSLOCK},
				{(int)Keys.OemComma, SDL_Scancode.SDL_SCANCODE_COMMA},
				{(int)Keys.Delete, SDL_Scancode.SDL_SCANCODE_DELETE},
				{(int)Keys.End, SDL_Scancode.SDL_SCANCODE_END},
				{(int)Keys.Back, SDL_Scancode.SDL_SCANCODE_BACKSPACE},
				{(int)Keys.Enter, SDL_Scancode.SDL_SCANCODE_RETURN},
				{(int)Keys.Escape, SDL_Scancode.SDL_SCANCODE_ESCAPE},
				{(int)Keys.Home, SDL_Scancode.SDL_SCANCODE_HOME},
				{(int)Keys.Insert, SDL_Scancode.SDL_SCANCODE_INSERT},
				{(int)Keys.OemMinus, SDL_Scancode.SDL_SCANCODE_MINUS},
				{(int)Keys.NumLock, SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR},
				{(int)Keys.PageUp, SDL_Scancode.SDL_SCANCODE_PAGEUP},
				{(int)Keys.PageDown, SDL_Scancode.SDL_SCANCODE_PAGEDOWN},
				{(int)Keys.Pause, SDL_Scancode.SDL_SCANCODE_PAUSE},
				{(int)Keys.OemPeriod, SDL_Scancode.SDL_SCANCODE_PERIOD},
				{(int)Keys.OemPlus, SDL_Scancode.SDL_SCANCODE_EQUALS},
				{(int)Keys.PrintScreen, SDL_Scancode.SDL_SCANCODE_PRINTSCREEN},
				{(int)Keys.OemQuotes, SDL_Scancode.SDL_SCANCODE_APOSTROPHE},
				{(int)Keys.Scroll, SDL_Scancode.SDL_SCANCODE_SCROLLLOCK},
				{(int)Keys.OemSemicolon, SDL_Scancode.SDL_SCANCODE_SEMICOLON},
				{(int)Keys.Sleep, SDL_Scancode.SDL_SCANCODE_SLEEP},
				{(int)Keys.Tab, SDL_Scancode.SDL_SCANCODE_TAB},
				{(int)Keys.OemTilde, SDL_Scancode.SDL_SCANCODE_GRAVE},
				{(int)Keys.VolumeUp, SDL_Scancode.SDL_SCANCODE_VOLUMEUP},
				{(int)Keys.VolumeDown, SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN},
				{(int)Keys.None, SDL_Scancode.SDL_SCANCODE_UNKNOWN}
			};

		public static void ShowRuntimeError(string title, string message) =>
			SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, title, message, IntPtr.Zero);

		public static string ProgramInit(LaunchParameters args)
		{
			// This is how we can weed out cases where fna libs is missing
			try
			{
				_osVersion = SDL_GetPlatform();
			}
			catch (DllNotFoundException)
			{
				FNALoggerEXT.LogError!("SDL2 was not found! Do you have FNA native libraries?");
				throw;
			}
			catch (BadImageFormatException e)
			{
				string error =
					$"This process is {(IntPtr.Size == 4 ? "32" : "64")}-bit, the DLL is {(IntPtr.Size == 4 ? "64" : "32")}-bit!";

				FNALoggerEXT.LogError!(error);
				throw new BadImageFormatException(error, e);
			}

			/* SDL2 might complain if an OS that uses SDL_main has not actually
			 * used SDL_main by the time you initialize SDL2.
			 * The only platform that is affected is Windows, but we can skip
			 * their WinMain. This was only added to prevent iOS from exploding.
			 * -flibit
			 */
			SDL_SetMainReady();

			/* A number of platforms don't support global mouse, but
			 * this really only matters on desktop where the game
			 * screen may not be covering the whole display.
			 */
			if (_osVersion.Equals("Windows", StringComparison.Ordinal) ||
			    _osVersion.Equals("Mac OS X", StringComparison.Ordinal) ||
			    _osVersion.Equals("Linux", StringComparison.Ordinal) ||
			    _osVersion.Equals("FreeBSD", StringComparison.Ordinal) ||
			    _osVersion.Equals("OpenBSD", StringComparison.Ordinal) ||
			    _osVersion.Equals("NetBSD", StringComparison.Ordinal))
			{
				_supportsGlobalMouse = true;
			}
			else
			{
				_supportsGlobalMouse = false;
			}

			// Also, Windows is an idiot. -flibit
			if (_osVersion.Equals("Windows", StringComparison.Ordinal) ||
			    _osVersion.Equals("WinRT", StringComparison.Ordinal))
			{
				// Visual Studio is an idiot.
				if (Debugger.IsAttached)
				{
					SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
				}

				/* Windows has terrible event pumping and doesn't give us
				 * WM_PAINT events correctly. So we get to do this!
				 * -flibit
				 */
				SDL_GetEventFilter(out _prevEventFilter, out var prevUserData);
				SDL_SetEventFilter(_win32OnPaint, prevUserData);
			}

			/* Mount TitleLocation.Path */
			var titleLocation = GetBaseDirectory();

			// If available, load the SDL_GameControllerDB
			// ReSharper disable once StringLiteralTypo
			var mappingsDB = Path.Combine(titleLocation, "gamecontrollerdb.txt");
			if (File.Exists(mappingsDB))
			{
				SDL_GameControllerAddMappingsFromFile(mappingsDB);
			}

			// Built-in SDL2 command line arguments
			// ReSharper disable once StringLiteralTypo
			if (args.TryGetValue("glprofile", out var arg))
			{
				switch (arg)
				{
					case "es3":
						Environment.SetEnvironmentVariable("FNA3D_OPENGL_FORCE_ES3", "1");
						break;
					case "core":
						Environment.SetEnvironmentVariable("FNA3D_OPENGL_FORCE_CORE_PROFILE", "1");
						break;
					case "compatibility":
						Environment.SetEnvironmentVariable("FNA3D_OPENGL_FORCE_COMPATIBILITY_PROFILE", "1");
						break;
				}
			}

			if (args.TryGetValue("angle", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable("FNA3D_OPENGL_FORCE_ES3", "1");
				Environment.SetEnvironmentVariable("SDL_OPENGL_ES_DRIVER", "1");
			}

			if (args.TryGetValue("forcemailboxvsync", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable("FNA3D_VULKAN_FORCE_MAILBOX_VSYNC", "1");
			}

			// This _should_ be the first real SDL call we make...
			SDL_Init(SDL_INIT_VIDEO | SDL_INIT_JOYSTICK | SDL_INIT_GAMECONTROLLER | SDL_INIT_HAPTIC);

			// Set any hints to match XNA4 behavior...
			string hint = SDL_GetHint(SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS);
			if (string.IsNullOrEmpty(hint))
			{
				SDL_SetHint(SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS, "1");
			}

			/* By default, assume physical layout, since XNA games mostly assume XInput.
			 * This used to be more flexible until Steam decided to enforce the variable
			 * that already had their desired value as the default (big surprise).
			 *
			 * TL;DR: Suck my ass, Steam
			 *
			 * -flibit
			 */
			SDL_SetHintWithPriority(SDL_HINT_GAMECONTROLLER_USE_BUTTON_LABELS, "0", SDL_HintPriority.SDL_HINT_OVERRIDE);

			SDL_SetHint(SDL_HINT_ORIENTATIONS, "LandscapeLeft LandscapeRight Portrait");

			// We want to initialize the controllers ASAP!
			SDL_Event[] evt = new SDL_Event[1];
			SDL_PumpEvents();
			while (SDL_PeepEvents(
				evt,
				1,
				SDL_eventaction.SDL_GETEVENT,
				SDL_EventType.SDL_CONTROLLERDEVICEADDED,
				SDL_EventType.SDL_CONTROLLERDEVICEADDED) == 1)
			{
				INTERNAL_AddInstance(evt[0].cdevice.which);
			}

			return titleLocation;
		}

		public static void ProgramExit(object? sender, EventArgs e)
		{
			AudioEngine.ProgramExiting = true;

			if (SoundEffect.FAudioContext.Context != null)
			{
				SoundEffect.FAudioContext.Context.Dispose();
			}

			MediaPlayer.DisposeIfNecessary();

			// This _should_ be the last SDL call we make...
			SDL_Quit();
		}

		public static GameWindow CreateWindow()
		{
			// Set and initialize the SDL2 window
			var initFlags = SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
			                SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS |
			                (SDL_WindowFlags)FNA3D.FNA3D_PrepareWindowAttributes();

			var cachePath = SDL_GetHint("FNA3D_VULKAN_PIPELINE_CACHE_FILE_NAME");
			if (cachePath == null)
			{
				if (_osVersion.Equals("Windows", StringComparison.Ordinal) ||
				    _osVersion.Equals("Mac OS X", StringComparison.Ordinal) ||
				    _osVersion.Equals("Linux", StringComparison.Ordinal) ||
				    _osVersion.Equals("FreeBSD", StringComparison.Ordinal) ||
				    _osVersion.Equals("OpenBSD", StringComparison.Ordinal) ||
				    _osVersion.Equals("NetBSD", StringComparison.Ordinal))
				{
#if DEBUG // Save pipeline cache files to the base directory for debug builds
					var exeName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName).Replace(".vshost", string.Empty);
					cachePath = Path.Combine(
						SDL_GetPrefPath(null, "FNA3D"),
						exeName + "_Vulkan_PipelineCache.blob");
#endif
				}
				else
				{
					/* For all non-desktop targets, disable
					  * the pipeline cache. There is usually
					  * some specialized path you have to
					  * take to use pipeline cache files, so
					  * developers will have to do things the
					  * hard way over there.
					  */
					cachePath = string.Empty;
				}

				SDL_SetHint(
					"FNA3D_VULKAN_PIPELINE_CACHE_FILE_NAME",
					cachePath);
			}

			if (Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1")
			{
				initFlags |= SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;
			}

			string title = AssemblyHelper.GetDefaultWindowTitle();
			var window = SDL_CreateWindow(
				title,
				SDL_WINDOWPOS_CENTERED,
				SDL_WINDOWPOS_CENTERED,
				GraphicsDeviceManager.DefaultBackBufferWidth,
				GraphicsDeviceManager.DefaultBackBufferHeight,
				initFlags);

			if (window == IntPtr.Zero)
			{
				/* If this happens, the GL attributes were
				 * rejected by the platform. This is EXTREMELY
				 * rare (unless you're on Android, of course).
				 */
				throw new NoSuitableGraphicsDeviceException(SDL_GetError());
			}

			INTERNAL_SetIcon(window, title);

			// Disable the screensaver.
			SDL_DisableScreenSaver();

			// We hide the mouse cursor by default.
			OnIsMouseVisibleChanged(false);

			/* If high DPI is not found, unset the HIGHDPI var.
			 * This is our way to communicate that it failed...
			 * -flibit
			 */
			FNA3D.FNA3D_GetDrawableSize(window, out var drawX, out var drawY);
			if (drawX == GraphicsDeviceManager.DefaultBackBufferWidth &&
			    drawY == GraphicsDeviceManager.DefaultBackBufferHeight)
			{
				Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "0");
			}
			else
			{
				// Store the full retina resolution of the display
				_retinaWidth = drawX;
				_retinaHeight = drawY;
			}

			return new FNAWindow(window, @"\\.\DISPLAY" + (SDL_GetWindowDisplayIndex(window) + 1));
		}

		public static void DisposeWindow(GameWindow window)
		{
			/* Some window managers might try to minimize the window as we're
			 * destroying it. This looks pretty stupid and could cause problems,
			 * so set this hint right before we destroy everything.
			 * -flibit
			 */
			SDL_SetHintWithPriority(SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS, "0", SDL_HintPriority.SDL_HINT_OVERRIDE);

			if (Mouse.WindowHandle == window.Handle)
			{
				Mouse.WindowHandle = IntPtr.Zero;
			}

			if (TouchPanel.WindowHandle == window.Handle)
			{
				TouchPanel.WindowHandle = IntPtr.Zero;
			}

			SDL_DestroyWindow(window.Handle);
		}

		public static void ApplyWindowChanges(
			IntPtr window,
			int clientWidth,
			int clientHeight,
			bool wantsFullscreen,
			string screenDeviceName,
			ref string resultDeviceName)
		{
			var center = false;
			if (Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1" &&
			    !string.IsNullOrEmpty(_osVersion) &&
			    _osVersion.Equals("Mac OS X", StringComparison.Ordinal))
			{
				/* For high-DPI windows, halve the size!
				 * The drawable size is now the primary width/height, so
				 * the window needs to accommodate the GL viewport.
				 * -flibit
				 */
				clientWidth /= 2;
				clientHeight /= 2;
			}

			// When windowed, set the size before moving
			if (!wantsFullscreen)
			{
				bool resize;
				if ((SDL_GetWindowFlags(window) & (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0)
				{
					var apiResult = SDL_SetWindowFullscreen(window, 0);
					if (apiResult != 0)
					{
						FNALoggerEXT.LogError!(SDL_GetError());
					}

					resize = true;
				}
				else
				{
					SDL_GetWindowSize(window, out var w, out var h);
					resize = clientWidth != w || clientHeight != h;
				}

				if (resize)
				{
					SDL_RestoreWindow(window);
					SDL_SetWindowSize(window, clientWidth, clientHeight);
					center = true;
				}
			}

			// Get on the right display!
			var displayIndex = 0;
			for (var i = 0; i < GraphicsAdapter.Adapters.Count; i += 1)
			{
				if (screenDeviceName == GraphicsAdapter.Adapters[i].DeviceName)
				{
					displayIndex = i;
					break;
				}
			}

			// Just to be sure, become a window first before changing displays
			if (resultDeviceName != screenDeviceName)
			{
				SDL_SetWindowFullscreen(window, 0);
				resultDeviceName = screenDeviceName;
				center = true;
			}

			// Window always gets centered on changes, per XNA behavior
			if (center)
			{
				var pos = SDL_WINDOWPOS_CENTERED_DISPLAY(displayIndex);
				SDL_SetWindowPosition(
					window,
					pos,
					pos);
			}

			// Set fullscreen after we've done all the ugly stuff.
			if (wantsFullscreen)
			{
				if ((SDL_GetWindowFlags(window) & (uint)SDL_WindowFlags.SDL_WINDOW_SHOWN) == 0)
				{
					/* If we're still hidden, we can't actually go fullscreen yet.
					 * But, we can at least set the hidden window size to match
					 * what the window/drawable sizes will eventually be later.
					 * -flibit
					 */
					var apiResultGetCurrentDisplayMode = SDL_GetCurrentDisplayMode(
						displayIndex,
						out var mode);

					if (apiResultGetCurrentDisplayMode != 0)
					{
						FNALoggerEXT.LogError!(SDL_GetError());
					}

					SDL_SetWindowSize(window, mode.w, mode.h);
				}

				var apiResultSetWindowFullscreen = SDL_SetWindowFullscreen(
					window,
					(uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP);

				if (apiResultSetWindowFullscreen != 0)
				{
					FNALoggerEXT.LogError!(SDL_GetError());
				}
			}

			// Update the mouse window bounds
			if (Mouse.WindowHandle == window)
			{
				var b = GetWindowBounds(window);
				Mouse._windowWidth = b.Width;
				Mouse._windowHeight = b.Height;
			}
		}

		public static Rectangle GetWindowBounds(IntPtr window)
		{
			Rectangle result = default;
			if ((SDL_GetWindowFlags(window) & (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0)
			{
				/* FIXME: SDL2 b u g
				 * SDL is a little weird about SDL_GetWindowSize.
				 * If you call it early enough (for example,
				 * Game.Initialize()), it reports outdated integers.
				 * So you know what, let's just use this.
				 * -flibit
				 */
				var apiResult = SDL_GetCurrentDisplayMode(SDL_GetWindowDisplayIndex(window), out var mode);
				if (apiResult != 0)
				{
					FNALoggerEXT.LogError!(SDL_GetError());
				}

				result.X = 0;
				result.Y = 0;
				result.Width = mode.w;
				result.Height = mode.h;
			}
			else
			{
				SDL_GetWindowPosition(
					window,
					out result.X,
					out result.Y);

				SDL_GetWindowSize(
					window,
					out result.Width,
					out result.Height);
			}

			return result;
		}

		public static bool GetWindowResizable(IntPtr window) => (SDL_GetWindowFlags(window) & (uint)SDL_WindowFlags.SDL_WINDOW_RESIZABLE) != 0;

		public static void SetWindowResizable(IntPtr window, bool resizable) =>
			SDL_SetWindowResizable(window, resizable ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE);

		public static bool GetWindowBorderless(IntPtr window) => (SDL_GetWindowFlags(window) & (uint)SDL_WindowFlags.SDL_WINDOW_BORDERLESS) != 0;

		public static void SetWindowBorderless(IntPtr window, bool borderless) =>
			SDL_SetWindowBordered(window, borderless ? SDL_bool.SDL_FALSE : SDL_bool.SDL_TRUE);

		public static void SetWindowTitle(IntPtr window, string title) => SDL_SetWindowTitle(window, title);

		public static void SetTextInputRectangle(Rectangle rectangle)
		{
			var rect = default(SDL_Rect);
			rect.x = rectangle.X;
			rect.y = rectangle.Y;
			rect.w = rectangle.Width;
			rect.h = rectangle.Height;
			SDL_SetTextInputRect(ref rect);
		}

		public static bool SupportsOrientationChanges() =>
			!string.IsNullOrEmpty(_osVersion) &&
			(_osVersion.Equals("iOS", StringComparison.Ordinal) || _osVersion.Equals("Android", StringComparison.Ordinal));

		public static GraphicsAdapter RegisterGame(Game game)
		{
			SDL_ShowWindow(game.Window.Handle);

			// Store this for internal event filter work
			_activeGames.Add(game);

			// Which display did we end up on?
			var displayIndex = SDL_GetWindowDisplayIndex(
				game.Window.Handle);

			return GraphicsAdapter.Adapters[displayIndex];
		}

		public static void UnregisterGame(Game game) => _activeGames.Remove(game);

		public static bool NeedsPlatformMainLoop() => SDL_GetPlatform().Equals("Emscripten", StringComparison.Ordinal);

		public static void RunPlatformMainLoop(Game game)
		{
			if (SDL_GetPlatform().Equals("Emscripten", StringComparison.Ordinal))
			{
				_emscriptenGame = game;
				emscripten_set_main_loop(
					RunEmscriptenMainLoop,
					0,
					1);
			}
			else
			{
				throw new NotSupportedException("Cannot run the main loop of an unknown platform");
			}
		}

		public static GraphicsAdapter[] GetGraphicsAdapters()
		{
			GraphicsAdapter[] adapters = new GraphicsAdapter[SDL_GetNumVideoDisplays()];
			for (var i = 0; i < adapters.Length; i += 1)
			{
				List<DisplayMode> modes = new();
				var numModes = SDL_GetNumDisplayModes(i);
				for (var j = numModes - 1; j >= 0; j -= 1)
				{
					var apiResult = SDL_GetDisplayMode(i, j, out var filler);
					if (apiResult != 0)
					{
						FNALoggerEXT.LogError!(SDL_GetError());
					}

					// Check for dupes caused by varying refresh rates.
					var dupe = false;
					// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
					foreach (DisplayMode mode in modes)
					{
						if (filler.w == mode.Width && filler.h == mode.Height)
						{
							dupe = true;
						}
					}

					if (!dupe)
					{
						modes.Add(
							new DisplayMode(
								filler.w,
								filler.h,
								SurfaceFormat.Color));
					}
				}

				adapters[i] = new GraphicsAdapter(
					new DisplayModeCollection(modes),
					@"\\.\DISPLAY" + (i + 1),
					SDL_GetDisplayName(i));
			}

			return adapters;
		}

		public static DisplayMode GetCurrentDisplayMode(int adapterIndex)
		{
			SDL_GetCurrentDisplayMode(adapterIndex, out var filler);

			if (!string.IsNullOrEmpty(_osVersion) &&
			    _osVersion.Equals("iOS", StringComparison.Ordinal) &&
			    Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1")
			{
				// Provide the actual resolution in pixels, not points.
				filler.w = _retinaWidth;
				filler.h = _retinaHeight;
			}

			return new DisplayMode(
				filler.w,
				filler.h,
				SurfaceFormat.Color);
		}

		public static void GetMouseState(
			IntPtr window,
			out int x,
			out int y,
			out ButtonState left,
			out ButtonState middle,
			out ButtonState right,
			out ButtonState x1,
			out ButtonState x2)
		{
			uint flags;
			if (GetRelativeMouseMode())
			{
				flags = SDL_GetRelativeMouseState(out x, out y);
			}
			else if (_supportsGlobalMouse)
			{
				flags = SDL_GetGlobalMouseState(out x, out y);
				SDL_GetWindowPosition(window, out var wx, out var wy);
				x -= wx;
				y -= wy;
			}
			else
			{
				/* This is inaccurate, but what can you do... */
				flags = SDL_GetMouseState(out x, out y);
			}

			left = (ButtonState)(flags & SDL_BUTTON_LMASK);
			middle = (ButtonState)((flags & SDL_BUTTON_MMASK) >> 1);
			right = (ButtonState)((flags & SDL_BUTTON_RMASK) >> 2);
			x1 = (ButtonState)((flags & SDL_BUTTON_X1MASK) >> 3);
			x2 = (ButtonState)((flags & SDL_BUTTON_X2MASK) >> 4);
		}

		public static void OnIsMouseVisibleChanged(bool visible) => SDL_ShowCursor(visible ? 1 : 0);

		public static bool GetRelativeMouseMode() => SDL_GetRelativeMouseMode() == SDL_bool.SDL_TRUE;

		public static void SetRelativeMouseMode(bool enable) => SDL_SetRelativeMouseMode(enable ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE);

		public static string GetStorageRoot()
		{
			// Generate the path of the game's save folder
			string exeName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName)
				// ReSharper disable once StringLiteralTypo
				.Replace(".vshost", string.Empty);

			// Get the OS save folder, append the EXE name
			if (!string.IsNullOrEmpty(_osVersion) &&
			    _osVersion.Equals("Windows", StringComparison.Ordinal))
			{
				return Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"SavedGames",
					exeName);
			}

			if (!string.IsNullOrEmpty(_osVersion) &&
			    _osVersion.Equals("Mac OS X", StringComparison.Ordinal))
			{
				var osConfigDir = Environment.GetEnvironmentVariable("HOME");
				if (string.IsNullOrEmpty(osConfigDir))
				{
					return "."; // Oh well.
				}

				return Path.Combine(
					osConfigDir,
					"Library/Application Support",
					exeName);
			}

			if (!string.IsNullOrEmpty(_osVersion) &&
			    (_osVersion.Equals("Linux", StringComparison.Ordinal) ||
			     _osVersion.Equals("FreeBSD", StringComparison.Ordinal) ||
			     _osVersion.Equals("OpenBSD", StringComparison.Ordinal) ||
			     _osVersion.Equals("NetBSD", StringComparison.Ordinal)))
			{
				// Assuming a non-macOS Unix platform will follow the XDG. Which it should.
				var osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (string.IsNullOrEmpty(osConfigDir))
				{
					osConfigDir = Environment.GetEnvironmentVariable("HOME");
					if (string.IsNullOrEmpty(osConfigDir))
					{
						return "."; // Oh well.
					}

					osConfigDir += "/.local/share";
				}

				return Path.Combine(osConfigDir, exeName);
			}

			/* There is a minor inaccuracy here: SDL_GetPrefPath
			 * creates the directories right away, whereas XNA will
			 * only create the directory upon creating a container.
			 * So if you create a StorageDevice and hit a property,
			 * the game folder is made early!
			 * -flibit
			 */
			return SDL_GetPrefPath(string.Empty, exeName);
		}

		public static DriveInfo? GetDriveInfo(string storageRoot)
		{
			if (!string.IsNullOrEmpty(_osVersion) &&
			    _osVersion.Equals("WinRT", StringComparison.Ordinal))
			{
				// WinRT DriveInfo is a bunch of crap -flibit
				return null;
			}

			DriveInfo? result;
			try
			{
				result = new DriveInfo(MonoPathRootWorkaround(storageRoot) ?? string.Empty);
			}
			catch (Exception e)
			{
				FNALoggerEXT.LogError!("Failed to get DriveInfo: " + e);
				result = null;
			}

			return result;
		}

		public static Microphone[] GetMicrophones()
		{
			// Init subsystem if needed
			if (!_micInit)
			{
				var apiResult = SDL_InitSubSystem(SDL_INIT_AUDIO);
				if (apiResult != 0)
				{
					FNALoggerEXT.LogError!(SDL_GetError());
				}

				_micInit = true;
			}

			// How many devices do we have...?
			var numDev = SDL_GetNumAudioDevices(1);
			if (numDev < 1)
			{
				return Array.Empty<Microphone>();
			}

			Microphone[] result = new Microphone[numDev + 1];

			// Default input format
			var want = default(SDL_AudioSpec);
			want.freq = Microphone.SamplerRate;
			want.format = AUDIO_S16;
			want.channels = 1;
			want.samples = 4096; /* FIXME: Anything specific? */

			// First mic is always OS default
			result[0] = new Microphone(
				SDL_OpenAudioDevice(
					string.Empty,
					1,
					ref want,
					out _,
					0),
				"Default Device");

			for (var i = 0; i < numDev; i += 1)
			{
				string name = SDL_GetAudioDeviceName(i, 1);
				result[i + 1] = new Microphone(
					SDL_OpenAudioDevice(
						name,
						1,
						ref want,
						out _,
						0),
					name);
			}

			return result;
		}

		public static unsafe int GetMicrophoneSamples(
			uint handle,
			byte[] buffer,
			int offset,
			int count)
		{
			fixed (byte* ptr = &buffer[offset])
			{
				return (int)SDL_DequeueAudio(handle, (IntPtr)ptr, (uint)count);
			}
		}

		public static int GetMicrophoneQueuedBytes(uint handle) => (int)SDL_GetQueuedAudioSize(handle);

		public static void StartMicrophone(uint handle) => SDL_PauseAudioDevice(handle, 0);

		public static void StopMicrophone(uint handle) => SDL_PauseAudioDevice(handle, 1);

		public static GamePadCapabilities GetGamePadCapabilities(int index)
		{
			if (_devices[index] == IntPtr.Zero)
			{
				return default;
			}

			return _capabilities[index];
		}

		public static GamePadState GetGamePadState(int index, GamePadDeadZone deadZoneMode)
		{
			var device = _devices[index];
			if (device == IntPtr.Zero)
			{
				return default;
			}

			// The "master" button state is built from this.
			Buttons buttonState = 0;

			// Sticks
			var stickLeft = new Vector2(
				SDL_GameControllerGetAxis(device, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX) / 32767.0f,
				SDL_GameControllerGetAxis(device, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY) / -32767.0f);

			var stickRight = new Vector2(
				SDL_GameControllerGetAxis(device, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX) / 32767.0f,
				SDL_GameControllerGetAxis(device, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY) / -32767.0f);

			buttonState |= READ_StickToButtons(
				stickLeft,
				Buttons.LeftThumbstickLeft,
				Buttons.LeftThumbstickRight,
				Buttons.LeftThumbstickUp,
				Buttons.LeftThumbstickDown,
				GamePad.LeftDeadZone);

			buttonState |= READ_StickToButtons(
				stickRight,
				Buttons.RightThumbstickLeft,
				Buttons.RightThumbstickRight,
				Buttons.RightThumbstickUp,
				Buttons.RightThumbstickDown,
				GamePad.RightDeadZone);

			// Triggers
			var triggerLeft =
				SDL_GameControllerGetAxis(device, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT) / 32767.0f;

			var triggerRight =
				SDL_GameControllerGetAxis(device, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT) / 32767.0f;

			if (triggerLeft > GamePad.TriggerThreshold)
			{
				buttonState |= Buttons.LeftTrigger;
			}

			if (triggerRight > GamePad.TriggerThreshold)
			{
				buttonState |= Buttons.RightTrigger;
			}

			// Buttons
			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A) != 0)
			{
				buttonState |= Buttons.A;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B) != 0)
			{
				buttonState |= Buttons.B;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X) != 0)
			{
				buttonState |= Buttons.X;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y) != 0)
			{
				buttonState |= Buttons.Y;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK) != 0)
			{
				buttonState |= Buttons.Back;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE) != 0)
			{
				buttonState |= Buttons.BigButton;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START) != 0)
			{
				buttonState |= Buttons.Start;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK) !=
			    0)
			{
				buttonState |= Buttons.LeftStick;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK) != 0)
			{
				buttonState |= Buttons.RightStick;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER) != 0)
			{
				buttonState |= Buttons.LeftShoulder;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER) != 0)
			{
				buttonState |= Buttons.RightShoulder;
			}

			// DPad
			var dpadUp = ButtonState.Released;
			var dpadDown = ButtonState.Released;
			var dpadLeft = ButtonState.Released;
			var dpadRight = ButtonState.Released;
			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP) !=
			    0)
			{
				buttonState |= Buttons.DPadUp;
				dpadUp = ButtonState.Pressed;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN) !=
			    0)
			{
				buttonState |= Buttons.DPadDown;
				dpadDown = ButtonState.Pressed;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT) !=
			    0)
			{
				buttonState |= Buttons.DPadLeft;
				dpadLeft = ButtonState.Pressed;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT) != 0)
			{
				buttonState |= Buttons.DPadRight;
				dpadRight = ButtonState.Pressed;
			}

			// Extensions
			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_MISC1) != 0)
			{
				buttonState |= Buttons.Misc1EXT;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_PADDLE1) != 0)
			{
				buttonState |= Buttons.Paddle1EXT;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_PADDLE2) != 0)
			{
				buttonState |= Buttons.Paddle2EXT;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_PADDLE3) != 0)
			{
				buttonState |= Buttons.Paddle3EXT;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_PADDLE4) != 0)
			{
				buttonState |= Buttons.Paddle4EXT;
			}

			if (SDL_GameControllerGetButton(device, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_TOUCHPAD) != 0)
			{
				buttonState |= Buttons.TouchPadEXT;
			}

			// Build the GamePadState, increment PacketNumber if state changed.
			var builtState = new GamePadState(
				new GamePadThumbSticks(stickLeft, stickRight, deadZoneMode),
				new GamePadTriggers(triggerLeft, triggerRight, deadZoneMode),
				new GamePadButtons(buttonState),
				new GamePadDPad(dpadUp, dpadDown, dpadLeft, dpadRight));

			builtState.IsConnected = true;
			builtState.PacketNumber = _states[index].PacketNumber;
			if (builtState != _states[index])
			{
				builtState.PacketNumber += 1;
				_states[index] = builtState;
			}

			return builtState;
		}

		public static bool SetGamePadVibration(int index, float leftTrigger, float rightTrigger)
		{
			var device = _devices[index];
			if (device == IntPtr.Zero)
			{
				return false;
			}

			return SDL_GameControllerRumble(
				device,
				(ushort)(MathHelper.Clamp(leftTrigger, 0.0f, 1.0f) * 0xFFFF),
				(ushort)(MathHelper.Clamp(rightTrigger, 0.0f, 1.0f) * 0xFFFF),
				0) == 0;
		}

		public static bool SetGamePadTriggerVibration(int index, float leftMotor, float rightMotor)
		{
			var device = _devices[index];
			if (device == IntPtr.Zero)
			{
				return false;
			}

			return SDL_GameControllerRumbleTriggers(
				device,
				(ushort)(MathHelper.Clamp(leftMotor, 0.0f, 1.0f) * 0xFFFF),
				(ushort)(MathHelper.Clamp(rightMotor, 0.0f, 1.0f) * 0xFFFF),
				0) == 0;
		}

		public static string GetGamePadGUID(int index) => _guids[index];

		public static void SetGamePadLightBar(int index, Color color)
		{
			var device = _devices[index];
			if (device == IntPtr.Zero)
			{
				return;
			}

			SDL_GameControllerSetLED(
				device,
				color.R,
				color.G,
				color.B);
		}

		public static bool GetGamePadGyro(int index, out Vector3 gyro)
		{
			var device = _devices[index];
			if (device == IntPtr.Zero)
			{
				gyro = Vector3.Zero;
				return false;
			}

			if (SDL_GameControllerIsSensorEnabled(device, SDL_SensorType.SDL_SENSOR_GYRO) == SDL_bool.SDL_FALSE)
			{
				SDL_GameControllerSetSensorEnabled(device, SDL_SensorType.SDL_SENSOR_GYRO, SDL_bool.SDL_TRUE);
			}

			unsafe
			{
				var data = stackalloc float[3];
				var result = SDL_GameControllerGetSensorData(
					device,
					SDL_SensorType.SDL_SENSOR_GYRO,
					(IntPtr)data,
					3);

				if (result < 0)
				{
					gyro = Vector3.Zero;
					return false;
				}

				gyro.X = data[0];
				gyro.Y = data[1];
				gyro.Z = data[2];
				return result == 0;
			}
		}

		public static bool GetGamePadAccelerometer(int index, out Vector3 accel)
		{
			var device = _devices[index];
			if (device == IntPtr.Zero)
			{
				accel = Vector3.Zero;
				return false;
			}

			if (SDL_GameControllerIsSensorEnabled(device, SDL_SensorType.SDL_SENSOR_ACCEL) == SDL_bool.SDL_FALSE)
			{
				SDL_GameControllerSetSensorEnabled(device, SDL_SensorType.SDL_SENSOR_ACCEL, SDL_bool.SDL_TRUE);
			}

			unsafe
			{
				var data = stackalloc float[3];
				var result = SDL_GameControllerGetSensorData(
					device,
					SDL_SensorType.SDL_SENSOR_ACCEL,
					(IntPtr)data,
					3);

				if (result < 0)
				{
					accel = Vector3.Zero;
					return false;
				}

				accel.X = data[0];
				accel.Y = data[1];
				accel.Z = data[2];
				return result == 0;
			}
		}

		public static TouchPanelCapabilities GetTouchCapabilities()
		{
			/* Take these reported capabilities with a grain of salt.
			 * On Windows, touch devices won't be detected until they
			 * are interacted with. Also, MaximumTouchCount is completely
			 * bogus. For any touch device, XNA always reports 4.
			 *
			 * -caleb
			 */
			var touchDeviceExists = SDL_GetNumTouchDevices() > 0;
			return new TouchPanelCapabilities(touchDeviceExists, touchDeviceExists ? 4 : 0);
		}

		public static unsafe void UpdateTouchPanelState()
		{
			// Poll the touch device for all active fingers
			var touchDevice = SDL_GetTouchDevice(0);
			for (var i = 0; i < TouchPanel.MaxTouches; i += 1)
			{
				var finger = (SDL_Finger*)SDL_GetTouchFinger(touchDevice, i);
				if (finger == null)
				{
					// No finger found at this index
					TouchPanel.SetFinger(i, TouchPanel.NoFinger, Vector2.Zero);
					continue;
				}

				// Send the finger data to the TouchPanel
				TouchPanel.SetFinger(
					i,
					(int)finger->id,
					new Vector2(
						(float)Math.Round(finger->x * TouchPanel.DisplayWidth),
						(float)Math.Round(finger->y * TouchPanel.DisplayHeight)));
			}
		}

		public static int GetNumTouchFingers() => SDL_GetNumTouchFingers(SDL_GetTouchDevice(0));

		public static Keys GetKeyFromScancode(Keys scancode)
		{
			if (UseScancodes)
			{
				return scancode;
			}

			if (_xnaMap.TryGetValue((int)scancode, out var retVal))
			{
				var sym = SDL_GetKeyFromScancode(retVal);
				if (_keyMap.TryGetValue((int)sym, out var result))
				{
					return result;
				}

				FNALoggerEXT.LogWarn!("KEYCODE MISSING FROM SDL2->XNA DICTIONARY: " + sym);
			}
			else
			{
				FNALoggerEXT.LogWarn!("SCANCODE MISSING FROM XNA->SDL2 DICTIONARY: " + scancode);
			}

			return Keys.None;
		}

		public static void PollEvents(
			Game game,
			ref GraphicsAdapter currentAdapter,
			bool[] textInputControlDown,
			int[] textInputControlRepeat,
			ref bool textInputSuppress)
		{
			while (SDL_PollEvent(out var evt) == 1)
			{
				var shouldQuit = PollEvent(ref evt, game, ref currentAdapter, textInputControlDown, textInputControlRepeat, ref textInputSuppress);
				if (shouldQuit)
				{
					break;
				}
			}

			// Text Input Controls Key Handling
			for (var i = 0; i < FNAPlatform.TextInputCharacters.Length; i += 1)
			{
				if (textInputControlDown[i] && textInputControlRepeat[i] <= Environment.TickCount)
				{
					TextInputEXT.OnTextInput(FNAPlatform.TextInputCharacters[i]);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool PollEvent(
			ref SDL_Event evt,
			Game game,
			ref GraphicsAdapter currentAdapter,
			bool[] textInputControlDown,
			int[] textInputControlRepeat,
			ref bool textInputSuppress)
		{
			// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
			switch (evt.type)
			{
				// Keyboard
				case SDL_EventType.SDL_KEYDOWN:
				{
					var key = ToXNAKey(ref evt.key.keysym);
					if (!Keyboard._keys.Contains(key))
					{
						Keyboard._keys.Add(key);
						if (FNAPlatform.TextInputBindings.TryGetValue(key, out var textIndex))
						{
							textInputControlDown[textIndex] = true;
							textInputControlRepeat[textIndex] = Environment.TickCount + 400;
							TextInputEXT.OnTextInput(FNAPlatform.TextInputCharacters[textIndex]);
						}
						else if (Keyboard._keys.Contains(Keys.LeftControl) && key == Keys.V)
						{
							textInputControlDown[6] = true;
							textInputControlRepeat[6] = Environment.TickCount + 400;
							TextInputEXT.OnTextInput(FNAPlatform.TextInputCharacters[6]);
							textInputSuppress = true;
						}
					}

					break;
				}

				// Mouse Input
				case SDL_EventType.SDL_KEYUP:
				{
					var key = ToXNAKey(ref evt.key.keysym);
					if (Keyboard._keys.Remove(key))
					{
						if (FNAPlatform.TextInputBindings.TryGetValue(key, out var value))
						{
							textInputControlDown[value] = false;
						}
						else if ((!Keyboard._keys.Contains(Keys.LeftControl) && textInputControlDown[3]) || key == Keys.V)
						{
							textInputControlDown[6] = false;
							textInputSuppress = false;
						}
					}

					break;
				}

				case SDL_EventType.SDL_MOUSEBUTTONDOWN:
					Mouse.INTERNAL_onClicked(evt.button.button - 1);
					break;
				// Touch Input
				case SDL_EventType.SDL_MOUSEWHEEL:
					// 120 units per notch. Because reasons.
					Mouse._mouseWheel += evt.wheel.y * 120;
					break;
				case SDL_EventType.SDL_FINGERDOWN:
					// Windows only notices a touch screen once it's touched
					TouchPanel.TouchDeviceExists = true;

					TouchPanel.INTERNAL_onTouchEvent(
						(int)evt.tfinger.fingerId,
						TouchLocationState.Pressed,
						evt.tfinger.x,
						evt.tfinger.y,
						0,
						0);

					break;
				case SDL_EventType.SDL_FINGERMOTION:
					TouchPanel.INTERNAL_onTouchEvent(
						(int)evt.tfinger.fingerId,
						TouchLocationState.Moved,
						evt.tfinger.x,
						evt.tfinger.y,
						evt.tfinger.dx,
						evt.tfinger.dy);

					break;
				// Various Window Events...
				case SDL_EventType.SDL_FINGERUP:
					TouchPanel.INTERNAL_onTouchEvent(
						(int)evt.tfinger.fingerId,
						TouchLocationState.Released,
						evt.tfinger.x,
						evt.tfinger.y,
						0,
						0);

					break;
				// Display Events
				case SDL_EventType.SDL_WINDOWEVENT:
					switch (evt.window.windowEvent)
					{
						// Window Focus
						case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
						{
							game.IsActive = true;

							if (!OSXUseSpaces)
							{
								// If we alt-tab away, we lose the 'fullscreen desktop' flag on some WMs
								SDL_SetWindowFullscreen(
									game.Window.Handle,
									game.GraphicsDevice.PresentationParameters.IsFullScreen ? (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP : 0);
							}

							// Disable the screensaver when we're back.
							SDL_DisableScreenSaver();
							break;
						}

						// Window Resize
						case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
						{
							game.IsActive = false;

							if (!OSXUseSpaces)
							{
								SDL_SetWindowFullscreen(game.Window.Handle, 0);
							}

							// Give the screensaver back, we're not that important now.
							SDL_EnableScreenSaver();
							break;
						}

						case SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
							// This is called on both API and WM resizes
							Mouse._windowWidth = evt.window.data1;
							Mouse._windowHeight = evt.window.data2;
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
						{
							/* This should be called on user resize only, NOT ApplyChanges!
					 * Sadly some window managers are idiots and fire events anyway.
					 * Also ignore any other "resizes" (alt-tab, fullscreen, etc.)
					 * -flibit
					 */
							var flags = SDL_GetWindowFlags(game.Window.Handle);
							if ((flags & (uint)SDL_WindowFlags.SDL_WINDOW_RESIZABLE) != 0 &&
							    (flags & (uint)SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0)
							{
								((FNAWindow)game.Window).INTERNAL_ClientSizeChanged();
							}

							break;
						}

						// Window Move
						case SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED:
							// This is typically called when the window is made bigger
							game.RedrawWindow();
							break;
						// Mouse Focus
						case SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
						{
							/* Apparently if you move the window to a new
					 * display, a GraphicsDevice Reset occurs.
					 * -flibit
					 */
							var newIndex = SDL_GetWindowDisplayIndex(game.Window.Handle);
							if (GraphicsAdapter.Adapters[newIndex] != currentAdapter)
							{
								currentAdapter = GraphicsAdapter.Adapters[newIndex];
								game.GraphicsDevice.Reset(
									game.GraphicsDevice.PresentationParameters,
									currentAdapter);
							}

							break;
						}

						case SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
							SDL_DisableScreenSaver();
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
							SDL_EnableScreenSaver();
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_NONE:
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN:
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_TAKE_FOCUS:
							break;
						case SDL_WindowEventID.SDL_WINDOWEVENT_HIT_TEST:
							break;
					}

					break;
				// Controller device management
				case SDL_EventType.SDL_DISPLAYEVENT:
				{
					GraphicsAdapter.AdaptersChanged();

					var displayIndex = SDL_GetWindowDisplayIndex(game.Window.Handle);
					currentAdapter = GraphicsAdapter.Adapters[displayIndex];

					// Orientation Change
					if (evt.display.displayEvent == SDL_DisplayEventID.SDL_DISPLAYEVENT_ORIENTATION)
					{
						var orientation = INTERNAL_ConvertOrientation((SDL_DisplayOrientation)evt.display.data1);

						INTERNAL_HandleOrientationChange(
							orientation,
							game.GraphicsDevice,
							(FNAWindow)game.Window);
					}

					break;
				}

				case SDL_EventType.SDL_CONTROLLERDEVICEADDED:
					INTERNAL_AddInstance(evt.cdevice.which);
					break;
				// Text Input
				case SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
					INTERNAL_RemoveInstance(evt.cdevice.which);
					break;
				case SDL_EventType.SDL_TEXTINPUT when !textInputSuppress:
					// Based on the SDL2# LPUtf8StrMarshaler
					unsafe
					{
						fixed (byte* p = evt.text.text)
						{
							var bytes = MeasureStringLength(p);
							if (bytes > 0)
							{
								/* UTF8 will never encode more characters
							 * than bytes in a string, so bytes is a
							 * suitable upper estimate of size needed
							 */
								var charsBuffer = stackalloc char[bytes];
								var chars = Encoding.UTF8.GetChars(
									p,
									bytes,
									charsBuffer,
									bytes);

								for (var i = 0; i < chars; i += 1)
								{
									TextInputEXT.OnTextInput(charsBuffer[i]);
								}
							}
						}
					}

					break;
				// Quit
				case SDL_EventType.SDL_TEXTEDITING:
					unsafe
					{
						fixed (byte* p = evt.edit.text)
						{
							var bytes = MeasureStringLength(p);
							if (bytes > 0)
							{
								var charsBuffer = stackalloc char[bytes];
								var chars = Encoding.UTF8.GetChars(
									p,
									bytes,
									charsBuffer,
									bytes);

								string text = new(charsBuffer, 0, chars);
								TextInputEXT.OnTextEditing(text, evt.edit.start, evt.edit.length);
							}
							else
							{
								TextInputEXT.OnTextEditing(string.Empty, 0, 0);
							}
						}
					}

					break;
				case SDL_EventType.SDL_QUIT:
					game.RunApplication = false;
					return true;
			}

			return false;
		}

		private static void INTERNAL_SetIcon(IntPtr window, string title)
		{
			string fileIn;

			/* If the game's using SDL2_image, provide the option to use a PNG
			 * instead of a BMP. Nice for anyone who cares about transparency.
			 * -flibit
			 */
			try
			{
				fileIn = INTERNAL_GetIconName(title + ".png");
				if (!string.IsNullOrEmpty(fileIn))
				{
					IntPtr pixels, icon;
					using (Stream stream = TitleContainer.OpenStream(fileIn))
					{
						pixels = FNA3D.ReadImageStream(
							stream,
							out var w,
							out var h,
							out _);

						icon = SDL_CreateRGBSurfaceFrom(
							pixels,
							w,
							h,
							8 * 4,
							w * 4,
							0x000000FF,
							0x0000FF00,
							0x00FF0000,
							0xFF000000);
					}

					SDL_SetWindowIcon(window, icon);
					SDL_FreeSurface(icon);
					FNA3D.FNA3D_Image_Free(pixels);
					return;
				}
			}
			catch (DllNotFoundException)
			{
				// Not that big a deal guys.
			}

			fileIn = INTERNAL_GetIconName(title + ".bmp");
			if (!string.IsNullOrEmpty(fileIn))
			{
				var icon = SDL_LoadBMP(fileIn);
				SDL_SetWindowIcon(window, icon);
				SDL_FreeSurface(icon);
			}
		}

		private static string INTERNAL_GetIconName(string title)
		{
			string fileIn = Path.Combine(TitleLocation.Path, title);
			if (File.Exists(fileIn))
			{
				// If the title and filename work, it just works. Fine.
				return fileIn;
			}

			// But sometimes the title has invalid characters inside.
			fileIn = Path.Combine(
				TitleLocation.Path,
				INTERNAL_StripBadChars(title));

			if (File.Exists(fileIn))
			{
				return fileIn;
			}

			return string.Empty;
		}

		private static string INTERNAL_StripBadChars(string path)
		{
			/* In addition to the filesystem's invalid charset, we need to
			 * blacklist the Windows standard set too, no matter what.
			 * -flibit
			 */
			char[] hardCodeBadChars = {'<', '>', ':', '"', '/', '\\', '|', '?', '*'};
			List<char> badChars = new();
			badChars.AddRange(Path.GetInvalidFileNameChars());
			badChars.AddRange(hardCodeBadChars);

			string stripChars = path;
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (var c in badChars)
			{
				stripChars = stripChars.Replace(c.ToString(), string.Empty);
			}

			return stripChars;
		}

		private static string? MonoPathRootWorkaround(string? storageRoot)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				// This is what we should be doing everywhere...
				return Path.GetPathRoot(storageRoot);
			}

			// This is stolen from Mono's Path.cs
			if (storageRoot == null)
			{
				return null;
			}

			if (storageRoot.Trim().Length == 0)
			{
				throw new ArgumentException("The specified path is not of a legal form.");
			}

			if (!Path.IsPathRooted(storageRoot))
			{
				return string.Empty;
			}

			/* FIXME: Mono b u g!
			 *
			 * For Unix, the Mono Path.GetPathRoot is pretty lazy:
			 * https://github.com/mono/mono/blob/master/mcs/class/corlib/System.IO/Path.cs#L443
			 * It should actually be checking the drives and
			 * comparing them to the provided path.
			 * If a Mono maintainer is reading this, please steal
			 * this code so we don't have to hack around Mono!
			 *
			 * -flibit
			 */
			int drive = -1, length = 0;
			string[] drives = Environment.GetLogicalDrives();
			for (var i = 0; i < drives.Length; i += 1)
			{
				if (string.IsNullOrEmpty(drives[i]))
				{
					// ... What?
					continue;
				}

				string name = drives[i];
				if (name[^1] != Path.DirectorySeparatorChar)
				{
					name += Path.DirectorySeparatorChar;
				}

				if (storageRoot.StartsWith(name, StringComparison.CurrentCulture) &&
				    name.Length > length)
				{
					drive = i;
					length = name.Length;
				}
			}

			// Uh
			return drive >= 0 ? drives[drive] : Path.GetPathRoot(storageRoot);
		}

		private static unsafe int MeasureStringLength(byte* ptr)
		{
			int bytes;
			for (bytes = 0; *ptr != 0; ptr += 1, bytes += 1)
			{
			}

			return bytes;
		}

		private static DisplayOrientation INTERNAL_ConvertOrientation(SDL_DisplayOrientation orientation)
		{
			switch (orientation)
			{
				case SDL_DisplayOrientation.SDL_ORIENTATION_LANDSCAPE:
					return DisplayOrientation.LandscapeLeft;

				case SDL_DisplayOrientation.SDL_ORIENTATION_LANDSCAPE_FLIPPED:
					return DisplayOrientation.LandscapeRight;

				case SDL_DisplayOrientation.SDL_ORIENTATION_PORTRAIT:
					return DisplayOrientation.Portrait;

				default:
					throw new NotSupportedException("FNA does not support this device orientation.");
			}
		}

		private static void INTERNAL_HandleOrientationChange(
			DisplayOrientation orientation,
			GraphicsDevice graphicsDevice,
			FNAWindow window)
		{
			// Flip the backbuffer dimensions if needed
			var width = graphicsDevice.PresentationParameters.BackBufferWidth;
			var height = graphicsDevice.PresentationParameters.BackBufferHeight;
			var min = Math.Min(width, height);
			var max = Math.Max(width, height);

			if (orientation == DisplayOrientation.Portrait)
			{
				graphicsDevice.PresentationParameters.BackBufferWidth = min;
				graphicsDevice.PresentationParameters.BackBufferHeight = max;
			}
			else
			{
				graphicsDevice.PresentationParameters.BackBufferWidth = max;
				graphicsDevice.PresentationParameters.BackBufferHeight = min;
			}

			// Update the graphics device and window
			graphicsDevice.PresentationParameters.DisplayOrientation = orientation;
			window.CurrentOrientation = orientation;

			graphicsDevice.Reset();
			window.INTERNAL_OnOrientationChanged();
		}

		[DllImport("emscripten", CallingConvention = CallingConvention.Cdecl)]
		[SuppressMessage("StyleCop.Naming", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "emscripten")]
		[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "emscripten")]
		private static extern void emscripten_set_main_loop(
			em_callback_func func,
			int fps,
			int simulate_infinite_loop);

		[DllImport("emscripten", CallingConvention = CallingConvention.Cdecl)]
		[SuppressMessage("StyleCop.Naming", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "emscripten")]
		private static extern void emscripten_cancel_main_loop();

		[MonoPInvokeCallback(typeof(em_callback_func))]
		private static void RunEmscriptenMainLoop()
		{
			_emscriptenGame!.RunOneFrame();

			// FIXME: Is this even needed...?
			if (!_emscriptenGame.RunApplication)
			{
				_emscriptenGame.Exit();
				emscripten_cancel_main_loop();
			}
		}

		private static string GetBaseDirectory()
		{
			if (Environment.GetEnvironmentVariable("FNA_SDL2_FORCE_BASE_PATH") != "1")
			{
				// If your platform uses a CLR, you want to be in this list!
				if (!string.IsNullOrEmpty(_osVersion) &&
				    (_osVersion.Equals("Windows", StringComparison.Ordinal) ||
				     _osVersion.Equals("Mac OS X", StringComparison.Ordinal) ||
				     _osVersion.Equals("Linux", StringComparison.Ordinal) ||
				     _osVersion.Equals("FreeBSD", StringComparison.Ordinal) ||
				     _osVersion.Equals("OpenBSD", StringComparison.Ordinal) ||
				     _osVersion.Equals("NetBSD", StringComparison.Ordinal)))
				{
					return AppDomain.CurrentDomain.BaseDirectory;
				}
			}

			string result = SDL_GetBasePath();
			if (string.IsNullOrEmpty(result))
			{
				result = AppDomain.CurrentDomain.BaseDirectory;
			}

			if (string.IsNullOrEmpty(result))
			{
				/* In the chance that there is no base directory,
				 * return the working directory and hope for the best.
				 *
				 * If we've reached this, the game has either been
				 * started from its directory, or a wrapper has set up
				 * the working directory to the game dir for us.
				 *
				 * Note about Android:
				 *
				 * There is no way from the C# side of things to cleanly
				 * obtain where the game is located without looking at an
				 * instance of System.Diagnostics.StackTrace or without
				 * some interop between the Java and C# side of things.
				 * We're assuming that either the environment itself is
				 * setting one of the possible base paths to point to the
				 * game dir, or that the Java side has called into the C#
				 * side to set Environment.CurrentDirectory.
				 *
				 * In the best case, nothing would be set and the game
				 * wouldn't use the title location in the first place, as
				 * the assets would be read directly from the .apk / .obb
				 * -ade
				 */
				result = Environment.CurrentDirectory;
			}

			return result;
		}

		private static void INTERNAL_AddInstance(int dev)
		{
			var which = -1;
			for (var i = 0; i < _devices.Length; i += 1)
			{
				if (_devices[i] == IntPtr.Zero)
				{
					which = i;
					break;
				}
			}

			if (which == -1)
			{
				return; // Ignoring more than 4 controllers.
			}

			// Clear the error buffer. We're about to do a LOT of dangerous stuff.
			SDL_ClearError();

			// Open the device!
			_devices[which] = SDL_GameControllerOpen(dev);

			// We use this when dealing with GUID initialization.
			var thisJoystick = SDL_GameControllerGetJoystick(_devices[which]);

			// Pair up the instance ID to the player index.
			// FIXME: Remove check after 2.0.4? -flibit
			var thisInstance = SDL_JoystickInstanceID(thisJoystick);
			if (_instanceList.ContainsKey(thisInstance))
			{
				// Duplicate? Usually this is OSX being dumb, but...?
				_devices[which] = IntPtr.Zero;
				return;
			}

			_instanceList.Add(thisInstance, which);

			// Start with a fresh state.
			_states[which] = default;
			_states[which].IsConnected = true;

			// Initialize the haptics for the joystick, if applicable.
			var hasRumble = SDL_GameControllerRumble(
				_devices[which],
				0,
				0,
				0) == 0;

			var hasTriggerRumble = SDL_GameControllerRumbleTriggers(
				_devices[which],
				0,
				0,
				0) == 0;

			// An SDL_GameController _should_ always be complete...
			var caps = default(GamePadCapabilities);
			caps.IsConnected = true;
			caps.GamePadType = _gamepadType[(int)SDL_JoystickGetType(thisJoystick)];
			caps.HasAButton = SDL_GameControllerGetBindForButton(
				                  _devices[which],
				                  SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A).bindType !=
			                  SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasBButton = SDL_GameControllerGetBindForButton(
				                  _devices[which],
				                  SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B).bindType !=
			                  SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasXButton = SDL_GameControllerGetBindForButton(
				                  _devices[which],
				                  SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X).bindType !=
			                  SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasYButton = SDL_GameControllerGetBindForButton(
				                  _devices[which],
				                  SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y).bindType !=
			                  SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasBackButton = SDL_GameControllerGetBindForButton(
				                     _devices[which],
				                     SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK).bindType !=
			                     SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasBigButton = SDL_GameControllerGetBindForButton(
				                    _devices[which],
				                    SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE).bindType !=
			                    SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasStartButton = SDL_GameControllerGetBindForButton(
				                      _devices[which],
				                      SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START).bindType !=
			                      SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasLeftStickButton = SDL_GameControllerGetBindForButton(
				                          _devices[which],
				                          SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK).bindType !=
			                          SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasRightStickButton = SDL_GameControllerGetBindForButton(
				                           _devices[which],
				                           SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK).bindType !=
			                           SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasLeftShoulderButton = SDL_GameControllerGetBindForButton(
				                             _devices[which],
				                             SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER).bindType !=
			                             SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasRightShoulderButton = SDL_GameControllerGetBindForButton(
				                              _devices[which],
				                              SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER).bindType !=
			                              SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasDPadUpButton = SDL_GameControllerGetBindForButton(
				                       _devices[which],
				                       SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP).bindType !=
			                       SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasDPadDownButton = SDL_GameControllerGetBindForButton(
				                         _devices[which],
				                         SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN).bindType !=
			                         SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasDPadLeftButton = SDL_GameControllerGetBindForButton(
				                         _devices[which],
				                         SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT).bindType !=
			                         SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasDPadRightButton = SDL_GameControllerGetBindForButton(
				                          _devices[which],
				                          SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT).bindType !=
			                          SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasLeftXThumbStick = SDL_GameControllerGetBindForAxis(
				                          _devices[which],
				                          SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX).bindType !=
			                          SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasLeftYThumbStick = SDL_GameControllerGetBindForAxis(
				                          _devices[which],
				                          SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY).bindType !=
			                          SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasRightXThumbStick = SDL_GameControllerGetBindForAxis(
				                           _devices[which],
				                           SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX).bindType !=
			                           SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasRightYThumbStick = SDL_GameControllerGetBindForAxis(
				                           _devices[which],
				                           SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY).bindType !=
			                           SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasLeftTrigger = SDL_GameControllerGetBindForAxis(
				                      _devices[which],
				                      SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT).bindType !=
			                      SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasRightTrigger = SDL_GameControllerGetBindForAxis(
					_devices[which],
					SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT)
				.bindType != SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasLeftVibrationMotor = hasRumble;
			caps.HasRightVibrationMotor = hasRumble;
			caps.HasVoiceSupport = false;
			caps.HasLightBarEXT = SDL_GameControllerHasLED(_devices[which]) == SDL_bool.SDL_TRUE;
			caps.HasTriggerVibrationMotorsEXT = hasTriggerRumble;
			caps.HasMisc1EXT = SDL_GameControllerGetBindForButton(_devices[which], SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_MISC1).bindType !=
			                   SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasPaddle1EXT = SDL_GameControllerGetBindForButton(_devices[which], SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_PADDLE1).bindType !=
			                     SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasPaddle2EXT = SDL_GameControllerGetBindForButton(_devices[which], SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_PADDLE2).bindType !=
			                     SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasPaddle3EXT = SDL_GameControllerGetBindForButton(_devices[which], SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_PADDLE3).bindType !=
			                     SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasPaddle4EXT = SDL_GameControllerGetBindForButton(_devices[which], SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_PADDLE4).bindType !=
			                     SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasTouchPadEXT =
				SDL_GameControllerGetBindForButton(_devices[which], SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_TOUCHPAD).bindType !=
				SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;

			caps.HasGyroEXT = SDL_GameControllerHasSensor(_devices[which], SDL_SensorType.SDL_SENSOR_GYRO) == SDL_bool.SDL_TRUE;
			caps.HasAccelerometerEXT = SDL_GameControllerHasSensor(_devices[which], SDL_SensorType.SDL_SENSOR_ACCEL) == SDL_bool.SDL_TRUE;
			_capabilities[which] = caps;

			// ReSharper disable once CommentTypo
			/* Store the GUID string for this device
			 * FIXME: Replace GetGUIDEXT string with 3 short values -flibit
			 */
			var vendor = SDL_JoystickGetVendor(thisJoystick);
			var product = SDL_JoystickGetProduct(thisJoystick);
			if (vendor == 0x00 && product == 0x00)
			{
				_guids[which] = "xinput";
			}
			else
			{
				_guids[which] = $"{vendor & 0xFF:x2}{vendor >> 8:x2}{product & 0xFF:x2}{product >> 8:x2}";
			}

			// Print controller information to stdout.
			FNALoggerEXT.LogInfo!("Controller " + which + ": " + SDL_GameControllerName(_devices[which]));
		}

		private static void INTERNAL_RemoveInstance(int dev)
		{
			if (!_instanceList.TryGetValue(dev, out var output))
			{
				// Odds are, this is controller 5+ getting removed.
				return;
			}

			_instanceList.Remove(dev);
			SDL_GameControllerClose(_devices[output]);
			_devices[output] = IntPtr.Zero;
			_states[output] = default;
			_guids[output] = string.Empty;

			// A lot of errors can happen here, but honestly, they can be ignored...
			SDL_ClearError();

			FNALoggerEXT.LogInfo!("Removed device, player: " + output);
		}

		// GetState can convert stick values to button values
		private static Buttons READ_StickToButtons(Vector2 stick, Buttons left, Buttons right, Buttons up, Buttons down, float deadZoneSize)
		{
			Buttons b = 0;

			if (stick.X > deadZoneSize)
			{
				b |= right;
			}

			if (stick.X < -deadZoneSize)
			{
				b |= left;
			}

			if (stick.Y > deadZoneSize)
			{
				b |= up;
			}

			if (stick.Y < -deadZoneSize)
			{
				b |= down;
			}

			return b;
		}

		private static string[] GenStringArray()
		{
			string[] result = new string[GamePad.GamePadCount];
			for (var i = 0; i < result.Length; i += 1)
			{
				result[i] = string.Empty;
			}

			return result;
		}

		private static Keys ToXNAKey(ref SDL_Keysym key)
		{
			Keys retVal;
			if (UseScancodes)
			{
				if (_scanMap.TryGetValue((int)key.scancode, out retVal))
				{
					return retVal;
				}
			}
			else
			{
				if (_keyMap.TryGetValue((int)key.sym, out retVal))
				{
					return retVal;
				}
			}

			FNALoggerEXT.LogWarn!(
				"KEY/SCANCODE MISSING FROM SDL2->XNA DICTIONARY: " +
				key.sym + " " +
				key.scancode);

			return Keys.None;
		}

		private static unsafe int Win32OnPaint(IntPtr userdata, IntPtr evtPtr)
		{
			var evt = (SDL_Event*)evtPtr;
			if (evt->type == SDL_EventType.SDL_WINDOWEVENT &&
			    evt->window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED)
			{
				foreach (Game game in _activeGames)
				{
					if (game.Window != null &&
					    evt->window.windowID == SDL_GetWindowID(game.Window.Handle))
					{
						game.RedrawWindow();
						return 0;
					}
				}
			}

			if (_prevEventFilter != null)
			{
				return _prevEventFilter(userdata, evtPtr);
			}

			return 1;
		}

		[SuppressMessage("StyleCop.Naming", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "emscripten")]
		[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "emscripten")]
		private delegate void em_callback_func();
	}
}
