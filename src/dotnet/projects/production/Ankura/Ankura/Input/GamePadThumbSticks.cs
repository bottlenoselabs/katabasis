// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Numerics;

namespace Ankura
{
    public struct GamePadThumbSticks
    {
        private Vector2 _left;
        private Vector2 _right;

        public Vector2 Left => _left;

        public Vector2 Right => _right;

        public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition)
        {
            _left = leftPosition;
            _right = rightPosition;
            ApplySquareClamp();
        }

        internal GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition, GamePadDeadZone deadZoneMode)
        {
            /* XNA applies dead zones before rounding/clamping values.
             * The public constructor does not allow this because the
             * dead zone must be known first.
             */
            _left = leftPosition;
            _right = rightPosition;
            ApplyDeadZone(deadZoneMode);
            if (deadZoneMode == GamePadDeadZone.Circular)
            {
                ApplyCircularClamp();
            }
            else
            {
                ApplySquareClamp();
            }
        }

        private void ApplyDeadZone(GamePadDeadZone dz)
        {
            switch (dz)
            {
                case GamePadDeadZone.None:
                    break;
                case GamePadDeadZone.IndependentAxes:
                    _left.X = GamePad.ExcludeAxisDeadZone(_left.X, GamePad.LeftDeadZone);
                    _left.Y = GamePad.ExcludeAxisDeadZone(_left.Y, GamePad.LeftDeadZone);
                    _right.X = GamePad.ExcludeAxisDeadZone(_right.X, GamePad.RightDeadZone);
                    _right.Y = GamePad.ExcludeAxisDeadZone(_right.Y, GamePad.RightDeadZone);
                    break;
                case GamePadDeadZone.Circular:
                    _left = ExcludeCircularDeadZone(_left, GamePad.LeftDeadZone);
                    _right = ExcludeCircularDeadZone(_right, GamePad.RightDeadZone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dz), dz, null);
            }
        }

        private void ApplySquareClamp()
        {
            _left.X = MathHelper.Clamp(_left.X, -1.0f, 1.0f);
            _left.Y = MathHelper.Clamp(_left.Y, -1.0f, 1.0f);
            _right.X = MathHelper.Clamp(_right.X, -1.0f, 1.0f);
            _right.Y = MathHelper.Clamp(_right.Y, -1.0f, 1.0f);
        }

        private void ApplyCircularClamp()
        {
            if (_left.LengthSquared() > 1.0f)
            {
                _left = Vector2.Normalize(_left);
            }

            if (_right.LengthSquared() > 1.0f)
            {
                _right = Vector2.Normalize(_right);
            }
        }

        private static Vector2 ExcludeCircularDeadZone(Vector2 value, float deadZone)
        {
            var originalLength = value.Length();
            if (originalLength <= deadZone)
            {
                return Vector2.Zero;
            }

            var newLength = (originalLength - deadZone) / (1.0f - deadZone);
            return value * (newLength / originalLength);
        }

        public static bool operator ==(GamePadThumbSticks left, GamePadThumbSticks right)
        {
            return left._left == right._left && left._right == right._right;
        }

        public static bool operator !=(GamePadThumbSticks left, GamePadThumbSticks right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is GamePadThumbSticks sticks && this == sticks;
        }

        public override int GetHashCode()
        {
            return Left.GetHashCode() + (37 * Right.GetHashCode());
        }
    }
}
