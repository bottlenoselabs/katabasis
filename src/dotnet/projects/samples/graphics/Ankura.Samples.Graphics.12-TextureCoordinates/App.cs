// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.IO;
using System.Numerics;

namespace Ankura.Samples
{
    public class App : Game
    {
        private VertexBuffer _vertexBuffer = null!;
        private Effect _shader = null!;
        private Texture2D _texture = null!;
        private SamplerState[] _samplerStates = null!;

        public App()
        {
            Window.Title = "Ankura Samples; Graphics: Texture Coordinates";
        }

        protected override void LoadContent()
        {
            _shader = CreateShader();
            _vertexBuffer = CreateVertexBuffer();
            _texture = CreateTexture();
            _samplerStates = CreateSamplerStates();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            for (var i = 0; i < _samplerStates.Length; i++)
            {
                var samplerState = _samplerStates[i];
                var offset = i switch
                {
                    0 => new Vector2(-0.5f, 0.5f),
                    1 => new Vector2(0.5f, 0.5f),
                    2 => new Vector2(-0.5f, -0.5f),
                    _ => throw new NotImplementedException()
                };

                DrawTexturedRectangle(samplerState, offset, new Vector2(0.4f));
            }
        }

        private void DrawTexturedRectangle(SamplerState samplerState, Vector2 position, Vector2 scale)
        {
            // bind vertex buffer
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);

            // XNA crap: we set our render pipeline state in the render loop before drawing
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // bind texture
            GraphicsDevice.Textures[0] = _texture;

            // XNA crap: texture filtering set in the render loop
            //     PLUS it's "global state" as opposed to texture instance specific
            GraphicsDevice.SamplerStates[0] = samplerState;

            // bind shader uniforms
            var shaderParameterScale = _shader!.Parameters!["Scale"];
            shaderParameterScale!.SetValue(scale);
            var shaderParameterOffset = _shader!.Parameters!["Offset"];
            shaderParameterOffset!.SetValue(position);

            // XNA crap: we bind our shader program by going through "techniques" and "passes"
            //     please don't use these, you should only ever have use for one effect technique and one effect pass
            _shader!.Techniques![0]!.Passes[0]!.Apply();

            // XNA crap: also we say the topology type of the vertices in the render loop; rasterizer should know this
            //    plus, in XNA we have `DrawIndexedPrimitives` and `DrawPrimitives`; we really only need `DrawElements`
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 4);
        }

        private static Effect CreateShader()
        {
            return Effect.FromStream(File.OpenRead("Assets/Shaders/Main.fxb"));
        }

        private static VertexBuffer CreateVertexBuffer()
        {
            var vertices = new Vertex[4];

            // model vertices of the rectangle
            vertices[0].Position = new Vector2(-1.0f, 1.0f); // top-left
            vertices[1].Position = new Vector2(1.0f, 1.0f); // top-right
            vertices[2].Position = new Vector2(-1.0f, -1.0f); // bottom-right
            vertices[3].Position = new Vector2(1.0f, -1.0f); // bottom-left

            var buffer = new VertexBuffer(Vertex.Declaration, vertices.Length, BufferUsage.WriteOnly);
            buffer.SetData(vertices);

            return buffer;
        }

        private static Texture2D CreateTexture()
        {
            var e = Color.Gray; // empty
            var w = Color.White; // white
            var r = Color.Red; // red
            var g = Color.Lime; // green
            var b = Color.Blue; // blue
            var y = Color.Yellow; // yellow
            var pixels = new[]
            {
                r, r, r, r, g, g, g, g,
                r, e, e, e, e, e, e, g,
                r, e, e, e, e, e, e, g,
                r, e, e, w, w, e, e, g,
                b, e, e, w, w, e, e, y,
                b, e, e, e, e, e, e, y,
                b, e, e, e, e, e, e, y,
                b, b, b, b, y, y, y, y
            };

            var size = (int)Math.Sqrt(pixels.Length);
            var texture = new Texture2D(size, size);
            texture.SetData(pixels);
            return texture;
        }

        private static SamplerState[] CreateSamplerStates()
        {
            // XNA 4.0 doesn't have clamp to border!
            var samplerStates = new[]
            {
                new SamplerState
                {
                    Name = "Repeat",
                    Filter = TextureFilter.Point,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                },
                new SamplerState
                {
                    Name = "Clamp to Edge",
                    Filter = TextureFilter.Point,
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                },
                new SamplerState
                {
                    Name = "Mirrored Repeat",
                    Filter = TextureFilter.Point,
                    AddressU = TextureAddressMode.Mirror,
                    AddressV = TextureAddressMode.Mirror,
                },
            };

            return samplerStates;
        }
    }
}
