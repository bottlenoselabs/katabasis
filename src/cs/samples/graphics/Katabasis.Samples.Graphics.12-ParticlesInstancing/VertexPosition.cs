// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.Numerics;

namespace bottlenoselabs.Katabasis.Samples
{
	internal struct VertexPosition : IVertexType
	{
		public Vector3 Position;

		public static readonly VertexDeclaration Declaration;

		VertexDeclaration IVertexType.VertexDeclaration => Declaration;

		static VertexPosition()
		{
			var elements = new[]
			{
				new VertexElement(
					0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
			};

			Declaration = new VertexDeclaration(elements);
		}
	}
}
