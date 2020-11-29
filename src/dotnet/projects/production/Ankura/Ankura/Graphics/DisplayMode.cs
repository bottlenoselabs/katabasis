// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Ankura
{
    [Serializable]
    public class DisplayMode
    {
        public float AspectRatio => Width / (float)Height;

        public SurfaceFormat Format { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public Rectangle TitleSafeArea => new Rectangle(0, 0, Width, Height);

        internal DisplayMode(int width, int height, SurfaceFormat format)
        {
            Width = width;
            Height = height;
            Format = format;
        }

        public static bool operator !=(DisplayMode left, DisplayMode right)
        {
            return !(left == right);
        }

        public static bool operator ==(DisplayMode left, DisplayMode right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return left.Format == right.Format &&
                   left.Height == right.Height &&
                   left.Width == right.Width;
        }

        public override bool Equals(object? obj)
        {
            return obj as DisplayMode == this;
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ Height.GetHashCode() ^ Format.GetHashCode();
        }

        public override string ToString()
        {
            return "{{Width:" + Width +
                   " Height:" + Height +
                   " Format:" + Format +
                   "}}";
        }
    }
}
