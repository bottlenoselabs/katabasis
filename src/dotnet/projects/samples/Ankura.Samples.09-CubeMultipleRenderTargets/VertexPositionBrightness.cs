// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ankura.Samples.CubeMultipleRenderTargets
{
    internal struct VertexPositionBrightness : IVertexType
    {
        public Vector3 Position;
        public float Brightness;

        public static readonly VertexDeclaration Declaration;

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;

        static VertexPositionBrightness()
        {
            var elements = new[]
            {
                new VertexElement(
                    0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(
                    12, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
            };
            Declaration = new VertexDeclaration(elements);
        }
    }
}
