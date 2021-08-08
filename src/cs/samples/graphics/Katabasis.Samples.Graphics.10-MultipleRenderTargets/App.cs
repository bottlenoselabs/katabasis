// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.IO;
using System.Numerics;

namespace Katabasis.Samples
{
	public class App : Game
	{
		private IndexBuffer _indexBuffer = null!;

		private Vector2 _offset;
		private RenderTarget2D[] _renderTargets = null!;
		private float _rotationX;
		private float _rotationY;
		private Effect _shaderDebug = null!;
		private Effect _shaderFullScreen = null!;
		private Effect _shaderOffScreen = null!;
		private VertexBuffer _vertexBufferPositionBrightness = null!;
		private VertexBuffer _vertexBufferTexture = null!;

		private Matrix4x4 _viewProjectionMatrix;
		private Matrix4x4 _worldViewProjectionMatrix;

		public App() => Window.Title = "Katabasis Samples; Graphics: Cube Multiple Render Targets (MRT)";

		protected override void LoadContent()
		{
			_shaderOffScreen = CreateShaderOffScreen();
			_shaderFullScreen = CreateShaderFullScreen();
			_shaderDebug = CreateShaderDebug();
			_vertexBufferTexture = CreateVertexBufferTexture();
			_vertexBufferPositionBrightness = CreateVertexBufferPositionBrightness();
			_indexBuffer = CreateIndexBuffer();

			_renderTargets = new RenderTarget2D[3];
			_renderTargets[0] = new RenderTarget2D(800, 600);
			_renderTargets[1] = new RenderTarget2D(800, 600);
			_renderTargets[2] = new RenderTarget2D(800, 600);
		}

		protected override void Draw(GameTime gameTime)
		{
			// XNA crap: this is how we say we want to START an offscreen pass MRT
			GraphicsDevice.SetRenderTargets(_renderTargets[0], _renderTargets[1], _renderTargets[2]);
			// clear the contents of the pass
			GraphicsDevice.Clear(Color.Black);

			// bind vertex buffer
			GraphicsDevice.SetVertexBuffer(_vertexBufferPositionBrightness);
			// bind index buffer
			GraphicsDevice.Indices = _indexBuffer;

			// XNA crap: we set our render pipeline state in the render loop before drawing
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			GraphicsDevice.DepthStencilState = DepthStencilState.None;

			// bind shader uniform
			var shaderParameterWorldViewProjectionMatrix = _shaderOffScreen!.Parameters!["WorldViewProjectionMatrix"];
			shaderParameterWorldViewProjectionMatrix!.SetValue(_worldViewProjectionMatrix);

			// XNA crap: we bind our shader program by going through "techniques" and "passes"
			//     please don't use these, you should only ever have use for one effect technique and one effect pass
			// NOTE: This applies any changes we have set for our render pipeline including:
			//     vertex buffers, index buffers, textures, samplers, blend, rasterizer, depth stencil, etc.
			_shaderOffScreen!.Techniques![0]!.Passes![0]!.Apply();

			// XNA crap: also we say the topology type of the vertices in the render loop; rasterizer should know this
			//    plus, in XNA we have `DrawIndexedPrimitives` and `DrawPrimitives`; we really only need `DrawElements`
			GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

			// XNA crap: this is how we say we want to END an offscreen pass, AND by consequence start the screen pass
			GraphicsDevice.SetRenderTargets(null);
			GraphicsDevice.Clear(Color.Gray);

			// bind vertex buffer
			GraphicsDevice.SetVertexBuffer(_vertexBufferTexture);
			// bind textures
			GraphicsDevice.Textures[0] = _renderTargets[0];
			GraphicsDevice.Textures[1] = _renderTargets[1];
			GraphicsDevice.Textures[2] = _renderTargets[2];

			// XNA crap: we set our render pipeline state in the render loop before drawing
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			GraphicsDevice.DepthStencilState = DepthStencilState.None;
			// XNA crap: texture filtering set in the render loop
			//     PLUS it's "global state" as opposed to texture instance specific
			GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

			// bind shader uniform
			var shaderParameterOffset = _shaderFullScreen.Parameters!["Offset"];
			shaderParameterOffset!.SetValue(_offset);

			// XNA crap: we bind our shader program by going through "techniques" and "passes"
			//     please don't use these, you should only ever have use for one effect technique and one effect pass
			// NOTE: This applies any changes we have set for our render pipeline including:
			//     vertex buffers, index buffers, textures, samplers, blend, rasterizer, depth stencil, etc.
			_shaderFullScreen!.Techniques![0]!.Passes![0]!.Apply();

			// XNA crap: also we say the topology type of the vertices in the render loop; rasterizer should know this
			//    plus, in XNA we have `DrawIndexedPrimitives` and `DrawPrimitives`; we really only need `DrawElements`
			GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 4);

