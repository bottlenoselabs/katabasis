// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.ComponentModel;

namespace bottlenoselabs.Katabasis
{
	/* This class exists because the XNA window handle is an abstract class.
	 *
	 * The idea is that each platform would host its own GameWindow type,
	 * but a lot of this can be handled with static functions and a native
	 * window pointer, rather than messy things like a C# Form.
	 *
	 * So, when implementing new FNAPlatforms, just use this to store your
	 * window pointer and the functions will allow you to do your work.
	 *
	 * -flibit
	 */
	internal class FNAWindow : GameWindow
	{
		private readonly IntPtr _window;
		private string _deviceName;
		private bool _wantsFullscreen;

		internal FNAWindow(IntPtr nativeWindow, string display)
		{
			_window = nativeWindow;
			_deviceName = display;
			_wantsFullscreen = false;
		}

		[DefaultValue(false)]
		public override bool AllowUserResizing
		{
			get => FNAPlatform.GetWindowResizable(_window);
			set => FNAPlatform.SetWindowResizable(_window, value);
		}

		public override Rectangle ClientBounds => FNAPlatform.GetWindowBounds(_window);

		public override DisplayOrientation CurrentOrientation { get; internal set; }

		public override IntPtr Handle => _window;

		public override bool IsBorderlessEXT
		{
			get => FNAPlatform.GetWindowBorderless(_window);
			set => FNAPlatform.SetWindowBorderless(_window, value);
		}

		public override string ScreenDeviceName => _deviceName;

		public override void BeginScreenDeviceChange(bool willBeFullScreen) => _wantsFullscreen = willBeFullScreen;

		public override void EndScreenDeviceChange(
			string screenDeviceName,
			int clientWidth,
			int clientHeight)
		{
			string prevName = _deviceName;
			FNAPlatform.ApplyWindowChanges(
				_window,
				clientWidth,
				clientHeight,
				_wantsFullscreen,
				screenDeviceName,
				ref _deviceName);

			if (_deviceName != prevName)
			{
				OnScreenDeviceNameChanged();
			}
		}

		internal void INTERNAL_ClientSizeChanged() => OnClientSizeChanged();

		// internal void INTERNAL_ScreenDeviceNameChanged()
		// {
		//     OnScreenDeviceNameChanged();
		// }

		internal void INTERNAL_OnOrientationChanged() => OnOrientationChanged();

		protected internal override void SetSupportedOrientations(DisplayOrientation orientations) =>
			/* XNA on Windows Phone had the ability to change
		     * the list of supported device orientations at runtime.
		     * Unfortunately, we can't support that reliably across
		     * multiple mobile platforms. Therefore this method is
		     * essentially a no-op.
		     *
		     * Instead, you should set your supported orientations
		     * in Info.plist (iOS) or AndroidManifest.xml (Android).
		     *
		     * -caleb
		     */
			FNALoggerEXT.LogWarn!("Setting SupportedOrientations has no effect!");

		protected override void SetTitle(string title) => FNAPlatform.SetWindowTitle(_window, title);
	}
}
