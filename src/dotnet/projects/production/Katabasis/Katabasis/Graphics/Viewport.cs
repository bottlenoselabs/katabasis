// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Katabasis
{
    [Serializable]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
    public struct Viewport
    {
        internal FNA3D.FNA3D_Viewport _viewport;

        public int Height
        {
            get => _viewport.H;
            set => _viewport.H = value;
        }

        public float MaxDepth
        {
            get => _viewport.MaxDepth;
            set => _viewport.MaxDepth = value;
        }

        public float MinDepth
        {
            get => _viewport.MinDepth;
            set => _viewport.MinDepth = value;
        }

        public int Width
        {
            get => _viewport.W;
            set => _viewport.W = value;
        }

        public int Y
        {
            get => _viewport.Y;
            set => _viewport.Y = value;
        }

        public int X
        {
            get => _viewport.X;
            set => _viewport.X = value;
        }

        public float AspectRatio
        {
            get
            {
                if (_viewport.H != 0 && _viewport.W != 0)
                {
                    return _viewport.W / (float)_viewport.H;
                }

                return 0.0f;
            }
        }

        public Rectangle Bounds
        {
            get =>
                new Rectangle(
                    _viewport.X,
                    _viewport.Y,
                    _viewport.W,
                    _viewport.H);

            set
            {
                _viewport.X = value.X;
                _viewport.Y = value.Y;
                _viewport.W = value.Width;
                _viewport.H = value.Height;
            }
        }

        public Rectangle TitleSafeArea => Bounds;

        public Viewport(int x, int y, int width, int height)
        {
            _viewport.X = x;
            _viewport.Y = y;
            _viewport.W = width;
            _viewport.H = height;
            _viewport.MinDepth = 0.0f;
            _viewport.MaxDepth = 1.0f;
        }

        public Viewport(Rectangle bounds)
        {
            _viewport.X = bounds.X;
            _viewport.Y = bounds.Y;
            _viewport.W = bounds.Width;
            _viewport.H = bounds.Height;
            _viewport.MinDepth = 0.0f;
            _viewport.MaxDepth = 1.0f;
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

            var a = ((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34) + matrix.M44;
            if (!MathHelper.WithinEpsilon(a, 1.0f))
            {
                vector.X /= a;
                vector.Y /= a;
                vector.Z /= a;
            }

            return vector;
        }

        public override string ToString()
        {
            return "{" +
                   "X:" + _viewport.X +
                   " Y:" + _viewport.Y +
                   " Width:" + _viewport.W +
                   " Height:" + _viewport.H +
                   " MinDepth:" + _viewport.MinDepth +
                   " MaxDepth:" + _viewport.MaxDepth +
                   "}";
        }
    }
}