			// bind vertex buffer
			GraphicsDevice.SetVertexBuffer(_vertexBufferTexture);

			// XNA crap: we set our render pipeline state in the render loop before drawing
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			GraphicsDevice.DepthStencilState = DepthStencilState.None;
			// XNA crap: texture filtering set in the render loop
			//     PLUS it's "global state" as opposed to texture instance specific
			GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

			// XNA crap: we bind our shader program by going through "techniques" and "passes"
			//     please don't use these, you should only ever have use for one effect technique and one effect pass
			// NOTE: This applies any changes we have set for our render pipeline including:
			//     vertex buffers, index buffers, textures, samplers, blend, rasterizer, depth stencil, etc.
			_shaderDebug!.Techniques![0]!.Passes![0]!.Apply();

			var originalViewport = GraphicsDevice.Viewport;
			for (var i = 0; i < 3; i++)
			{
				GraphicsDevice.Viewport = new Viewport(i * 50, 0, 50, 50);
				GraphicsDevice.Textures[0] = _renderTargets[i];
				// XNA crap: also we say the topology type of the vertices in the render loop; rasterizer should know this
				//    plus, in XNA we have `DrawIndexedPrimitives` and `DrawPrimitives`; we really only need `DrawElements`
				GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 4);
			}

			GraphicsDevice.Viewport = originalViewport;
		}

		protected override void Update(GameTime gameTime)
		{
			CreateViewProjectionMatrix();
			RotateModel(gameTime);

			// update offset used in shader
			ref var offset = ref _offset;
			offset.X = (float)(0.1 * Math.Sin(_rotationX));
			offset.Y = (float)(0.1 * Math.Sin(_rotationY));
		}

		private static Effect CreateShaderOffScreen() => Effect.FromStream(File.OpenRead("Assets/Shaders/OffScreen.fxb"));

		private static Effect CreateShaderFullScreen() => Effect.FromStream(File.OpenRead("Assets/Shaders/FullScreen.fxb"));

		private static Effect CreateShaderDebug() => Effect.FromStream(File.OpenRead("Assets/Shaders/Debug.fxb"));

		private static VertexBuffer CreateVertexBufferTexture()
		{
			// xna crap: xna is locked to DirectX 9.0c - Shader Model 3.0;
			//     in d3d10 (SM4) the system-value semantic `SV_VertexID` exists which allows to fetch these static texture
			//     coordinates directly from a internal constant buffer; thus in d3d10 we don't need this vertex buffer

			var vertices = new VertexTexture[4];

			vertices[0].TextureCoordinates = new Vector2(0, 0);
			vertices[1].TextureCoordinates = new Vector2(1, 0);
			vertices[2].TextureCoordinates = new Vector2(0, 1);
			vertices[3].TextureCoordinates = new Vector2(1, 1);

			var buffer = new VertexBuffer(VertexTexture.Declaration, vertices.Length, BufferUsage.WriteOnly);
			buffer.SetData(vertices);

			return buffer;
		}

		private static VertexBuffer CreateVertexBufferPositionBrightness()
		{
			var vertices = new VertexPositionBrightness[24];

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
			const float brightness1 = 1f;
			vertices[0].Position = new Vector3(leftX, bottomY, backZ);
			vertices[0].Brightness = brightness1;
			vertices[1].Position = new Vector3(rightX, bottomY, backZ);
			vertices[1].Brightness = brightness1;
			vertices[2].Position = new Vector3(rightX, topY, backZ);
			vertices[2].Brightness = brightness1;
			vertices[3].Position = new Vector3(leftX, topY, backZ);
			vertices[3].Brightness = brightness1;
			// rectangle 2; front
			const float brightness2 = 0.8f;
			vertices[4].Position = new Vector3(leftX, bottomY, frontZ);
			vertices[4].Brightness = brightness2;
			vertices[5].Position = new Vector3(rightX, bottomY, frontZ);
			vertices[5].Brightness = brightness2;
			vertices[6].Position = new Vector3(rightX, topY, frontZ);
			vertices[6].Brightness = brightness2;
			vertices[7].Position = new Vector3(leftX, topY, frontZ);
			vertices[7].Brightness = brightness2;
			// rectangle 3; left
			const float brightness3 = 0.6f;
			vertices[8].Position = new Vector3(leftX, bottomY, backZ);
			vertices[8].Brightness = brightness3;
			vertices[9].Position = new Vector3(leftX, topY, backZ);
			vertices[9].Brightness = brightness3;
			vertices[10].Position = new Vector3(leftX, topY, frontZ);
			vertices[10].Brightness = brightness3;
			vertices[11].Position = new Vector3(leftX, bottomY, frontZ);
			vertices[11].Brightness = brightness3;
			// rectangle 4; right
			const float brightness4 = 0.4f;
			vertices[12].Position = new Vector3(rightX, bottomY, backZ);
			vertices[12].Brightness = brightness4;
			vertices[13].Position = new Vector3(rightX, topY, backZ);
			vertices[13].Brightness = brightness4;
			vertices[14].Position = new Vector3(rightX, topY, frontZ);
			vertices[14].Brightness = brightness4;
			vertices[15].Position = new Vector3(rightX, bottomY, frontZ);
			vertices[15].Brightness = brightness4;
			// rectangle 5; bottom
			const float brightness5 = 0.5f;
			vertices[16].Position = new Vector3(leftX, bottomY, backZ);
			vertices[16].Brightness = brightness5;
			vertices[17].Position = new Vector3(leftX, bottomY, frontZ);
			vertices[17].Brightness = brightness5;
			vertices[18].Position = new Vector3(rightX, bottomY, frontZ);
			vertices[18].Brightness = brightness5;
			vertices[19].Position = new Vector3(rightX, bottomY, backZ);
			vertices[19].Brightness = brightness5;
			// rectangle 6; top
			const float brightness6 = 0.7f;
			vertices[20].Position = new Vector3(leftX, topY, backZ);
			vertices[20].Brightness = brightness6;
			vertices[21].Position = new Vector3(leftX, topY, frontZ);
			vertices[21].Brightness = brightness6;
			vertices[22].Position = new Vector3(rightX, topY, frontZ);
			vertices[22].Brightness = brightness6;
			vertices[23].Position = new Vector3(rightX, topY, backZ);
			vertices[23].Brightness = brightness6;

			var buffer = new VertexBuffer(VertexPositionBrightness.Declaration, vertices.Length, BufferUsage.WriteOnly);
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

		private void CreateViewProjectionMatrix()
		{
			var viewport = GraphicsDevice.Viewport;

			const float fieldOfViewDegrees = 40.0f;
			const float fieldOfViewRadians = (float)(fieldOfViewDegrees * Math.PI / 180);
			var aspectRatio = (float)viewport.Width / viewport.Height;
			const float nearPlaneDistance = 0.01f;
			const float farPlaneDistance = 10.0f;
			var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfViewRadians, aspectRatio, nearPlaneDistance, farPlaneDistance);

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
			_rotationY += 2.0f * deltaSeconds;
			var rotationMatrixX = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, _rotationX);
			var rotationMatrixY = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, _rotationY);
			var modelToWorldMatrix = rotationMatrixX * rotationMatrixY;

			_worldViewProjectionMatrix = modelToWorldMatrix * _viewProjectionMatrix;
		}
	}
}
