// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using SDL2;

namespace Katabasis
{
	internal static class FNAPlatform
	{
		public delegate void ApplyWindowChangesFunc(
			IntPtr window,
			int clientWidth,
			int clientHeight,
			bool wantsFullscreen,
			string screenDeviceName,
			ref string resultDeviceName);

		public delegate GameWindow CreateWindowFunc();

		public delegate void DisposeWindowFunc(GameWindow window);

		public delegate DisplayMode GetCurrentDisplayModeFunc(int adapterIndex);

		public delegate DriveInfo? GetDriveInfoFunc(string storageRoot);

		public delegate bool GetGamePadAccelerometerFunc(int index, out Vector3 accel);

		public delegate GamePadCapabilities GetGamePadCapabilitiesFunc(int index);

		public delegate string GetGamePadGUIDFunc(int index);

		public delegate bool GetGamePadGyroFunc(int index, out Vector3 gyro);

		public delegate GamePadState GetGamePadStateFunc(
			int index,
			GamePadDeadZone deadZoneMode);

		public delegate GraphicsAdapter[] GetGraphicsAdaptersFunc();

		public delegate Keys GetKeyFromScancodeFunc(Keys scancode);

		public delegate int GetMicrophoneQueuedBytesFunc(uint handle);

		public delegate int GetMicrophoneSamplesFunc(
			uint handle,
			byte[] buffer,
			int offset,
			int count);

		public delegate Microphone[] GetMicrophonesFunc();

		public delegate void GetMouseStateFunc(
			IntPtr window,
			out int x,
			out int y,
			out ButtonState left,
			out ButtonState middle,
			out ButtonState right,
			out ButtonState x1,
			out ButtonState x2);

		public delegate int GetNumTouchFingersFunc();

		public delegate bool GetRelativeMouseModeFunc();

		public delegate string GetStorageRootFunc();

		public delegate TouchPanelCapabilities GetTouchCapabilitiesFunc();

		public delegate bool GetWindowBorderlessFunc(IntPtr window);

		public delegate Rectangle GetWindowBoundsFunc(IntPtr window);

		public delegate bool GetWindowResizableFunc(IntPtr window);

		public delegate bool NeedsPlatformMainLoopFunc();

		public delegate void OnIsMouseVisibleChangedFunc(bool visible);

		public delegate void PollEventsFunc(
			Game game,
			ref GraphicsAdapter currentAdapter,
			bool[] textInputControlDown,
			int[] textInputControlRepeat,
			ref bool textInputSuppress);

		public delegate GraphicsAdapter RegisterGameFunc(Game game);

		public delegate void RunPlatformMainLoopFunc(Game game);

		public delegate void SetGamePadLightBarFunc(int index, Color color);

		public delegate bool SetGamePadTriggerVibrationFunc(
			int index,
			float leftTrigger,
			float rightTrigger);

		public delegate bool SetGamePadVibrationFunc(
			int index,
			float leftMotor,
			float rightMotor);

		public delegate void SetMousePositionFunc(
			IntPtr window,
			int x,
			int y);

		public delegate void SetRelativeMouseModeFunc(bool enable);

		public delegate void SetTextInputRectangleFunc(Rectangle rectangle);

		public delegate void SetWindowBorderlessFunc(IntPtr window, bool borderless);

		public delegate void SetWindowResizableFunc(IntPtr window, bool resizable);

		public delegate void SetWindowTitleFunc(IntPtr window, string title);

		public delegate void ShowRuntimeErrorFunc(string title, string message);

		public delegate void StartMicrophoneFunc(uint handle);

		public delegate void StartTextInputFunc();

		public delegate void StopMicrophoneFunc(uint handle);

		public delegate void StopTextInputFunc();

		public delegate bool SupportsOrientationChangesFunc();

		public delegate void UnregisterGameFunc(Game game);

		public delegate void UpdateTouchPanelStateFunc();

		public static readonly string TitleLocation;

		/* Setup Text Input Control Character Arrays
		 * (Only 7 control keys supported at this time)
		 */
		public static readonly char[] TextInputCharacters =
		{
			(char)2, // Home
			(char)3, // End
			(char)8, // Backspace
			(char)9, // Tab
			(char)13, // Enter
			(char)127, // Delete
			(char)22 // Ctrl+V (Paste)
		};

		public static readonly Dictionary<Keys, int> TextInputBindings = new()
		{
			{Keys.Home, 0},
			{Keys.End, 1},
			{Keys.Back, 2},
			{Keys.Tab, 3},
			{Keys.Enter, 4},
			{Keys.Delete, 5}
			// Ctrl+V is special!
		};

