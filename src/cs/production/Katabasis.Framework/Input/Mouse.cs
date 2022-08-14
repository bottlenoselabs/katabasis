// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "CA2211", Justification = "Hooks.")]
	public static class Mouse
	{
		public static Action<int>? ClickedEXT;

		internal static int _windowWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
		internal static int _windowHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
		internal static int _backBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
		internal static int _backBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;

		internal static int _mouseWheel;

		public static IntPtr WindowHandle { get; set; }

		public static bool IsRelativeMouseModeEXT
		{
			get => FNAPlatform.GetRelativeMouseMode();
			set => FNAPlatform.SetRelativeMouseMode(value);
		}

		internal static void INTERNAL_onClicked(int button) => ClickedEXT?.Invoke(button);

		public static MouseState GetState()
		{
			FNAPlatform.GetMouseState(
				WindowHandle,
				out var x,
				out var y,
				out var left,
				out var middle,
				out var right,
				out var x1,
				out var x2);

			// Scale the mouse coordinates for the faux-backbuffer
			x = (int)((double)x * _backBufferWidth / _windowWidth);
			y = (int)((double)y * _backBufferHeight / _windowHeight);

			return new MouseState(
				x,
				y,
				_mouseWheel,
				left,
				middle,
				right,
				x1,
				x2);
		}

		public static void SetPosition(int x, int y)
		{
			// In relative mode, this function is meaningless
			if (IsRelativeMouseModeEXT)
			{
				return;
			}

			// Scale the mouse coordinates for the faux-backbuffer
			x = (int)((double)x * _windowWidth / _backBufferWidth);
			y = (int)((double)y * _windowHeight / _backBufferHeight);

			FNAPlatform.SetMousePosition(WindowHandle, x, y);
		}
	}
}
