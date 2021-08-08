// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	public struct GamePadDPad
	{
		public ButtonState Down { get; internal set; }

		public ButtonState Left { get; internal set; }

		public ButtonState Right { get; internal set; }

		public ButtonState Up { get; internal set; }

		public GamePadDPad(
			ButtonState upValue,
			ButtonState downValue,
			ButtonState leftValue,
			ButtonState rightValue)
			: this()
		{
			Up = upValue;
			Down = downValue;
			Left = leftValue;
			Right = rightValue;
		}

		/* Used by GamePadState public constructor, DO NOT USE !*/
		internal static GamePadDPad FromButtonArray(params Buttons[] buttons)
		{
			var up = ButtonState.Released;
			var down = ButtonState.Released;
			var left = ButtonState.Released;
			var right = ButtonState.Released;
			foreach (var b in buttons)
			{
				if ((b & Buttons.DPadUp) == Buttons.DPadUp)
				{
					up = ButtonState.Pressed;
				}

				if ((b & Buttons.DPadDown) == Buttons.DPadDown)
				{
					down = ButtonState.Pressed;
				}

				if ((b & Buttons.DPadLeft) == Buttons.DPadLeft)
				{
					left = ButtonState.Pressed;
				}

				if ((b & Buttons.DPadRight) == Buttons.DPadRight)
				{
					right = ButtonState.Pressed;
				}
			}

			return new GamePadDPad(up, down, left, right);
		}

		public static bool operator ==(GamePadDPad left, GamePadDPad right) =>
			left.Down == right.Down &&
			left.Left == right.Left &&
			left.Right == right.Right &&
			left.Up == right.Up;

		public static bool operator !=(GamePadDPad left, GamePadDPad right) => !(left == right);

		public override bool Equals(object? obj) => obj is GamePadDPad pad && this == pad;

		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
		public override int GetHashCode() =>
			(Down == ButtonState.Pressed ? 1 : 0) +
			(Left == ButtonState.Pressed ? 2 : 0) +
			(Right == ButtonState.Pressed ? 4 : 0) +
			(Up == ButtonState.Pressed ? 8 : 0);
	}
}
