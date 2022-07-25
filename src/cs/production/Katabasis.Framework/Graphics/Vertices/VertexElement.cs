// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
namespace bottlenoselabs.Katabasis
{
	public struct VertexElement
	{
		public int Offset { get; set; }

		public VertexElementFormat VertexElementFormat { get; set; }

		public VertexElementUsage VertexElementUsage { get; set; }

		public int UsageIndex { get; set; }

		public VertexElement(
			int offset,
			VertexElementFormat elementFormat,
			VertexElementUsage elementUsage,
			int usageIndex)
			: this()
		{
			Offset = offset;
			UsageIndex = usageIndex;
			VertexElementFormat = elementFormat;
			VertexElementUsage = elementUsage;
		}

		public override int GetHashCode() =>
			// TODO: Fix hashes
			0;

		public override string ToString() =>
			"{{Offset:" + Offset +
			" Format:" + VertexElementFormat +
			" Usage:" + VertexElementUsage +
			" UsageIndex: " + UsageIndex +
			"}}";

		public override bool Equals(object? obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return this == (VertexElement)obj;
		}

		public static bool operator ==(VertexElement left, VertexElement right) =>
			left.Offset == right.Offset &&
			left.UsageIndex == right.UsageIndex &&
			left.VertexElementUsage == right.VertexElementUsage &&
			left.VertexElementFormat == right.VertexElementFormat;

		public static bool operator !=(VertexElement left, VertexElement right) => !(left == right);
	}
}
