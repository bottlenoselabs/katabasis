// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.IO;
using System.Numerics;

namespace Katabasis.Samples
{
	public class App : Game
	{
		private IndexBuffer _indexBuffer = null!;
		private float _rotationX;
		private float _rotationY;
		private Effect _shader = null!;
		private VertexBuffer _vertexBuffer = null!;

		private Matrix4x4 _viewProjectionMatrix;
		private Matrix4x4 _worldViewProjectionMatrix;

		public App() => Window.Title = "Katabasis Samples; Graphics: Cube";

		protected override void LoadContent()
		{
			_shader = CreateShader();
			_vertexBuffer = CreateVertexBuffer();
			_indexBuffer = CreateIndexBuffer();
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Gray);

			// bind vertex buffer
			GraphicsDevice.SetVertexBuffer(_vertexBuffer);
			// bind index buffer
			GraphicsDevice.Indices = _indexBuffer;

			// XNA crap: we set our render pipeline state in the render loop before drawing
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			GraphicsDevice.DepthStencilState = DepthStencilState.None;

			// bind shader uniform
			var shaderParameterWorldViewProjectionMatrix = _shader!.Parameters!["WorldViewProjectionMatrix"];
			shaderParameterWorldViewProjectionMatrix!.SetValue(_worldViewProjectionMatrix);

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
			base.Update(gameTime); // important to call the base.Update for updating internal stuff!
		}

		private static Effect CreateShader() => Effect.FromStream(File.OpenRead("Assets/Shaders/Main.fxb"));

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
			var color1 = Color.Red; // #FF0000
			vertices[0].Position = new Vector3(leftX, bottomY, backZ);
			vertices[0].Color = color1;
			vertices[1].Position = new Vector3(rightX, bottomY, backZ);
			vertices[1].Color = color1;
			vertices[2].Position = new Vector3(rightX, topY, backZ);
			vertices[2].Color = color1;
			vertices[3].Position = new Vector3(leftX, topY, backZ);
			vertices[3].Color = color1;
			// rectangle 2; front
			var color2 = Color.Lime; // NOTE: "lime" is #00FF00; "green" is actually #008000
			vertices[4].Position = new Vector3(leftX, bottomY, frontZ);
			vertices[4].Color = color2;
			vertices[5].Position = new Vector3(rightX, bottomY, frontZ);
			vertices[5].Color = color2;
			vertices[6].Position = new Vector3(rightX, topY, frontZ);
			vertices[6].Color = color2;
			vertices[7].Position = new Vector3(leftX, topY, frontZ);
			vertices[7].Color = color2;
			// rectangle 3; left
			var color3 = Color.Blue; // #0000FF
			vertices[8].Position = new Vector3(leftX, bottomY, backZ);
			vertices[8].Color = color3;
			vertices[9].Position = new Vector3(leftX, topY, backZ);
			vertices[9].Color = color3;
			vertices[10].Position = new Vector3(leftX, topY, frontZ);
			vertices[10].Color = color3;
			vertices[11].Position = new Vector3(leftX, bottomY, frontZ);
			vertices[11].Color = color3;
			// rectangle 4; right
			var color4 = Color.Yellow; // #FFFF00
			vertices[12].Position = new Vector3(rightX, bottomY, backZ);
			vertices[12].Color = color4;
			vertices[13].Position = new Vector3(rightX, topY, backZ);
			vertices[13].Color = color4;
			vertices[14].Position = new Vector3(rightX, topY, frontZ);
			vertices[14].Color = color4;
			vertices[15].Position = new Vector3(rightX, bottomY, frontZ);
			vertices[15].Color = color4;
			// rectangle 5; bottom
			var color5 = Color.Aqua; // #00FFFF
			vertices[16].Position = new Vector3(leftX, bottomY, backZ);
			vertices[16].Color = color5;
			vertices[17].Position = new Vector3(leftX, bottomY, frontZ);
			vertices[17].Color = color5;
			vertices[18].Position = new Vector3(rightX, bottomY, frontZ);
			vertices[18].Color = color5;
			vertices[19].Position = new Vector3(rightX, bottomY, backZ);
			vertices[19].Color = color5;
			// rectangle 6; top
			var color6 = Color.Fuchsia; // #FF00FF
			vertices[20].Position = new Vector3(leftX, topY, backZ);
			vertices[20].Color = color6;
			vertices[21].Position = new Vector3(leftX, topY, frontZ);
			vertices[21].Color = color6;
			vertices[22].Position = new Vector3(rightX, topY, frontZ);
			vertices[22].Color = color6;
			vertices[23].Position = new Vector3(rightX, topY, backZ);
			vertices[23].Color = color6;

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

		private void CreateViewProjectionMatrix()
		{
			var viewport = GraphicsDevice.Viewport;

			const float fieldOfViewDegrees = 40.0f;
			const float fieldOfViewRadians = (float)(fieldOfViewDegrees * Math.PI / 180);
			var aspectRatio = (float)viewport.Width / viewport.Height;
			const float nearPlaneDistance = 0.01f;
			const float farPlaneDistance = 10.0f;
			var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
				fieldOfViewRadians, aspectRatio, nearPlaneDistance, farPlaneDistance);

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
