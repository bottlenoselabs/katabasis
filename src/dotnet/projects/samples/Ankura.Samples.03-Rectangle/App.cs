// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ankura.Samples.Rectangle
{
    public class App : Game
    {
        private Effect _shader = null!;
        private VertexBuffer _vertexBuffer = null!;
        private IndexBuffer _indexBuffer = null!;

        public App()
        {
            Window.Title = "DNA Samples: Rectangle";
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            _shader = CreateShader();
            _vertexBuffer = CreateVertexBuffer();
            _indexBuffer = CreateIndexBuffer();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // bind vertex buffer
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            // bind index buffer
            GraphicsDevice.Indices = _indexBuffer;

            // XNA crap: we bind our shader program by going through "techniques" and "passes"
            //     please don't use these, you should only ever have use for one effect technique and one effect pass
            _shader.Techniques[0].Passes[0].Apply();

            // XNA crap: we set our render pipeline state in the render loop before drawing
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // XNA crap: also we say the topology type of the vertices in the render loop; rasterizer should know this
            //    plus, in XNA we have `DrawIndexedPrimitives` and `DrawPrimitives`; we really only need `DrawElements`
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }

        private Effect CreateShader()
        {
            return Content.Load<Effect>("Shaders/Main");
        }

        private unsafe VertexBuffer CreateVertexBuffer()
        {
            var vertices = (Span<Vertex>)stackalloc Vertex[4];

            // vertices of quad in clip-space (after model-to-world x world-to-view x view-to-projection transform)
            vertices[0].Position = new Vector3(-0.5f, 0.5f, 0.5f);
            vertices[0].Color = Color.Red;
            vertices[1].Position = new Vector3(0.5f, 0.5f, 0.5f);
            vertices[1].Color = Color.Green;
            vertices[2].Position = new Vector3(0.5f, -0.5f, 0.5f);
            vertices[2].Color = Color.Blue;
            vertices[3].Position = new Vector3(-0.5f, -0.5f, 0.5f);
            vertices[3].Color = Color.Yellow;

            var buffer = new VertexBuffer(Vertex.Declaration, vertices.Length, BufferUsage.WriteOnly);
            ref var dataReference = ref MemoryMarshal.GetReference(vertices);
            var dataPointer = (IntPtr)Unsafe.AsPointer(ref dataReference);
            var dataSize = Marshal.SizeOf<Vertex>() * vertices.Length;
            buffer.SetDataPointerEXT(0, dataPointer, dataSize, SetDataOptions.None);

            return buffer;
        }

        private unsafe IndexBuffer CreateIndexBuffer()
        {
            var indices = (Span<ushort>)stackalloc ushort[]
            {
                0, 1, 2, // triangle 1 indices
                0, 2, 3 // triangle 2 indices
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
