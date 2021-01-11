// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

namespace Katabasis
{
	public struct MouseState
	{
		public int X { get; internal set; }

		public int Y { get; internal set; }

		public ButtonState LeftButton { get; internal set; }

		public ButtonState RightButton { get; internal set; }

		public ButtonState MiddleButton { get; internal set; }

		public ButtonState XButton1 { get; internal set; }

		public ButtonState XButton2 { get; internal set; }

		public int ScrollWheelValue { get; internal set; }

		public MouseState(
			int x,
			int y,
			int scrollWheel,
			ButtonState leftButton,
			ButtonState middleButton,
			ButtonState rightButton,
			ButtonState xButton1,
			ButtonState xButton2)
			: this()
		{
			X = x;
			Y = y;
			ScrollWheelValue = scrollWheel;
			LeftButton = leftButton;
			MiddleButton = middleButton;
			RightButton = rightButton;
			XButton1 = xButton1;
			XButton2 = xButton2;
		}

		public static bool operator ==(MouseState left, MouseState right) =>
			left.X == right.X &&
			left.Y == right.Y &&
			left.LeftButton == right.LeftButton &&
			left.MiddleButton == right.MiddleButton &&
			left.RightButton == right.RightButton &&
			left.ScrollWheelValue == right.ScrollWheelValue;

		public static bool operator !=(MouseState left, MouseState right) => !(left == right);

		public override bool Equals(object? obj) => obj is MouseState state && this == state;

		public override int GetHashCode() => base.GetHashCode();

		public override string ToString()
		{
			string buttons = string.Empty;
			if (LeftButton == ButtonState.Pressed)
			{
				buttons = "Left";
			}

			if (RightButton == ButtonState.Pressed)
			{
				if (buttons.Length > 0)
				{
					buttons += " ";
				}

				buttons += "Right";
			}

			if (MiddleButton == ButtonState.Pressed)
			{
				if (buttons.Length > 0)
				{
					buttons += " ";
				}

				buttons += "Middle";
			}

			if (XButton1 == ButtonState.Pressed)
			{
				if (buttons.Length > 0)
				{
					buttons += " ";
				}

				buttons += "XButton1";
			}

			if (XButton2 == ButtonState.Pressed)
			{
				if (buttons.Length > 0)
				{
					buttons += " ";
				}

				buttons += "XButton2";
			}

			if (string.IsNullOrEmpty(buttons))
			{
				buttons = "None";
			}

			return $"[MouseState X={X}, Y={Y}, Buttons={buttons}, Wheel={ScrollWheelValue}]";
		}
	}
}
