// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.IO;
using System.Numerics;

namespace Ankura.Samples
{
    public class App : Game
    {
        private Effect _shader = null!;
        private VertexBuffer _vertexBuffer = null!;
        private IndexBuffer _indexBuffer = null!;
        private Texture3D _texture = null!;

        private Matrix4x4 _viewProjectionMatrix;
        private Matrix4x4 _worldViewProjectionMatrix;
        private float _textureCoordinatesScale;
        private float _rotationX;
        private float _rotationY;

        private static uint _xorShift32State = 0x12345678;

        public App()
        {
            Window.Title = "Ankura Samples; Graphics: Cube Texture 3D";
        }

        protected override void LoadContent()
        {
            _shader = CreateShader();
            _vertexBuffer = CreateVertexBuffer();
            _indexBuffer = CreateIndexBuffer();
            _texture = CreateTexture();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            // bind vertex buffer
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            // bind index buffer
            GraphicsDevice.Indices = _indexBuffer;
            // bind texture
            GraphicsDevice.Textures[0] = _texture;

            // XNA crap: we set our render pipeline state in the render loop before drawing
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            // XNA crap: texture filtering set in the render loop
            //     PLUS it's "global state" as opposed to texture instance specific
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            // bind shader uniforms
            var shaderParameterWorldViewProjectionMatrix = _shader.Parameters!["WorldViewProjectionMatrix"];
            shaderParameterWorldViewProjectionMatrix!.SetValue(_worldViewProjectionMatrix);
            var shaderParameterScale = _shader.Parameters!["Scale"];
            shaderParameterScale!.SetValue(_textureCoordinatesScale);

            // XNA crap: we bind our shader program by going through "techniques" and "passes"
            //     please don't use these, you should only ever have use for one effect technique and one effect pass
            // NOTE: This applies any changes we have set for our render pipeline including:
            //     vertex buffers, index buffers, textures, samplers, blend, rasterizer, depth stencil, etc.
            _shader.Techniques![0]!.Passes[0]!.Apply();

            // XNA crap: also we say the topology type of the vertices in the render loop; rasterizer should know this
            //    plus, in XNA we have `DrawIndexedPrimitives` and `DrawPrimitives`; we really only need `DrawElements`
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
        }

        protected override void Update(GameTime gameTime)
        {
            CreateViewProjectionMatrix();
            RotateModel(gameTime);
            CalculateTextureCoordinatesScale(gameTime);
        }

        private static Effect CreateShader()
        {
            return Effect.FromStream(File.OpenRead("Assets/Shaders/Main.fxb"));
        }

        private static VertexBuffer CreateVertexBuffer()
        {
            var vertices = new Vertex[24];

            // model vertices of the cube using standard cartesian coordinate system:
            //    +Z is towards your eyes, -Z is towards the screen
            //    +X is to the right, -X to the left
            //    +Y is towards the sky (up), -Y is towards the floor (down)
            const float leftX = -1.0f;
            const float rightX = 1.0f;
            const float bottomY = -1.0f;
            const float topY = 1.0f;
            const float backZ = -1.0f;
            const float frontZ = 1.0f;

            // each face of the cube is a rectangle (two triangles), each rectangle is 4 vertices
            // rectangle 1; back
            vertices[0].Position = new Vector3(leftX, bottomY, backZ);
            vertices[1].Position = new Vector3(rightX, bottomY, backZ);
            vertices[2].Position = new Vector3(rightX, topY, backZ);
            vertices[3].Position = new Vector3(leftX, topY, backZ);
            // rectangle 2; front
            vertices[4].Position = new Vector3(leftX, bottomY, frontZ);
            vertices[5].Position = new Vector3(rightX, bottomY, frontZ);
            vertices[6].Position = new Vector3(rightX, topY, frontZ);
            vertices[7].Position = new Vector3(leftX, topY, frontZ);
            // rectangle 3; left
            vertices[8].Position = new Vector3(leftX, bottomY, backZ);
            vertices[9].Position = new Vector3(leftX, topY, backZ);
            vertices[10].Position = new Vector3(leftX, topY, frontZ);
            vertices[11].Position = new Vector3(leftX, bottomY, frontZ);
            // rectangle 4; right
            vertices[12].Position = new Vector3(rightX, bottomY, backZ);
            vertices[13].Position = new Vector3(rightX, topY, backZ);
            vertices[14].Position = new Vector3(rightX, topY, frontZ);
            vertices[15].Position = new Vector3(rightX, bottomY, frontZ);
            // rectangle 5; bottom
            vertices[16].Position = new Vector3(leftX, bottomY, backZ);
            vertices[17].Position = new Vector3(leftX, bottomY, frontZ);
            vertices[18].Position = new Vector3(rightX, bottomY, frontZ);
            vertices[19].Position = new Vector3(rightX, bottomY, backZ);
            // rectangle 6; top
            vertices[20].Position = new Vector3(leftX, topY, backZ);
            vertices[21].Position = new Vector3(leftX, topY, frontZ);
            vertices[22].Position = new Vector3(rightX, topY, frontZ);
            vertices[23].Position = new Vector3(rightX, topY, backZ);

            var buffer = new VertexBuffer(Vertex.Declaration, vertices.Length, BufferUsage.WriteOnly);
            buffer.SetData(vertices);

            return buffer;
        }

