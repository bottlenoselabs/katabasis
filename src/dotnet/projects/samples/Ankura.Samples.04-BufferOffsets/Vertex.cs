// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ankura.Samples.BufferOffsets
{
    internal struct Vertex : IVertexType
    {
        public Vector2 Position;
        public Color Color;

        public static readonly VertexDeclaration Declaration;

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;

        static Vertex()
        {
            var elements = new[]
            {
                new VertexElement(
                    0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(
                    8, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            };
            Declaration = new VertexDeclaration(elements);
        }
    }
}
