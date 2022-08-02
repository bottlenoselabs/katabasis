// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace bottlenoselabs.Katabasis
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionTexture : IVertexType
	{
		VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

		public Vector3 Position;
		public Vector2 TextureCoordinate;

		public static readonly VertexDeclaration VertexDeclaration;

		static VertexPositionTexture() =>
			VertexDeclaration = new VertexDeclaration(
				new VertexElement(
					0,
					VertexElementFormat.Vector3,
					VertexElementUsage.Position,
					0),
				new VertexElement(
					12,
					VertexElementFormat.Vector2,
					VertexElementUsage.TextureCoordinate,
					0));

		public VertexPositionTexture(Vector3 position, Vector2 textureCoordinate)
		{
			Position = position;
			TextureCoordinate = textureCoordinate;
		}

		public override int GetHashCode() =>
			// TODO: Fix GetHashCode
			0;

		public override string ToString() =>
			"{{Position:" + Position +
			" TextureCoordinate:" + TextureCoordinate +
			"}}";

		public static bool operator ==(VertexPositionTexture left, VertexPositionTexture right) =>
			left.Position == right.Position &&
			left.TextureCoordinate == right.TextureCoordinate;

		public static bool operator !=(VertexPositionTexture left, VertexPositionTexture right) => !(left == right);

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

			return this == (VertexPositionTexture)obj;
		}
	}
}
