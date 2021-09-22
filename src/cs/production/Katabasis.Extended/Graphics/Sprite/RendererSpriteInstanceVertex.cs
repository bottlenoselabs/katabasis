// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Numerics;
using System.Runtime.InteropServices;

namespace Katabasis.Extended.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RendererSpriteInstanceVertex : IVertexType
    {
        public Matrix3x2 TransformMatrix;
        public ushort SourceRectangleX;
        public ushort SourceRectangleY;
        public ushort SourceRectangleWidth;
        public ushort SourceRectangleHeight;
        public Color TintColor;
        public float Depth;

        public static readonly VertexDeclaration Declaration;

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;

        static RendererSpriteInstanceVertex()
        {
            var elements = new[]
            {
                new VertexElement(
                    0, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(
                    12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(
                    24, VertexElementFormat.Short4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(
                    32, VertexElementFormat.Color, VertexElementUsage.Color, 3),
                new VertexElement(
                    36, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4)
            };

            Declaration = new VertexDeclaration(elements);
        }
    }
}
