// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
namespace Katabasis
{
	public struct GamePadCapabilities
	{
		public bool IsConnected { get; internal set; }

		public bool HasAButton { get; internal set; }

		public bool HasBackButton { get; internal set; }

		public bool HasBButton { get; internal set; }

		public bool HasDPadDownButton { get; internal set; }

		public bool HasDPadLeftButton { get; internal set; }

		public bool HasDPadRightButton { get; internal set; }

		public bool HasDPadUpButton { get; internal set; }

		public bool HasLeftShoulderButton { get; internal set; }

		public bool HasLeftStickButton { get; internal set; }

		public bool HasRightShoulderButton { get; internal set; }

		public bool HasRightStickButton { get; internal set; }

		public bool HasStartButton { get; internal set; }

		public bool HasXButton { get; internal set; }

		public bool HasYButton { get; internal set; }

		public bool HasBigButton { get; internal set; }

		public bool HasLeftXThumbStick { get; internal set; }

		public bool HasLeftYThumbStick { get; internal set; }

		public bool HasRightXThumbStick { get; internal set; }

		public bool HasRightYThumbStick { get; internal set; }

		public bool HasLeftTrigger { get; internal set; }

		public bool HasRightTrigger { get; internal set; }

		public bool HasLeftVibrationMotor { get; internal set; }

		public bool HasRightVibrationMotor { get; internal set; }

		public bool HasVoiceSupport { get; internal set; }

		public GamePadType GamePadType { get; internal set; }

		public bool HasLightBarEXT { get; internal set; }

		public bool HasTriggerVibrationMotorsEXT { get; internal set; }

		public bool HasMisc1EXT { get; internal set; }

		public bool HasPaddle1EXT { get; internal set; }

		public bool HasPaddle2EXT { get; internal set; }

		public bool HasPaddle3EXT { get; internal set; }

		public bool HasPaddle4EXT { get; internal set; }

		public bool HasTouchPadEXT { get; internal set; }

		public bool HasGyroEXT { get; internal set; }

		public bool HasAccelerometerEXT { get; internal set; }
	}
}
