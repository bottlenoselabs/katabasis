// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

namespace Ankura
{
    public readonly struct GamePadTriggers
    {
        public float Left { get; }

        public float Right { get; }

        public GamePadTriggers(float leftTrigger, float rightTrigger)
        {
            Left = MathHelper.Clamp(leftTrigger, 0.0f, 1.0f);
            Right = MathHelper.Clamp(rightTrigger, 0.0f, 1.0f);
        }

        internal GamePadTriggers(
            float leftTrigger,
            float rightTrigger,
            GamePadDeadZone deadZoneMode)
        {
            /* XNA applies dead zones before rounding/clamping values.
             * The public constructor does not allow this because the
             * dead zone must be known first.
             */
            if (deadZoneMode == GamePadDeadZone.None)
            {
                Left = MathHelper.Clamp(leftTrigger, 0.0f, 1.0f);
                Right = MathHelper.Clamp(rightTrigger, 0.0f, 1.0f);
            }
            else
            {
                Left = MathHelper.Clamp(
                    GamePad.ExcludeAxisDeadZone(
                        leftTrigger,
                        GamePad.TriggerThreshold),
                    0.0f,
                    1.0f);
                Right = MathHelper.Clamp(
                    GamePad.ExcludeAxisDeadZone(
                        rightTrigger,
                        GamePad.TriggerThreshold),
                    0.0f,
                    1.0f);
            }
        }

        public static bool operator ==(GamePadTriggers left, GamePadTriggers right)
        {
            return MathHelper.WithinEpsilon(left.Left, right.Left) &&
                   MathHelper.WithinEpsilon(left.Right, right.Right);
        }

        public static bool operator !=(GamePadTriggers left, GamePadTriggers right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is GamePadTriggers triggers && this == triggers;
        }

        public override int GetHashCode()
        {
            return Left.GetHashCode() + Right.GetHashCode();
        }
    }
}
