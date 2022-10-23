// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[Serializable]
	[TypeConverter(typeof(RectangleConverter))]
	[DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
	[PublicAPI]
	public struct Rectangle : IEquatable<Rectangle>
	{
		public int Left => X;

		public int Right => X + Width;

		public int Top => Y;

		public int Bottom => Y + Height;

		public bool IsEmpty =>
			Width == 0 &&
			Height == 0 &&
			X == 0 &&
			Y == 0;

		public static Rectangle Empty { get; }

		internal string DebugDisplayString =>
			string.Concat(
				X.ToString(CultureInfo.InvariantCulture),
				" ",
				Y.ToString(CultureInfo.InvariantCulture),
				" ",
				Width.ToString(CultureInfo.InvariantCulture),
				" ",
				Height.ToString(CultureInfo.InvariantCulture));

		public int X;

		public int Y;

		public int Width;

		public int Height;

		public Rectangle(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public bool Contains(int x, int y) =>
			X <= x &&
			x < X + Width &&
			Y <= y &&
			y < Y + Height;

		public bool Contains(Rectangle value) =>
			X <= value.X &&
			value.X + value.Width <= X + Width &&
			Y <= value.Y &&
			value.Y + value.Height <= Y + Height;

		public void Contains(ref Rectangle value, out bool result) =>
			result = X <= value.X &&
			         value.X + value.Width <= X + Width &&
			         Y <= value.Y &&
			         value.Y + value.Height <= Y + Height;

		public void Offset(int offsetX, int offsetY)
		{
			X += offsetX;
			Y += offsetY;
		}

		public void Inflate(int horizontalValue, int verticalValue)
		{
			X -= horizontalValue;
			Y -= verticalValue;
			Width += horizontalValue * 2;
			Height += verticalValue * 2;
		}

		public bool Equals(Rectangle other) => this == other;

		public override bool Equals(object? obj) => obj is Rectangle rectangle && this == rectangle;

		public override string ToString() =>
			"{X:" + X +
			" Y:" + Y +
			" Width:" + Width +
			" Height:" + Height +
			"}";

		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
		public override int GetHashCode() => X ^ Y ^ Width ^ Height;

		public bool Intersects(Rectangle value) =>
			value.Left < Right &&
			Left < value.Right &&
			value.Top < Bottom &&
			Top < value.Bottom;

		public void Intersects(ref Rectangle value, out bool result) =>
			result = value.Left < Right &&
			         Left < value.Right &&
			         value.Top < Bottom &&
			         Top < value.Bottom;

		public static bool operator ==(Rectangle a, Rectangle b) =>
			a.X == b.X &&
			a.Y == b.Y &&
			a.Width == b.Width &&
			a.Height == b.Height;

		public static bool operator !=(Rectangle a, Rectangle b) => !(a == b);

		public static Rectangle Intersect(Rectangle value1, Rectangle value2)
		{
			Intersect(ref value1, ref value2, out var result);
			return result;
		}

		public static void Intersect(
			ref Rectangle value1,
			ref Rectangle value2,
			out Rectangle result)
		{
			if (value1.Intersects(value2))
			{
				var rightSide = Math.Min(
					value1.X + value1.Width,
					value2.X + value2.Width);

				var leftSide = Math.Max(value1.X, value2.X);
				var topSide = Math.Max(value1.Y, value2.Y);
				var bottomSide = Math.Min(
					value1.Y + value1.Height,
					value2.Y + value2.Height);

				result = new Rectangle(
					leftSide,
					topSide,
					rightSide - leftSide,
					bottomSide - topSide);
			}
			else
			{
				result = new Rectangle(0, 0, 0, 0);
			}
		}

		public static Rectangle Union(Rectangle value1, Rectangle value2)
		{
			var x = Math.Min(value1.X, value2.X);
			var y = Math.Min(value1.Y, value2.Y);
			return new Rectangle(
				x,
				y,
				Math.Max(value1.Right, value2.Right) - x,
				Math.Max(value1.Bottom, value2.Bottom) - y);
		}

		public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
		{
			var x = Math.Min(value1.X, value2.X);
			var y = Math.Min(value1.Y, value2.Y);
			var width = Math.Max(value1.Right, value2.Right) - x;
			var height = Math.Max(value1.Bottom, value2.Bottom) - y;

			result = new Rectangle(x, y, width, height);
		}
	}
}
