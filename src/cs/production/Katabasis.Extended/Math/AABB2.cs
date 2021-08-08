// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;

namespace Katabasis.Extended
{
    /// <summary>
    ///     An axis-aligned, four sided, two dimensional box defined by a center point along with half-width and
    ///     half-height; the values are type of <see cref="float" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An <see cref="AABB2" /> is categorized by having its faces oriented in such a way that its
    ///         face normals are at all times parallel with the axes of the given coordinate system. I.e., no rotation.
    ///     </para>
    ///     <para>
    ///         The <see cref="AABB2" /> of a rotated rectangle will be equivalent or larger in size than the
    ///         original rectangle depending on the angle of rotation.
    ///     </para>
    /// </remarks>
    /// <seealso cref="IEquatable{T}" />
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Acronym.")]
    public struct AABB2 : IEquatable<AABB2>
    {
        /// <summary>
        ///     The <see cref="AABB2" /> with <see cref="Center" /> set to <see cref="Vector2.Zero"/> and
        ///     <see cref="HalfSize" /> set to <see cref="Vector2.Zero"/>.
        /// </summary>
        public static readonly AABB2 Empty;

        /// <summary>
        ///     The centre position of this <see cref="AABB2" />.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        ///     The distance from the <see cref="Center" /> point along both axes to any point on the boundary of the
        ///     <see cref="AABB2" />.
        /// </summary>
        public Vector2 HalfSize;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AABB2" /> structure from the specified center and half-size.
        /// </summary>
        /// <param name="center">The center <see cref="Vector2" />.</param>
        /// <param name="halfSize">The radii <see cref="Vector2" />.</param>
        public AABB2(Vector2 center, Vector2 halfSize)
        {
            Center = center;
            HalfSize = halfSize;
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> from a minimum point and maximum point.
        /// </summary>
        /// <param name="minimum">The minimum point.</param>
        /// <param name="maximum">The maximum point.</param>
        /// <param name="result">The resulting bounding rectangle.</param>
        public static void CreateFrom(Vector2 minimum, Vector2 maximum, out AABB2 result)
        {
            result.Center = new Vector2((maximum.X + minimum.X) * 0.5f, (maximum.Y + minimum.Y) * 0.5f);
            result.HalfSize = new Vector2((maximum.X - minimum.X) * 0.5f, (maximum.Y - minimum.Y) * 0.5f);
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> from a minimum point and maximum point.
        /// </summary>
        /// <param name="minimum">The minimum point.</param>
        /// <param name="maximum">The maximum point.</param>
        /// <returns>The resulting <see cref="AABB2" />.</returns>
        public static AABB2 CreateFrom(Vector2 minimum, Vector2 maximum)
        {
            CreateFrom(minimum, maximum, out var result);
            return result;
        }

        // /// <summary>
        // ///     Computes the <see cref="AABB2" /> from a list of points.
        // /// </summary>
        // /// <param name="points">The points.</param>
        // /// <param name="result">The resulting bounding rectangle.</param>
        // public static void CreateFrom(IReadOnlyList<Vector2> points, out AABB2 result)
        // {
        //     MathUtilities.CreateRectangleFromPoints(points, out var minimum, out var maximum);
        //     CreateFrom(minimum, maximum, out result);
        // }

        // /// <summary>
        // ///     Computes the <see cref="AABB2" /> from a list of points.
        // /// </summary>
        // /// <param name="points">The points.</param>
        // /// <returns>The resulting <see cref="AABB2" />.</returns>
        // public static AABB2 CreateFrom(IReadOnlyList<Vector2> points)
        // {
        //     CreateFrom(points, out var result);
        //     return result;
        // }

        // /// <summary>
        // ///     Computes the <see cref="AABB2" /> from the specified <see cref="AABB2" /> transformed by
        // ///     the
        // ///     specified <see cref="Matrix3x2" />.
        // /// </summary>
        // /// <param name="aabb">The bounding rectangle.</param>
        // /// <param name="transformMatrix">The transform matrix.</param>
        // /// <param name="result">The resulting bounding rectangle.</param>
        // /// <remarks>
        // ///     <para>
        // ///         If a transformed <see cref="AABB2" /> is used for <paramref name="aabb" /> then the
        // ///         resulting <see cref="AABB2" /> will have the compounded transformation, which most likely is
        // ///         not desired.
        // ///     </para>
        // /// </remarks>
        // public static void Transform(ref AABB2 aabb, ref Matrix3x2 transformMatrix, out AABB2 result)
        // {
        //     MathUtilities.TransformRectangle(ref aabb.Center, ref aabb.HalfSize, ref transformMatrix);
        //     result.Center = aabb.Center;
        //     result.HalfSize = aabb.HalfSize;
        // }

        // /// <summary>
        // ///     Computes the <see cref="AABB2" /> from the specified <see cref="AABB2" /> transformed by
        // ///     the
        // ///     specified <see cref="Matrix3x2" />.
        // /// </summary>
        // /// <param name="aabb">The bounding rectangle.</param>
        // /// <param name="transformMatrix">The transform matrix.</param>
        // /// <returns>
        // ///     The <see cref="AABB2" /> from the <paramref name="aabb" /> transformed by the
        // ///     <paramref name="transformMatrix" />.
        // /// </returns>
        // /// <remarks>
        // ///     <para>
        // ///         If a transformed <see cref="AABB2" /> is used for <paramref name="aabb" /> then the
        // ///         resulting <see cref="AABB2" /> will have the compounded transformation, which most likely is
        // ///         not desired.
        // ///     </para>
        // /// </remarks>
        // public static AABB2 Transform(AABB2 aabb, ref Matrix3x2 transformMatrix)
        // {
        //     Transform(ref aabb, ref transformMatrix, out var result);
        //     return result;
        // }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that contains the two specified <see cref="AABB2" /> structures.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <param name="result">The resulting axis-aligned-bounding-box that contains both <paramref name="first" />
        ///     and <paramref name="second" />.</param>
        public static void Union(ref AABB2 first, ref AABB2 second, out AABB2 result)
        {
            var firstMinimum = first.Center - first.HalfSize;
            var firstMaximum = first.Center + first.HalfSize;
            var secondMinimum = second.Center - second.HalfSize;
            var secondMaximum = second.Center + second.HalfSize;

            var minimum = Vector2.Min(firstMinimum, secondMinimum);
            var maximum = Vector2.Max(firstMaximum, secondMaximum);

            result = CreateFrom(minimum, maximum);
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that contains the two specified <see cref="AABB2" /> structures.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     A <see cref="AABB2" /> that contains both <paramref name="first" /> and <paramref name="second" />.
        /// </returns>
        public static AABB2 Union(AABB2 first, AABB2 second)
        {
            Union(ref first, ref second, out var result);
            return result;
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that contains both the specified <see cref="AABB2" /> and this
        ///     <see cref="AABB2" />.
        /// </summary>
        /// <param name="aabb">The bounding rectangle.</param>
        /// <returns>
        ///     A <see cref="AABB2" /> that contains both the <paramref name="aabb" /> and this <see cref="AABB2" />.
        /// </returns>
        public AABB2 Union(AABB2 aabb)
        {
            return Union(this, aabb);
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that is in common between the two specified <see cref="AABB2" /> structures.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <param name="result">The resulting axis-aligned-bounding-box that is in common between both
        ///     <paramref name="first" /> and <paramref name="second" />, if they intersect; otherwise,
        ///     <see cref="Empty"/>.</param>
        public static void Intersection(ref AABB2 first, ref AABB2 second, out AABB2 result)
        {
            var firstMinimum = first.Center - first.HalfSize;
            var firstMaximum = first.Center + first.HalfSize;
            var secondMinimum = second.Center - second.HalfSize;
            var secondMaximum = second.Center + second.HalfSize;

            var minimum = Vector2.Max(firstMinimum, secondMinimum);
            var maximum = Vector2.Min(firstMaximum, secondMaximum);

            result = maximum.X < minimum.X || maximum.Y < minimum.Y ? default : CreateFrom(minimum, maximum);
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that is in common between the two specified <see cref="AABB2" /> structures.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     A <see cref="AABB2" /> that is in common between both <paramref name="first" /> and
        ///     <paramref name="second" />, if they intersect; otherwise, <see cref="Empty"/>.
        /// </returns>
        public static AABB2 Intersection(AABB2 first, AABB2 second)
        {
            Intersection(ref first, ref second, out var result);
            return result;
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that is in common between the specified <see cref="AABB2" /> and this <see cref="AABB2" />.
        /// </summary>
        /// <param name="aabb">The bounding rectangle.</param>
        /// <returns>
        ///     A <see cref="AABB2" /> that is in common between both the <paramref name="aabb" /> and
        ///     this <see cref="AABB2"/>, if they intersect; otherwise, <see cref="Empty"/>.
        /// </returns>
        public AABB2 Intersection(AABB2 aabb)
        {
            Intersection(ref this, ref aabb, out var result);
            return result;
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that is in common between the specified <see cref="AABB2" /> and this <see cref="AABB2" />.
        /// </summary>
        /// <param name="aabb">The bounding rectangle.</param>
        /// <returns>
        ///     A <see cref="AABB2" /> that is in common between both the <paramref name="aabb" /> and
        ///     this <see cref="AABB2"/>, if they intersect; otherwise, <see cref="Empty"/>.
        /// </returns>
        public AABB2 Intersection(ref AABB2 aabb)
        {
            Intersection(ref this, ref aabb, out var result);
            return result;
        }

        /// <summary>
        ///     Determines whether the two specified <see cref="AABB2" /> structures intersect.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="first" /> intersects with the <see cref="second" />; otherwise, <c>false</c>.
        /// </returns>
        public static bool Intersects(ref AABB2 first, ref AABB2 second)
        {
            var position = first.Center - second.Center;
            var size = first.HalfSize + second.HalfSize;
            return Math.Abs(position.X) <= size.X && Math.Abs(position.Y) <= size.Y;
        }

        /// <summary>
        ///     Determines whether the two specified <see cref="AABB2" /> structures intersect.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="first" /> intersects with the <see cref="second" />; otherwise, <c>false</c>.
        /// </returns>
        public static bool Intersects(AABB2 first, AABB2 second)
        {
            return Intersects(ref first, ref second);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="AABB2" /> intersects with this <see cref="AABB2" />.
        /// </summary>
        /// <param name="aabb">The axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="aabb" /> intersects with this <see cref="AABB2" />; otherwise,
        ///     <c>false</c>.
        /// </returns>
        public bool Intersects(ref AABB2 aabb)
        {
            return Intersects(ref this, ref aabb);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="AABB2" /> intersects with this
        ///     <see cref="AABB2" />.
        /// </summary>
        /// <param name="aabb">The axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="aabb" /> intersects with this <see cref="AABB2" />; otherwise,
        ///     <c>false</c>.
        /// </returns>
        public bool Intersects(AABB2 aabb)
        {
            return Intersects(ref this, ref aabb);
        }

        // /// <summary>
        // ///     Updates this <see cref="AABB2" /> from a list of points.
        // /// </summary>
        // /// <param name="points">The points.</param>
        // public void UpdateFromPoints(IReadOnlyList<Vector2> points)
        // {
        //     var aabb = CreateFrom(points);
        //     Center = aabb.Center;
        //     HalfSize = aabb.HalfSize;
        // }

        // /// <summary>
        // ///     Determines whether the specified <see cref="AABB2" /> contains the specified point.
        // /// </summary>
        // /// <param name="aabb">The axis-aligned-bounding-box.</param>
        // /// <param name="point">The point.</param>
        // /// <returns>
        // ///     <c>true</c> if the <paramref name="aabb" /> contains the <paramref name="point" />; otherwise,
        // ///     <c>false</c>.
        // /// </returns>
        // public static bool Contains(ref AABB2 aabb, ref Vector2 point)
        // {
        //     // Real-Time Collision Detection, Christer Ericson, 2005. Chapter 4.2; Bounding Volumes - Axis-aligned Bounding Boxes (AABBs). pg 78
        //
        //     var position = aabb.Center - point;
        //     var halfSize = aabb.HalfSize;
        //
        //     return Math.Abs(x) <= halfWidth && Math.Abs(y) <= halfHeight;
        // }

        // /// <summary>
        // ///     Determines whether the specified <see cref="AABB2" /> contains the specified point.
        // /// </summary>
        // /// <param name="aabb">The bounding rectangle.</param>
        // /// <param name="point">The point.</param>
        // /// <returns>
        // ///     <c>true</c> if the <paramref name="aabb" /> contains the <paramref name="point" />; otherwise,
        // ///     <c>false</c>.
        // /// </returns>
        // public static bool Contains(AABB2 aabb, Vector2 point)
        // {
        //     return Contains(ref aabb, ref point);
        // }
        //
        // /// <summary>
        // ///     Determines whether this <see cref="AABB2" /> contains the specified point.
        // /// </summary>
        // /// <param name="point">The point.</param>
        // /// <returns>
        // ///     <c>true</c> if this <see cref="AABB2" /> contains the <paramref name="point" />; otherwise,
        // ///     <c>false</c>.
        // /// </returns>
        // public bool Contains(Vector2 point)
        // {
        //     return Contains(this, point);
        // }

        // /// <summary>
        // ///     Computes the squared distance from this <see cref="AABB2"/> to a point.
        // /// </summary>
        // /// <param name="point">The point.</param>
        // /// <returns>The squared distance from this <see cref="AABB2"/> to the <paramref name="point"/>.</returns>
        // public float SquaredDistanceTo(Vector2 point)
        // {
        //     return MathUtilities.SquaredDistanceToPointFromRectangle(Center - HalfSize, Center + HalfSize, point);
        // }
        //
        // /// <summary>
        // ///     Computes the closest point on this <see cref="AABB2" /> to a specified point.
        // /// </summary>
        // /// <param name="point">The point.</param>
        // /// <returns>The closest point on this <see cref="AABB2" /> to the <paramref name="point" />.</returns>
        // public Vector2 ClosestPointTo(Vector2 point)
        // {
        //     MathUtilities.ClosestPointToPointFromRectangle(Center - HalfSize, Center + HalfSize, point, out var result);
        //     return result;
        // }

        /// <summary>
        ///     Compares two <see cref="AABB2" /> structures. The result specifies whether the values of the
        ///     <see cref="Center" /> and <see cref="HalfSize" /> fields of the two <see cref="AABB2" /> structures
        ///     are equal.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <see cref="Center" /> and <see cref="HalfSize" /> fields of the two
        ///     <see cref="AABB2" /> structures are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(AABB2 first, AABB2 second)
        {
            return first.Equals(ref second);
        }

        /// <summary>
        ///     Compares two <see cref="AABB2" /> structures. The result specifies whether the values of the
        ///     <see cref="Center" /> and <see cref="HalfSize" /> fields of the two <see cref="AABB2" /> structures
        ///     are unequal.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <see cref="Center" /> and <see cref="HalfSize" /> fields of the two
        ///     <see cref="AABB2" /> structures are unequal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(AABB2 first, AABB2 second)
        {
            return !(first == second);
        }

        /// <summary>
        ///     Indicates whether this <see cref="AABB2" /> is equal to another <see cref="AABB2" />.
        /// </summary>
        /// <param name="aabb">The axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if this <see cref="AABB2" /> is equal to the <paramref name="aabb" />;
        ///     otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(AABB2 aabb)
        {
            return Equals(ref aabb);
        }

        /// <summary>
        ///     Indicates whether this <see cref="AABB2" /> is equal to another <see cref="AABB2" />.
        /// </summary>
        /// <param name="aabb">The axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if this <see cref="AABB2" /> is equal to the <paramref name="aabb" />;
        ///     otherwise,
        ///     <c>false</c>.
        /// </returns>
        public bool Equals(ref AABB2 aabb)
        {
            return (aabb.Center == Center) && (aabb.HalfSize == HalfSize);
        }

        /// <summary>
        ///     Returns a value indicating whether this <see cref="AABB2" /> is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to make the comparison with.</param>
        /// <returns>
        ///     <c>true</c> if this  <see cref="AABB2" /> is equal to <paramref name="obj" />; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj is AABB2 aabb && Equals(aabb);
        }

        /// <summary>
        ///     Returns a hash code of this <see cref="AABB2" /> suitable for use in hashing algorithms and data
        ///     structures like a hashtable.
        /// </summary>
        /// <returns>
        ///     A hash code of this <see cref="AABB2" />.
        /// </returns>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable desired.")]
        public override int GetHashCode()
        {
            return HashCode.Combine(Center, HalfSize);
        }

        /// <summary>
        ///     Performs an implicit conversion from a <see cref="Rectangle" /> to a <see cref="AABB2" />.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns>
        ///     The resulting <see cref="AABB2" />.
        /// </returns>
        public static implicit operator AABB2(Rectangle rectangle)
        {
            var halfSize = new Vector2(rectangle.Width * 0.5f, rectangle.Height * 0.5f);
            var centre = new Vector2(rectangle.X + halfSize.X, rectangle.Y + halfSize.Y);
            return new AABB2(centre, halfSize);
        }

        /// <summary>
        ///     Performs an explicit conversion (explicit ) from a <see cref="AABB2" /> to a <see cref="Rectangle" />.
        /// </summary>
        /// <param name="aabb">The bounding rectangle.</param>
        /// <returns>
        ///     The resulting <see cref="Rectangle" />.
        /// </returns>
        public static explicit operator Rectangle(AABB2 aabb)
        {
            var minimum = aabb.Center - aabb.HalfSize;
            return new Rectangle((int)minimum.X, (int)minimum.Y, (int)aabb.HalfSize.X * 2, (int)aabb.HalfSize.Y * 2);
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this <see cref="AABB2" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this <see cref="AABB2" />.
        /// </returns>
        public override string ToString()
        {
            return $"Centre: {Center}, Radii: {HalfSize}";
        }

        internal string DebugDisplayString => ToString();
    }
}
