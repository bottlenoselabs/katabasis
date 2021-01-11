// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public struct GamePadButtons
	{
		internal Buttons _buttons;

		public ButtonState A =>
			(_buttons & Buttons.A) == Buttons.A ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState B =>
			(_buttons & Buttons.B) == Buttons.B ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState Back =>
			(_buttons & Buttons.Back) == Buttons.Back ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState X =>
			(_buttons & Buttons.X) == Buttons.X ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState Y =>
			(_buttons & Buttons.Y) == Buttons.Y ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState Start =>
			(_buttons & Buttons.Start) == Buttons.Start ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState LeftShoulder =>
			(_buttons & Buttons.LeftShoulder) == Buttons.LeftShoulder ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState LeftStick =>
			(_buttons & Buttons.LeftStick) == Buttons.LeftStick ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState RightShoulder =>
			(_buttons & Buttons.RightShoulder) == Buttons.RightShoulder ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState RightStick =>
			(_buttons & Buttons.RightStick) == Buttons.RightStick ? ButtonState.Pressed : ButtonState.Released;

		public ButtonState BigButton =>
			(_buttons & Buttons.BigButton) == Buttons.BigButton ? ButtonState.Pressed : ButtonState.Released;

		public GamePadButtons(Buttons buttons) => _buttons = buttons;

		/* Used by GamePadState public constructor, DO NOT USE! */
		internal static GamePadButtons FromButtonArray(params Buttons[] buttons)
		{
			Buttons mask = 0;
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var b in buttons)
			{
				mask |= b;
			}

			return new GamePadButtons(mask);
		}

		public static bool operator ==(GamePadButtons left, GamePadButtons right) => left._buttons == right._buttons;

		public static bool operator !=(GamePadButtons left, GamePadButtons right) => !(left == right);

		public override bool Equals(object? obj) => obj is GamePadButtons buttons && this == buttons;

		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
		public override int GetHashCode() => (int)_buttons;
	}
}
