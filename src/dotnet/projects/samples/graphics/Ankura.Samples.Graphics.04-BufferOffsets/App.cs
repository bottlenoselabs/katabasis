// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.IO;
using System.Numerics;

namespace Ankura.Samples
{
    public class App : Game
    {
        private Effect _shader = null!;
        private VertexBuffer _vertexBuffer = null!;
        private IndexBuffer _indexBuffer = null!;

        public App()
        {
            Window.Title = "Ankura Samples; Graphics: Buffer Offsets";
        }

        protected override void LoadContent()
        {
            _shader = CreateShader();
            _vertexBuffer = CreateVertexBuffer();
            _indexBuffer = CreateIndexBuffer();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0x80, 0x80, 0xFF));

            // bind vertex buffer
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            // bind index buffer
            GraphicsDevice.Indices = _indexBuffer;

            // XNA crap: we set our render pipeline state in the render loop before drawing
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // XNA crap: we bind our shader program by going through "techniques" and "passes"
            //     please don't use these, you should only ever have use for one effect technique and one effect pass
            // NOTE: This applies any changes we have set for our render pipeline including:
            //     vertex buffers, index buffers, textures, samplers, blend, rasterizer, depth stencil, etc.
            _shader!.Techniques![0]!.Passes[0]!.Apply();

            // XNA crap: also we say the topology type of the vertices in the render loop; rasterizer should know this
            //    plus, in XNA we have `DrawIndexedPrimitives` and `DrawPrimitives`; we really only need `DrawElements`

            // triangle
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 3, 0, 1);
            // quad
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 3, 0, 4, 3, 2);
        }

        private static Effect CreateShader()
        {
            return Effect.FromStream(File.OpenRead("Assets/Shaders/Main.fxb"));
        }

        private static VertexBuffer CreateVertexBuffer()
        {
            var vertices = new Vertex[7];

            // vertices in clip-space (after model-to-world x world-to-view x view-to-projection transform)
            // the vertices of the triangle
            vertices[0].Position = new Vector2(0f, 0.55f);
            vertices[0].Color = Color.Red;
            vertices[1].Position = new Vector2(0.25f, 0.05f);
            vertices[1].Color = Color.Lime;
            vertices[2].Position = new Vector2(-0.25f, 0.05f);
            vertices[2].Color = Color.Blue;

            // the vertices of the rectangle
            vertices[3].Position = new Vector2(-0.25f, -0.05f);
            vertices[3].Color = Color.Blue;
            vertices[4].Position = new Vector2(0.25f, -0.05f);
            vertices[4].Color = Color.Lime;
            vertices[5].Position = new Vector2(0.25f, -0.55f);
            vertices[5].Color = Color.Red;
            vertices[6].Position = new Vector2(-0.25f, -0.55f);
            vertices[6].Color = Color.Yellow;

            var buffer = new VertexBuffer(Vertex.Declaration, vertices.Length, BufferUsage.WriteOnly);
            buffer.SetData(vertices);

            return buffer;
        }

        private static IndexBuffer CreateIndexBuffer()
        {
            var indices = new ushort[]
            {
                0, 1, 2, // triangle
                0, 1, 2, // triangle 1 of rectangle
                0, 2, 3 // triangle 2 of rectangle
            };

            var buffer = new IndexBuffer(typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            buffer.SetData(indices);
            return buffer;
        }
    }
}
