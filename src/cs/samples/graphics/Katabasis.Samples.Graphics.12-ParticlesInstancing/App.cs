// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Katabasis.Samples
{
	public class App : Game
	{
		private const int _maximumParticlesCount = 512 * 1024;
		private const int _particlesCountEmittedPerFrame = 10;
		private readonly VertexPosition[] _particlePositionOffsets = new VertexPosition[_maximumParticlesCount];
		private readonly Vector3[] _particleVelocities = new Vector3[_maximumParticlesCount];

		private readonly Random _random = new();
		private VertexBuffer _bufferInstances = null!;
		private IndexBuffer _bufferVertexIndices = null!;
		private VertexBuffer _bufferVertices = null!;
		private int _currentParticleCount;
		private float _rotationY;

		private Effect _shader = null!;

		private Matrix4x4 _viewProjectionMatrix;
		private Matrix4x4 _worldViewProjectionMatrix;

		public App() => Window.Title = "Katabasis Samples; Graphics: Particles Instancing";

		protected override void LoadContent()
		{
			_shader = CreateShader();
			_bufferVertices = CreateBufferVertices();
			_bufferVertexIndices = CreateBufferVertexIndices();
			_bufferInstances = CreateBufferInstanceData();
		}

		protected override void Draw(GameTime gameTime)
		{
			// we upload the particle positions before drawing so we ensure we only upload once per frame
			_bufferInstances.SetData(_particlePositionOffsets, 0, _currentParticleCount);

			GraphicsDevice.Clear(Color.Black);

			// bind vertex buffers
			GraphicsDevice.SetVertexBuffers(
				new VertexBufferBinding(_bufferVertices, 0, 0),
				new VertexBufferBinding(_bufferInstances, 0, 1));

			// bind index buffer
			GraphicsDevice.Indices = _bufferVertexIndices;

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
			_shader!.Techniques![0]!.Passes[0]!.Apply();

			// XNA crap: also we say the topology type of the vertices in the render loop; rasterizer should know this
			//    plus, in XNA we have `DrawIndexedPrimitives` and `DrawPrimitives`; we really only need `DrawElements`
			GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 6, 0, 8, _currentParticleCount);
		}

		protected override void Update(GameTime gameTime)
		{
			CreateViewProjectionMatrix();
			EmitNewParticles();
			MoveParticles();
			RotateModel(gameTime);
			
			base.Update(gameTime); // important to call the base.Update for updating internal stuff!
		}

		private void MoveParticles()
		{
			const float elapsedSeconds = 1 / 60f;
			for (var i = 0; i < _currentParticleCount; i++)
			{
				_particleVelocities[i].Y -= 1.0f * elapsedSeconds;
				_particlePositionOffsets[i].Position.X += _particleVelocities[i].X * elapsedSeconds;
				_particlePositionOffsets[i].Position.Y += _particleVelocities[i].Y * elapsedSeconds;
				_particlePositionOffsets[i].Position.Z += _particleVelocities[i].Z * elapsedSeconds;
				// ReSharper disable once InvertIf
				if (_particlePositionOffsets[i].Position.Y < -2.0f)
				{
					_particlePositionOffsets[i].Position.Y = -1.8f;
					_particleVelocities[i].Y = -_particleVelocities[i].Y;
					_particleVelocities[i].X *= 0.8f;
					_particleVelocities[i].Y *= 0.8f;
					_particleVelocities[i].Z *= 0.8f;
				}
			}
		}

		private void EmitNewParticles()
		{
			for (var i = 0; i < _particlesCountEmittedPerFrame; i++)
			{
				if (_currentParticleCount >= _maximumParticlesCount)
				{
					break;
				}

				_particlePositionOffsets[_currentParticleCount].Position = Vector3.Zero;
				_particleVelocities[_currentParticleCount] = new Vector3(
					((float)(_random.Next() & 0x7FFF) / 0x7FFF) - 0.5f,
					((float)(_random.Next() & 0x7FFF) / 0x7FFF * 0.5f) + 2.0f,
					((float)(_random.Next() & 0x7FFF) / 0x7FFF) - 0.5f);

				_currentParticleCount++;
			}
		}

		private static Effect CreateShader() => Effect.FromStream(File.OpenRead("Assets/Shaders/Main.fxb"));

		private static unsafe VertexBuffer CreateBufferVertices()
		{
			var vertices = (Span<VertexPositionColor>)stackalloc VertexPositionColor[24];

			const float r = 0.05f;
			// describe the vertices of the quad
			vertices[0].Position = new Vector3(0, -r, 0);
			vertices[0].Color = Color.Red;
			vertices[1].Position = new Vector3(r, 0, r);
			vertices[1].Color = Color.Green;
			vertices[2].Position = new Vector3(r, 0, -r);
			vertices[2].Color = Color.Blue;
			vertices[3].Position = new Vector3(-r, 0, -r);
			vertices[3].Color = Color.Yellow;
			vertices[4].Position = new Vector3(-r, 0, r);
			vertices[4].Color = Color.Cyan;
			vertices[5].Position = new Vector3(0, r, 0);
			vertices[5].Color = Color.Magenta;

			var buffer = new VertexBuffer(VertexPositionColor.Declaration, vertices.Length, BufferUsage.WriteOnly);
			ref var dataReference = ref MemoryMarshal.GetReference(vertices);
			var dataPointer = (IntPtr)Unsafe.AsPointer(ref dataReference);
			var dataSize = Marshal.SizeOf<VertexPositionColor>() * vertices.Length;
			buffer.SetDataPointerEXT(0, dataPointer, dataSize, SetDataOptions.None);

			return buffer;
		}

		private static unsafe IndexBuffer CreateBufferVertexIndices()
		{
			// the indices of the cube, here we define the triangles using the vertices from zero-based index
			var indices = (Span<ushort>)stackalloc ushort[] {0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 1, 5, 1, 2, 5, 2, 3, 5, 3, 4, 5, 4, 1};

			var buffer = new IndexBuffer(typeof(ushort), indices.Length, BufferUsage.WriteOnly);
			ref var dataReference = ref MemoryMarshal.GetReference(indices);
			var dataPointer = (IntPtr)Unsafe.AsPointer(ref dataReference);
			var dataSize = Marshal.SizeOf<ushort>() * indices.Length;
			buffer.SetDataPointerEXT(0, dataPointer, dataSize, SetDataOptions.None);
			return buffer;
		}

		private static VertexBuffer CreateBufferInstanceData()
		{
			var buffer = new DynamicVertexBuffer(VertexPosition.Declaration, _maximumParticlesCount, BufferUsage.WriteOnly);
			return buffer;
		}

		private void CreateViewProjectionMatrix()
		{
			var viewport = GraphicsDevice.Viewport;

			const float fieldOfViewDegrees = 40.0f;
			const float fieldOfViewRadians = (float)(fieldOfViewDegrees * Math.PI / 180);
			var aspectRatio = (float)viewport.Width / viewport.Height;
			const float nearPlaneDistance = 0.01f;
			const float farPlaneDistance = 50.0f;
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

			// here we use one model particle for all particles
			_rotationY += 1.0f * deltaSeconds;
			var rotationMatrixY = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, _rotationY);
			var modelMatrix = rotationMatrixY;
			_worldViewProjectionMatrix = modelMatrix * _viewProjectionMatrix;
		}
	}
}
