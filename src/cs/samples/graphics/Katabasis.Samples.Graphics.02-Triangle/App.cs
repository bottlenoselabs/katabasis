// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.IO;
using System.Numerics;

namespace bottlenoselabs.Katabasis.Samples
{
	public class App : Game
	{
		private Effect _shader = null!;
		private VertexBuffer _vertexBuffer = null!;

		public App() => Window.Title = "Katabasis Samples; Graphics: Triangle";

		protected override void LoadContent()
		{
			_shader = CreateShader();
			_vertexBuffer = CreateVertexBuffer();
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// bind vertex buffer
			GraphicsDevice.SetVertexBuffer(_vertexBuffer);

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
			GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
		}

		private static Effect CreateShader() => Effect.FromStream(File.OpenRead("Assets/Shaders/Main.fxb"));

		private static VertexBuffer CreateVertexBuffer()
		{
			var vertices = new Vertex[3];

			// vertices of triangle in clip-space (after model-to-world x world-to-view x view-to-projection transform)
			vertices[0].Position = new Vector3(0.0f, 0.5f, 0.5f);
			vertices[0].Color = Color.Red;
			vertices[1].Position = new Vector3(0.5f, -0.5f, 0.5f);
			vertices[1].Color = Color.Green;
			vertices[2].Position = new Vector3(-0.5f, -0.5f, 0.5f);
			vertices[2].Color = Color.Blue;

			var buffer = new VertexBuffer(Vertex.Declaration, vertices.Length, BufferUsage.WriteOnly);
			buffer.SetData(vertices);

			return buffer;
		}
	}
}
