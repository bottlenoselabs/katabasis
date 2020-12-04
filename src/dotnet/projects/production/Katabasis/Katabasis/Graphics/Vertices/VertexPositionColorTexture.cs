// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Katabasis
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorTexture : IVertexType
    {
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public Vector3 Position;
        public Color Color;
        public Vector2 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration;

        static VertexPositionColorTexture()
        {
            VertexDeclaration = new VertexDeclaration(
                new VertexElement(
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0),
                new VertexElement(
                    12,
                    VertexElementFormat.Color,
                    VertexElementUsage.Color,
                    0),
                new VertexElement(
                    16,
                    VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate,
                    0));
        }

        public VertexPositionColorTexture(
            Vector3 position,
            Color color,
            Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }

        public override int GetHashCode()
        {
            // TODO: Fix GetHashCode
            return 0;
        }

        public override string ToString()
        {
            return $"{{{{Position:{Position} Color:{Color} TextureCoordinate:{TextureCoordinate}}}}}";
        }

        public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return left.Position == right.Position &&
                   left.Color == right.Color &&
                   left.TextureCoordinate == right.TextureCoordinate;
        }

        public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return !(left == right);
        }

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

            return this == (VertexPositionColorTexture)obj;
        }
    }
}
