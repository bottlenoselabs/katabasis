// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Katabasis
{
    [Serializable]
    [TypeConverter(typeof(RectangleConverter))]
    [DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Not used?")]
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

        public static Rectangle Empty { get; } = default;

        internal string DebugDisplayString =>
            string.Concat(
                X.ToString(),
                " ",
                Y.ToString(),
                " ",
                Width.ToString(),
                " ",
                Height.ToString());

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

        public bool Contains(int x, int y)
        {
            return X <= x &&
                   x < X + Width &&
                   Y <= y &&
                   y < Y + Height;
        }

        public bool Contains(Rectangle value)
        {
            return X <= value.X &&
                   value.X + value.Width <= X + Width &&
                   Y <= value.Y &&
                   value.Y + value.Height <= Y + Height;
        }

        public void Contains(ref Rectangle value, out bool result)
        {
            result = X <= value.X &&
                     value.X + value.Width <= X + Width &&
                     Y <= value.Y &&
                     value.Y + value.Height <= Y + Height;
        }

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

        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        public override bool Equals(object? obj)
        {
            return obj is Rectangle rectangle && this == rectangle;
        }

        public override string ToString()
        {
            return "{X:" + X +
                   " Y:" + Y +
                   " Width:" + Width +
                   " Height:" + Height +
                   "}";
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
        public override int GetHashCode()
        {
            return X ^ Y ^ Width ^ Height;
        }

        public bool Intersects(Rectangle value)
        {
            return value.Left < Right &&
                   Left < value.Right &&
                   value.Top < Bottom &&
                   Top < value.Bottom;
        }

        public void Intersects(ref Rectangle value, out bool result)
        {
            result = value.Left < Right &&
                     Left < value.Right &&
                     value.Top < Bottom &&
                     Top < value.Bottom;
        }

        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.X == b.X &&
                   a.Y == b.Y &&
                   a.Width == b.Width &&
                   a.Height == b.Height;
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }

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
                var right_side = Math.Min(
                    value1.X + value1.Width,
                    value2.X + value2.Width);
                var left_side = Math.Max(value1.X, value2.X);
                var top_side = Math.Max(value1.Y, value2.Y);
                var bottom_side = Math.Min(
                    value1.Y + value1.Height,
                    value2.Y + value2.Height);
                result = new Rectangle(
                    left_side,
                    top_side,
                    right_side - left_side,
                    bottom_side - top_side);
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