		public static readonly CreateWindowFunc CreateWindow;

		public static readonly DisposeWindowFunc DisposeWindow;

		public static readonly ApplyWindowChangesFunc ApplyWindowChanges;

		public static readonly GetWindowBoundsFunc GetWindowBounds;

		public static readonly GetWindowResizableFunc GetWindowResizable;

		public static readonly SetWindowResizableFunc SetWindowResizable;

		public static readonly GetWindowBorderlessFunc GetWindowBorderless;

		public static readonly SetWindowBorderlessFunc SetWindowBorderless;

		public static readonly SetWindowTitleFunc SetWindowTitle;

		public static readonly RegisterGameFunc RegisterGame;

		public static readonly UnregisterGameFunc UnregisterGame;

		public static readonly PollEventsFunc PollEvents;

		public static readonly GetGraphicsAdaptersFunc GetGraphicsAdapters;

		public static readonly GetCurrentDisplayModeFunc GetCurrentDisplayMode;

		public static readonly GetKeyFromScancodeFunc GetKeyFromScancode;

		public static readonly StartTextInputFunc StartTextInput;

		public static readonly StopTextInputFunc StopTextInput;

		public static readonly SetTextInputRectangleFunc SetTextInputRectangle;

		public static readonly GetMouseStateFunc GetMouseState;

		public static readonly SetMousePositionFunc SetMousePosition;

		public static readonly OnIsMouseVisibleChangedFunc OnIsMouseVisibleChanged;

		public static readonly GetRelativeMouseModeFunc GetRelativeMouseMode;

		public static readonly SetRelativeMouseModeFunc SetRelativeMouseMode;

		public static readonly GetGamePadCapabilitiesFunc GetGamePadCapabilities;

		public static readonly GetGamePadStateFunc GetGamePadState;

		public static readonly SetGamePadVibrationFunc SetGamePadVibration;

		public static readonly SetGamePadTriggerVibrationFunc SetGamePadTriggerVibration;

		public static readonly GetGamePadGUIDFunc GetGamePadGUID;

		public static readonly SetGamePadLightBarFunc SetGamePadLightBar;

		public static readonly GetGamePadGyroFunc GetGamePadGyro;

		public static readonly GetGamePadAccelerometerFunc GetGamePadAccelerometer;

		public static readonly GetStorageRootFunc GetStorageRoot;

		public static readonly GetDriveInfoFunc GetDriveInfo;

		public static readonly ShowRuntimeErrorFunc ShowRuntimeError;

		public static readonly GetMicrophonesFunc GetMicrophones;

		public static readonly GetMicrophoneSamplesFunc GetMicrophoneSamples;

		public static readonly GetMicrophoneQueuedBytesFunc GetMicrophoneQueuedBytes;

		public static readonly StartMicrophoneFunc StartMicrophone;

		public static readonly StopMicrophoneFunc StopMicrophone;

		public static readonly GetTouchCapabilitiesFunc GetTouchCapabilities;

		public static readonly UpdateTouchPanelStateFunc UpdateTouchPanelState;

		public static readonly GetNumTouchFingersFunc GetNumTouchFingers;

		public static readonly SupportsOrientationChangesFunc SupportsOrientationChanges;

		public static readonly NeedsPlatformMainLoopFunc NeedsPlatformMainLoop;

		public static readonly RunPlatformMainLoopFunc RunPlatformMainLoop;

