// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace bottlenoselabs.Katabasis
{
	[Serializable]
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public struct Viewport
	{
		internal FNA3D.FNA3D_Viewport _viewport;

		public int Height
		{
			get => _viewport.h;
			set => _viewport.h = value;
		}

		public float MaxDepth
		{
			get => _viewport.maxDepth;
			set => _viewport.maxDepth = value;
		}

		public float MinDepth
		{
			get => _viewport.minDepth;
			set => _viewport.minDepth = value;
		}

		public int Width
		{
			get => _viewport.w;
			set => _viewport.w = value;
		}

		public int Y
		{
			get => _viewport.y;
			set => _viewport.y = value;
		}

		public int X
		{
			get => _viewport.x;
			set => _viewport.x = value;
		}

		public float AspectRatio
		{
			get
			{
				if (_viewport.h != 0 && _viewport.w != 0)
				{
					return _viewport.w / (float)_viewport.h;
				}

				return 0.0f;
			}
		}

		public Rectangle Bounds
		{
			get =>
				new(
					_viewport.x,
					_viewport.y,
					_viewport.w,
					_viewport.h);

			set
			{
				_viewport.x = value.X;
				_viewport.y = value.Y;
				_viewport.w = value.Width;
				_viewport.h = value.Height;
			}
		}

		public Rectangle TitleSafeArea => Bounds;

		public Viewport(int x, int y, int width, int height)
		{
			_viewport.x = x;
			_viewport.y = y;
			_viewport.w = width;
			_viewport.h = height;
			_viewport.minDepth = 0.0f;
			_viewport.maxDepth = 1.0f;
		}

		public Viewport(Rectangle bounds)
		{
			_viewport.x = bounds.X;
			_viewport.y = bounds.Y;
			_viewport.w = bounds.Width;
			_viewport.h = bounds.Height;
			_viewport.minDepth = 0.0f;
			_viewport.maxDepth = 1.0f;
		}

		public Vector3 Project(
			Vector3 source,
			Matrix4x4 projection,
			Matrix4x4 view,
			Matrix4x4 world)
		{
			var matrix = Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection);
			var vector = Vector3.Transform(source, matrix);

			var a = (source.X * matrix.M14) + (source.Y * matrix.M24) + (source.Z * matrix.M34) + matrix.M44;
			if (!MathHelper.WithinEpsilon(a, 1.0f))
			{
				vector.X /= a;
				vector.Y /= a;
				vector.Z /= a;
			}

			vector.X = ((vector.X + 1f) * 0.5f * Width) + X;
			vector.Y = ((-vector.Y + 1f) * 0.5f * Height) + Y;
			vector.Z = (vector.Z * (MaxDepth - MinDepth)) + MinDepth;
			return vector;
		}

		public Vector3 Unproject(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
		{
			Matrix4x4.Invert(world * view * projection, out var matrix);
			source.X = ((source.X - X) / Width * 2f) - 1f;
			source.Y = -(((source.Y - Y) / Height * 2f) - 1f);
			source.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);
			var vector = Vector3.Transform(source, matrix);

			var a = (source.X * matrix.M14) + (source.Y * matrix.M24) + (source.Z * matrix.M34) + matrix.M44;
			if (!MathHelper.WithinEpsilon(a, 1.0f))
			{
				vector.X /= a;
				vector.Y /= a;
				vector.Z /= a;
			}

			return vector;
		}

		public override string ToString() =>
			"{" +
			"X:" + _viewport.x +
			" Y:" + _viewport.y +
			" Width:" + _viewport.w +
			" Height:" + _viewport.h +
			" MinDepth:" + _viewport.minDepth +
			" MaxDepth:" + _viewport.maxDepth +
			"}";
	}
}
