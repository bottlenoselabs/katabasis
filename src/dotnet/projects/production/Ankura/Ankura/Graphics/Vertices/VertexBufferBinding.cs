// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

namespace Ankura
{
    public readonly struct VertexBufferBinding
    {
        public int InstanceFrequency { get; }

        public VertexBuffer? VertexBuffer { get; }

        public int VertexOffset { get; }

        internal static readonly VertexBufferBinding None = new VertexBufferBinding(null);

        public VertexBufferBinding(VertexBuffer? vertexBuffer)
        {
            VertexBuffer = vertexBuffer;
            VertexOffset = 0;
            InstanceFrequency = 0;
        }

        public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset)
        {
            VertexBuffer = vertexBuffer;
            VertexOffset = vertexOffset;
            InstanceFrequency = 0;
        }

        public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset, int instanceFrequency)
        {
            VertexBuffer = vertexBuffer;
            VertexOffset = vertexOffset;
            InstanceFrequency = instanceFrequency;
        }

        public static implicit operator VertexBufferBinding(VertexBuffer buffer)
        {
            return new VertexBufferBinding(buffer);
        }
    }
}