		[SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Custom parameters.")]
		static FNAPlatform()
		{
			/* I suspect you may have an urge to put an #if in here for new
			 * FNAPlatform implementations.
			 *
			 * DON'T.
			 *
			 * Determine this at runtime, or load dynamically.
			 * No amount of whining will get me to budge on this.
			 * -flibit
			 */

			// Environment.GetEnvironmentVariable("FNA_PLATFORM_BACKEND");

			// Built-in command line arguments
			LaunchParameters args = new();
			if (args.TryGetValue("enablehighdpi", out var arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable(
					"FNA_GRAPHICS_ENABLE_HIGHDPI",
					"1");
			}

			if (args.TryGetValue("gldevice", out arg))
			{
				Environment.SetEnvironmentVariable(
					"FNA3D_FORCE_DRIVER",
					arg);
			}

			if (args.TryGetValue("disablelateswaptear", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable(
					"FNA3D_DISABLE_LATESWAPTEAR",
					"1");
			}

			if (args.TryGetValue("mojoshaderprofile", out arg))
			{
				Environment.SetEnvironmentVariable(
					"FNA3D_MOJOSHADER_PROFILE",
					arg);
			}

			if (args.TryGetValue("backbufferscalenearest", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable(
					"FNA3D_BACKBUFFER_SCALE_NEAREST",
					"1");
			}

			if (args.TryGetValue("usescancodes", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable(
					"FNA_KEYBOARD_USE_SCANCODES",
					"1");
			}

			CreateWindow = SDL2_FNAPlatform.CreateWindow;
			DisposeWindow = SDL2_FNAPlatform.DisposeWindow;
			ApplyWindowChanges = SDL2_FNAPlatform.ApplyWindowChanges;
			GetWindowBounds = SDL2_FNAPlatform.GetWindowBounds;
			GetWindowResizable = SDL2_FNAPlatform.GetWindowResizable;
			SetWindowResizable = SDL2_FNAPlatform.SetWindowResizable;
			GetWindowBorderless = SDL2_FNAPlatform.GetWindowBorderless;
			SetWindowBorderless = SDL2_FNAPlatform.SetWindowBorderless;
			SetWindowTitle = SDL2_FNAPlatform.SetWindowTitle;
			RegisterGame = SDL2_FNAPlatform.RegisterGame;
			UnregisterGame = SDL2_FNAPlatform.UnregisterGame;
			PollEvents = SDL2_FNAPlatform.PollEvents;
			GetGraphicsAdapters = SDL2_FNAPlatform.GetGraphicsAdapters;
			GetCurrentDisplayMode = SDL2_FNAPlatform.GetCurrentDisplayMode;
			GetKeyFromScancode = SDL2_FNAPlatform.GetKeyFromScancode;
			StartTextInput = SDL.SDL_StartTextInput;
			StopTextInput = SDL.SDL_StopTextInput;
			SetTextInputRectangle = SDL2_FNAPlatform.SetTextInputRectangle;
			GetMouseState = SDL2_FNAPlatform.GetMouseState;
			SetMousePosition = SDL.SDL_WarpMouseInWindow;
			OnIsMouseVisibleChanged = SDL2_FNAPlatform.OnIsMouseVisibleChanged;
			GetRelativeMouseMode = SDL2_FNAPlatform.GetRelativeMouseMode;
			SetRelativeMouseMode = SDL2_FNAPlatform.SetRelativeMouseMode;
			GetGamePadCapabilities = SDL2_FNAPlatform.GetGamePadCapabilities;
			GetGamePadState = SDL2_FNAPlatform.GetGamePadState;
			SetGamePadVibration = SDL2_FNAPlatform.SetGamePadVibration;
			SetGamePadTriggerVibration = SDL2_FNAPlatform.SetGamePadTriggerVibration;
			GetGamePadGUID = SDL2_FNAPlatform.GetGamePadGUID;
			SetGamePadLightBar = SDL2_FNAPlatform.SetGamePadLightBar;
			GetGamePadGyro = SDL2_FNAPlatform.GetGamePadGyro;
			GetGamePadAccelerometer = SDL2_FNAPlatform.GetGamePadAccelerometer;
			GetStorageRoot = SDL2_FNAPlatform.GetStorageRoot;
			GetDriveInfo = SDL2_FNAPlatform.GetDriveInfo;
			ShowRuntimeError = SDL2_FNAPlatform.ShowRuntimeError;
			GetMicrophones = SDL2_FNAPlatform.GetMicrophones;
			GetMicrophoneSamples = SDL2_FNAPlatform.GetMicrophoneSamples;
			GetMicrophoneQueuedBytes = SDL2_FNAPlatform.GetMicrophoneQueuedBytes;
			StartMicrophone = SDL2_FNAPlatform.StartMicrophone;
			StopMicrophone = SDL2_FNAPlatform.StopMicrophone;
			GetTouchCapabilities = SDL2_FNAPlatform.GetTouchCapabilities;
			UpdateTouchPanelState = SDL2_FNAPlatform.UpdateTouchPanelState;
			GetNumTouchFingers = SDL2_FNAPlatform.GetNumTouchFingers;
			SupportsOrientationChanges = SDL2_FNAPlatform.SupportsOrientationChanges;
			NeedsPlatformMainLoop = SDL2_FNAPlatform.NeedsPlatformMainLoop;
			RunPlatformMainLoop = SDL2_FNAPlatform.RunPlatformMainLoop;

			FNALoggerEXT.Initialize();

			AppDomain.CurrentDomain.ProcessExit += SDL2_FNAPlatform.ProgramExit;
			TitleLocation = SDL2_FNAPlatform.ProgramInit(args);
		}
	}
}
