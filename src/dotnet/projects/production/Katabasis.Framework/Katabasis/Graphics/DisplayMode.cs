// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[Serializable]
	public class DisplayMode
	{
		internal DisplayMode(int width, int height, SurfaceFormat format)
		{
			Width = width;
			Height = height;
			Format = format;
		}

		public float AspectRatio => Width / (float)Height;

		public SurfaceFormat Format { get; private set; }

		public int Height { get; private set; }

		public int Width { get; private set; }

		public Rectangle TitleSafeArea => new(0, 0, Width, Height);

		public static bool operator !=(DisplayMode left, DisplayMode right) => !(left == right);

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
			if (!(obj is DisplayMode displayMode))
			{
				return false;
			}

			return displayMode == this;
		}

		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
		public override int GetHashCode() => Width.GetHashCode() ^ Height.GetHashCode() ^ Format.GetHashCode();

		public override string ToString() =>
			"{{Width:" + Width +
			" Height:" + Height +
			" Format:" + Format +
			"}}";
	}
}
