// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[PublicAPI]
	public static class GamePad
	{
		/* Based on the XInput constants */
		internal const float LeftDeadZone = 7849.0f / 32768.0f;
		internal const float RightDeadZone = 8689.0f / 32768.0f;
		internal const float TriggerThreshold = 30.0f / 255.0f;

		/* Determines how many controllers we should be tracking.
		 * Per XNA4 we track 4 by default, but if you want to track more you can
		 * do this by changing PlayerIndex.cs to include more index names.
		 * -flibit
		 */
		internal static readonly int GamePadCount = DetermineNumGamePads();

		internal static float ExcludeAxisDeadZone(float value, float deadZone)
		{
			if (value < -deadZone)
			{
				value += deadZone;
			}
			else if (value > deadZone)
			{
				value -= deadZone;
			}
			else
			{
				return 0.0f;
			}

			return value / (1.0f - deadZone);
		}

		public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex) => FNAPlatform.GetGamePadCapabilities((int)playerIndex);

		public static GamePadState GetState(PlayerIndex playerIndex) =>
			FNAPlatform.GetGamePadState(
				(int)playerIndex,
				GamePadDeadZone.IndependentAxes);

		public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode) =>
			FNAPlatform.GetGamePadState(
				(int)playerIndex,
				deadZoneMode);

		public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor) =>
			FNAPlatform.SetGamePadVibration(
				(int)playerIndex,
				leftMotor,
				rightMotor);

		// ReSharper disable once IdentifierTypo
		[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Abbreviations.")]
		public static string GetGUIDEXT(PlayerIndex playerIndex) => FNAPlatform.GetGamePadGUID((int)playerIndex);

		public static void SetLightBarEXT(PlayerIndex playerIndex, Color color) => FNAPlatform.SetGamePadLightBar((int)playerIndex, color);

		public static bool SetTriggerVibrationEXT(PlayerIndex playerIndex, float leftTrigger, float rightTrigger) =>
			FNAPlatform.SetGamePadTriggerVibration(
				(int)playerIndex,
				leftTrigger,
				rightTrigger);

		public static bool GetGyroEXT(PlayerIndex playerIndex, out Vector3 gyro) =>
			FNAPlatform.GetGamePadGyro(
				(int)playerIndex,
				out gyro);

		public static bool GetAccelerometerEXT(PlayerIndex playerIndex, out Vector3 accel) =>
			FNAPlatform.GetGamePadAccelerometer(
				(int)playerIndex,
				out accel);

		private static int DetermineNumGamePads()
		{
			var numGamePadString = Environment.GetEnvironmentVariable("FNA_GAMEPAD_NUM_GAMEPADS");
			if (!string.IsNullOrEmpty(numGamePadString))
			{
				if (int.TryParse(numGamePadString, out var numGamePads))
				{
					if (numGamePads >= 0)
					{
						return numGamePads;
					}
				}
			}

			return Enum.GetNames(typeof(PlayerIndex)).Length;
		}
	}
}
