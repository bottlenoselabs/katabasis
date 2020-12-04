// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Numerics;

namespace Katabasis.Samples
{
    internal struct Vertex : IVertexType
    {
        public Vector2 Position;

        public static readonly VertexDeclaration Declaration;

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;

        static Vertex()
        {
            var elements = new[]
            {
                new VertexElement(
                    0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0)
            };
            Declaration = new VertexDeclaration(elements);
        }
    }
}
