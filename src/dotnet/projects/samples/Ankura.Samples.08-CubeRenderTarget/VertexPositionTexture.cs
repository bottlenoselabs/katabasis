// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ankura.Samples.CubeRenderTarget
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
