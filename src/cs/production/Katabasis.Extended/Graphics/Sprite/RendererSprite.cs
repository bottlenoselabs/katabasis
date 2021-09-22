// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Katabasis.Extended.Graphics
{
    public class RendererSprite
    {
        private const int MaximumSprites = 2048;

        private VertexBuffer _bufferInstances = null!;
        private IndexBuffer _bufferVertexIndices = null!;
        private VertexBuffer _bufferVertices = null!;

        private RendererSpriteInstanceVertex[] _sprites;
        private ushort[] _indices;

        private bool _isBeginCalled;
        private bool _isInitialized;

        public RendererSprite()
        {
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            LoadGraphicsResources();
        }

        public void Begin()
        {
            VerifyBeginNotCalled();

            _isBeginCalled = true;
        }

        public void End()
        {
            VerifyBeginCalled();

            _isBeginCalled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VerifyBeginNotCalled()
        {
            if (_isBeginCalled)
            {
                throw new InvalidOperationException(
                    @$"
The method '{nameof(Begin)}' has been called twice.
The method '{nameof(Begin)}' cannot be called again until '{nameof(End)}' has been successfully called.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VerifyBeginCalled()
        {
            if (!_isBeginCalled)
            {
                throw new InvalidOperationException(
                    @$"
The method '{nameof(End)}' was called, but the method '{nameof(Begin)}' has not yet been called.
You must call the method '{nameof(Begin)}' successfully before you can call the $'{nameof(End)}' method.
".Trim());
            }

            _isBeginCalled = false;
        }

        private void LoadGraphicsResources()
        {
            _sprites = new RendererSpriteInstanceVertex[MaximumSprites];
            _bufferInstances = CreateVertexBuffer();
            _bufferVertexIndices = CreateIndexBuffer();
        }

        private static VertexBuffer CreateVertexBuffer()
        {
            var buffer = new DynamicVertexBuffer(
                RendererSpriteInstanceVertex.Declaration,
                MaximumSprites,
                BufferUsage.WriteOnly);
            return buffer;
        }

        private static unsafe IndexBuffer CreateIndexBuffer()
        {
            /* Rectangle from two clockwise triangles:
                (0, 1, 2) and (0, 2, 3)
                1---------2
                |       / |
                │     /   |
                │   /     │
                │ /       │
                0---------3
            */

            Span<ushort> indices = stackalloc ushort[]
            {
                0, 1, 2,
                0, 2, 3
            };

            var buffer = new IndexBuffer(typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            ref var dataReference = ref MemoryMarshal.GetReference(indices);
            var dataPointer = (IntPtr)Unsafe.AsPointer(ref dataReference);
            var dataSize = Marshal.SizeOf<ushort>() * indices.Length;
            buffer.SetDataPointerEXT(0, dataPointer, dataSize, SetDataOptions.None);
            return buffer;
        }
    }
}
