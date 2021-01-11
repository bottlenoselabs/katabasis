// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	/// <summary>
	///     Contains commonly used precalculated values and mathematical operations.
	/// </summary>
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
	public static class MathHelper
	{
		public const float E = (float)Math.E;

		public const float Log10E = 0.4342945f;

		public const float Log2E = 1.442695f;

		public const float Pi = (float)Math.PI;

		public const float PiOver2 = (float)(Math.PI / 2.0);

		public const float PiOver4 = (float)(Math.PI / 4.0);

		public const float TwoPi = (float)(Math.PI * 2.0);
		internal static readonly float MachineEpsilonFloat = GetMachineEpsilonFloat();

		/// <summary>
		///     Find the current machine's Epsilon for the float data type.
		///     (That is, the largest float, e,  where e == 0.0f is true.)
		/// </summary>
		private static float GetMachineEpsilonFloat()
		{
			var machineEpsilon = 1.0f;
			float comparison;

			/* Keep halving the working value of machineEpsilon until we get a number that
			 * when added to 1.0f will still evaluate as equal to 1.0f.
			 */
			do
			{
				machineEpsilon *= 0.5f;
				comparison = 1.0f + machineEpsilon;
			} while (comparison > 1.0f);

			return machineEpsilon;
		}

		public static float Barycentric(
			float value1,
			float value2,
			float value3,
			float amount1,
			float amount2) =>
			value1 + ((value2 - value1) * amount1) + ((value3 - value1) * amount2);

		public static float CatmullRom(
			float value1,
			float value2,
			float value3,
			float value4,
			float amount)
		{
			/* Using formula from http://www.mvps.org/directx/articles/catmull/
			 * Internally using doubles not to lose precision.
			 */
			double amountSquared = amount * amount;
			var amountCubed = amountSquared * amount;
			return (float)(
				0.5 *
				((2.0 * value2) + ((value3 - value1) * amount) +
				 (((2.0 * value1) - (5.0 * value2) + (4.0 * value3) - value4) * amountSquared) +
				 (((3.0 * value2) - value1 - (3.0 * value3) + value4) * amountCubed)));
		}

		public static float Clamp(float value, float min, float max)
		{
			// First we check to see if we're greater than the max.
			value = value > max ? max : value;

			// Then we check to see if we're less than the min.
			value = value < min ? min : value;

			// There's no check to see if min > max.
			return value;
		}

		public static float Distance(float value1, float value2) => Math.Abs(value1 - value2);

		public static float Hermite(
			float value1,
			float tangent1,
			float value2,
			float tangent2,
			float amount)
		{
			/* All transformed to double not to lose precision
			 * Otherwise, for high numbers of param:amount the result is NaN instead
			 * of Infinity.
			 */
			double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount;
			double result;
			var amountCubed = s * s * s;
			var amountSquared = s * s;

			if (WithinEpsilon(amount, 0f))
			{
				result = value1;
			}
			else if (WithinEpsilon(amount, 1f))
			{
				result = value2;
			}
			else
			{
				result = (((2 * v1) - (2 * v2) + t2 + t1) * amountCubed) +
				         (((3 * v2) - (3 * v1) - (2 * t1) - t2) * amountSquared) +
				         (t1 * s) +
				         v1;
			}

			return (float)result;
		}

		public static float Lerp(float value1, float value2, float amount) => value1 + ((value2 - value1) * amount);

		public static float Max(float value1, float value2) => value1 > value2 ? value1 : value2;

		public static float Min(float value1, float value2) => value1 < value2 ? value1 : value2;

		public static float SmoothStep(float value1, float value2, float amount)
		{
			/* It is expected that 0 < amount < 1.
			 * If amount < 0, return value1.
			 * If amount > 1, return value2.
			 */
			var result = Clamp(amount, 0f, 1f);
			result = Hermite(value1, 0f, value2, 0f, result);

			return result;
		}

		public static float ToDegrees(float radians) => (float)(radians * 57.295779513082320876798154814105);

		public static float ToRadians(float degrees) => (float)(degrees * 0.017453292519943295769236907684886);

		public static float WrapAngle(float angle)
		{
			if (angle > -Pi && angle <= Pi)
			{
				return angle;
			}

			angle %= TwoPi;
			if (angle <= -Pi)
			{
				return angle + TwoPi;
			}

			if (angle > Pi)
			{
				return angle - TwoPi;
			}

			return angle;
		}

		// FIXME: This could be an extension! ClampIntEXT? -flibit
		/// <summary>
		///     Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">
		///     The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c>
		///     will be returned.
		/// </param>
		/// <param name="max">
		///     The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c>
		///     will be returned.
		/// </param>
		/// <returns>The clamped value.</returns>
		internal static int Clamp(int value, int min, int max)
		{
			value = value > max ? max : value;
			value = value < min ? min : value;
			return value;
		}

		internal static bool WithinEpsilon(float floatA, float floatB) => Math.Abs(floatA - floatB) < MachineEpsilonFloat;

		internal static int ClosestMSAAPower(int value)
		{
			/* Checking for the highest power of two _after_ than the given int:
			 * http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
			 * Take result, divide by 2, get the highest power of two _before_!
			 * -flibit
			 */
			if (value == 1)
			{
				// ... Except for 1, which is invalid for MSAA -flibit
				return 0;
			}

			var result = value - 1;
			result |= result >> 1;
			result |= result >> 2;
			result |= result >> 4;
			result |= result >> 8;
			result |= result >> 16;
			result += 1;
			if (result == value)
			{
				return result;
			}

			return result >> 1;
		}
	}
}
