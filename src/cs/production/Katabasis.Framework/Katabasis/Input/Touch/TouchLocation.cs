// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Katabasis
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchlocation.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public readonly struct TouchLocation : IEquatable<TouchLocation>
	{
		private readonly Vector2 _prevPosition;
		private readonly TouchLocationState _prevState;

		public int Id { get; }

		public Vector2 Position { get; }

		public TouchLocationState State { get; }

		public TouchLocation(
			int id,
			TouchLocationState state,
			Vector2 position)
			: this()
		{
			Id = id;
			State = state;
			Position = position;
			_prevState = TouchLocationState.Invalid;
			_prevPosition = Vector2.Zero;
		}

		public TouchLocation(
			int id,
			TouchLocationState state,
			Vector2 position,
			TouchLocationState previousState,
			Vector2 previousPosition)
			: this()
		{
			Id = id;
			State = state;
			Position = position;
			_prevState = previousState;
			_prevPosition = previousPosition;
		}

		public bool Equals(TouchLocation other) =>
			Id == other.Id &&
			Position == other.Position &&
			State == other.State &&
			_prevPosition == other._prevPosition &&
			_prevState == other._prevState;

		public override bool Equals(object? obj) => obj is TouchLocation location && Equals(location);

		public override int GetHashCode() => Id.GetHashCode() + Position.GetHashCode();

		public override string ToString() => "{Position:" + Position + "}";

		public bool TryGetPreviousLocation(out TouchLocation previousLocation)
		{
			previousLocation = new TouchLocation(
				Id,
				_prevState,
				_prevPosition);

			return previousLocation.State != TouchLocationState.Invalid;
		}

		public static bool operator ==(
			TouchLocation value1,
			TouchLocation value2) =>
			value1.Equals(value2);

		public static bool operator !=(
			TouchLocation value1,
			TouchLocation value2) =>
			!value1.Equals(value2);
	}
}