        private static IndexBuffer CreateIndexBuffer()
        {
            // the indices of the cube, here we define the triangles using the vertices from zero-based index
            var indices = new ushort[]
            {
                0, 1, 2, 0, 2, 3, // rectangle 1 of cube, back, clockwise, base vertex: 0
                6, 5, 4, 7, 6, 4, // rectangle 2 of cube, front, counter-clockwise, base vertex: 4
                8, 9, 10, 8, 10, 11, // rectangle 3 of cube, left, clockwise, base vertex: 8
                14, 13, 12, 15, 14, 12, // rectangle 4 of cube, right, counter-clockwise, base vertex: 12
                16, 17, 18, 16, 18, 19, // rectangle 5 of cube, bottom, clockwise, base vertex: 16
                22, 21, 20, 23, 22, 20 // rectangle 6 of cube, top, counter-clockwise, base vertex: 20
            };

            var buffer = new IndexBuffer(typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            buffer.SetData(indices);
            return buffer;
        }

        private static Texture3D CreateTexture()
        {
            const int textureSize = 32;
            var pixelData = new Color[(int)Math.Pow(textureSize, 3)];
            for (var x = 0; x < textureSize; x++)
            {
                for (var y = 0; y < textureSize; y++)
                {
                    for (var z = 0; z < textureSize; z++)
                    {
                        var randomU32 = XORShift32(ref _xorShift32State);
                        var red = (byte)(randomU32 >> 24);
                        var green = (byte)(randomU32 >> 16);
                        var blue = (byte)(randomU32 >> 8);
                        var alpha = (byte)randomU32;
                        var color = new Color(red, green, blue, alpha);
                        var index = x + (textureSize * (y + (textureSize * z)));
                        pixelData[index] = color;
                    }
                }
            }

            var texture = new Texture3D(
                textureSize,
                textureSize,
                textureSize,
                false,
                SurfaceFormat.Color);
            texture.SetData(pixelData);
            return texture;
        }

        private static uint XORShift32(ref uint state)
        {
            // This is a fast but unsecure pseudo random number generator: https://en.wikipedia.org/wiki/Xorshift
            var x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            return state = x;
        }

        private void CreateViewProjectionMatrix()
        {
            var viewport = GraphicsDevice.Viewport;

            const float fieldOfViewDegrees = 40.0f;
            const float fieldOfViewRadians = (float)(fieldOfViewDegrees * Math.PI / 180);
            var aspectRatio = (float)viewport.Width / viewport.Height;
            const float nearPlaneDistance = 0.01f;
            const float farPlaneDistance = 10.0f;
            var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                fieldOfViewRadians,
                aspectRatio,
                nearPlaneDistance,
                farPlaneDistance);

            var cameraPosition = new Vector3(0.0f, 1.5f, 6.0f);
            var cameraTarget = Vector3.Zero;
            var cameraUpVector = Vector3.UnitY;
            var viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);

            _viewProjectionMatrix = viewMatrix * projectionMatrix;
        }

        private void RotateModel(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _rotationX += 1.0f * deltaSeconds;
            _rotationY += 1.5f * deltaSeconds;
            var rotationMatrixX = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, _rotationX);
            var rotationMatrixY = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, _rotationY);
            var modelToWorldMatrix = rotationMatrixX * rotationMatrixY;

            _worldViewProjectionMatrix = modelToWorldMatrix * _viewProjectionMatrix;
        }

        private void CalculateTextureCoordinatesScale(GameTime gameTime)
        {
            var totalSeconds = gameTime.TotalGameTime.TotalSeconds;
            var radians = totalSeconds;
            // [0, 1] sinusoid = (cos(x - Pi) + 1) * 0.5 from [0, 2c * Pi] where c is an integer
            var scale = (float)((Math.Cos(radians - MathHelper.Pi) + 1.0) * 0.5);
            // perform Hermite lerp to emphasis start and end positions: https://thebookofshaders.com/glossary/?search=smoothstep
            var smoothScale = MathHelper.SmoothStep(0.02f, 1.0f, scale);
            _textureCoordinatesScale = smoothScale;
        }
    }
}
