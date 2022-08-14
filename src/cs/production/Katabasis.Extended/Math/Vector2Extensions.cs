// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace bottlenoselabs.Katabasis.Extended
{
    public static class Vector2Extensions
    {
        /// <summary>
        ///     Calculates the dot product of two vectors. If the two vectors are unit vectors, the dot product returns
        ///     a floating point value between -1 and 1 that can be used to determine some properties of the angle
        ///     between two vectors. For example, it can show whether the vectors are orthogonal, parallel, or have an
        ///     acute or obtuse angle between them.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The dot product of the two vectors.</returns>
        /// <remarks>
        ///     <para>The dot product is also known as the inner product.</para>
        ///     <para>
        ///         For any two vectors, the dot product is defined as: <c>(vector1.X * vector2.X) + (vector1.Y * vector2.Y).</c>
        ///         The result of this calculation, plus or minus some margin to account for floating point error, is equal to:
        ///         <c>Length(vector1) * Length(vector2) * System.Math.Cos(theta)</c>, where <c>theta</c> is the angle between the
        ///         two vectors.
        ///     </para>
        ///     <para>
        ///         If <paramref name="vector1" /> and <paramref name="vector2" /> are unit vectors, the length of each
        ///         vector will be equal to 1. So, when <paramref name="vector1" /> and <paramref name="vector2" /> are unit
        ///         vectors, the dot product is simply equal to the cosine of the angle between the two vectors. For example, both
        ///         <c>cos</c> values in the following calcuations would be equal in value:
        ///         <c>vector1.Normalize(); vector2.Normalize(); var cos = vector1.Dot(vector2)</c>,
        ///         <c>var cos = System.Math.Cos(theta)</c>, where <c>theta</c> is angle in radians between the two vectors.
        ///     </para>
        ///     <para>
        ///         If <paramref name="vector1" /> and <paramref name="vector2" /> are unit vectors, without knowing the
        ///         value of <c>theta</c> or using a potentially processor-intensive trigonometric function, the value
        ///         of the dot product can tell us the following things:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     If <c>vector1.Dot(vector2) &gt; 0</c>, the angle between the two vectors
        ///                     is less than 90 degrees.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <c>vector1.Dot(vector2) &lt; 0</c>, the angle between the two vectors
        ///                     is more than 90 degrees.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <c>vector1.Dot(vector2) == 0</c>, the angle between the two vectors
        ///                     is 90 degrees; that is, the vectors are orthogonal.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <c>vector1.Dot(vector2) == 1</c>, the angle between the two vectors
        ///                     is 0 degrees; that is, the vectors point in the same direction and are parallel.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <c>vector1.Dot(vector2) == -1</c>, the angle between the two vectors
        ///                     is 180 degrees; that is, the vectors point in opposite directions and are parallel.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <note type="caution">
        ///         Because of floating point error, two orthogonal vectors may not return a dot product that is exactly
        ///         zero. It might be zero plus some amount of floating point error. In your code, you will want to
        ///         determine what amount of error is acceptable in your calculation, and take that into account when
        ///         you do your comparisons.
        ///     </note>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(this Vector2 vector1, Vector2 vector2)
        {
            return Vector2.Dot(vector1, vector2);
        }

        /// <summary>
        ///     Calculates the scalar projection of one vector onto another. The scalar projection returns the length of
        ///     the orthogonal projection of the first vector onto a straight line parallel to the second vector, with a
        ///     negative value if the projection has an opposite direction with respect to the second vector.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The scalar projection of <paramref name="vector1" /> onto <paramref name="vector2" />.</returns>
        /// <remarks>
        ///     <para>
        ///         The scalar projection is also known as the scalar resolute of the first vector in the direction of
        ///         the second vector.
        ///     </para>
        ///     <para>
        ///         For any two vectors, the scalar projection is defined as:
        ///         <c>vector1.Dot(vector2) / Length(vector2)</c>. The result of this calculation, plus or minus some
        ///         margin to account for floating point error, is equal to:
        ///         <c>Length(vector1) * System.Math.Cos(theta)</c>, where <c>theta</c> is the angle in radians between
        ///         <paramref name="vector1" /> and <paramref name="vector2" />.
        ///     </para>
        ///     <para>
        ///         The value of the scalar projection can tell us the following things:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     If <c>vector1.ScalarProjectOnto(vector2) &gt;= 0</c>, the angle between
        ///                     <paramref name="vector1" /> and <paramref name="vector2" /> is between 0 degrees
        ///                     (exclusive) and 90 degrees (inclusive).
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <c>vector1.ScalarProjectOnto(vector2) &lt; 0</c>, the angle between
        ///                     <paramref name="vector1" /> and <paramref name="vector2" /> is between 90 degrees
        ///                     (exclusive) and 180 degrees (inclusive).
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ScalarProjectOnto(this Vector2 vector1, Vector2 vector2)
        {
            var dotNumerator = Vector2.Dot(vector1, vector2);
            var lengthSquaredDenominator = Vector2.Dot(vector2, vector2);
            return dotNumerator/(float) Math.Sqrt(lengthSquaredDenominator);
        }

        /// <summary>
        ///     Calculates the vector projection of one vector onto another. The vector projection returns the
        ///     orthogonal projection of the first vector onto a straight line parallel to the second vector.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The vector projection of <paramref name="vector1" /> onto <paramref name="vector2" />.</returns>
        /// <remarks>
        ///     <para>
        ///         The vector projection is also known as the vector component or vector resolute of the first vector
        ///         in the direction of the second vector.
        ///     </para>
        ///     <para>
        ///         For any two vectors, the vector projection is defined as:
        ///         <c>( vector1.Dot(vector2) / Length(vector2)^2 ) * vector2</c>.
        ///         The
        ///         result of this calculation, plus or minus some margin to account for floating point error, is equal
        ///         to: <c>( Length(vector1) * System.Math.Cos(theta) ) * vector2 / Length(vector2)</c>, where
        ///         <c>theta</c> is the angle in radians between <paramref name="vector1" /> and
        ///         <paramref name="vector2" />.
        ///     </para>
        ///     <para>
        ///         This function is easier to compute than <see cref="ScalarProjectOnto" /> since it does not use a
        ///         square root. When the vector projection and the scalar projection is required, consider using this
        ///         function; the scalar projection can be obtained by taking the length of the projection vector.
        ///     </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ProjectOnto(this Vector2 vector1, Vector2 vector2)
        {
            var dotNumerator = Vector2.Dot(vector1, vector2);
            var lengthSquaredDenominator = Vector2.Dot(vector2, vector2);
            return dotNumerator/lengthSquaredDenominator*vector2;
        }

        public static float Clamp(float value, float min, float max, float? rangeMin = null, float? rangeMax = null)
        {
            var rmin = rangeMin ?? -Math.PI;
            var rmax = rangeMax ?? Math.PI;

            var modulus = (float)Math.Abs(rmin - rmax);
            if ((value %= modulus) < 0f)
            {
                value += modulus;
            }

            value += (float)Math.Min(rmin, rmax);
            return Math.Clamp(value, min, max);
        }
    }
}
