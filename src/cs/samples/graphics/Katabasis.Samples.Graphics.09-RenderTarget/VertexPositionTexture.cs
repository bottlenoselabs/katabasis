// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.Numerics;

namespace bottlenoselabs.Katabasis.Samples
{
	internal struct VertexPositionTexture : IVertexType
	{
		public Vector3 Position;
		public Vector2 TextureCoordinates;

		public static readonly VertexDeclaration Declaration;

		VertexDeclaration IVertexType.VertexDeclaration => Declaration;

		static VertexPositionTexture()
		{
			var elements = new[]
			{
				new VertexElement(
					0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(
					12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
			};

			Declaration = new VertexDeclaration(elements);
		}
	}
}
