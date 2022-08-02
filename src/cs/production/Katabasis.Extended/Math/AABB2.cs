// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis.Extended
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
    [PublicAPI]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Acronym.")]
    public struct AABB2 : IEquatable<AABB2>
    {
        /// <summary>
        ///     The <see cref="AABB2" /> with <see cref="Position" /> set to <see cref="Vector2.Zero"/> and
        ///     <see cref="HalfSize" /> set to <see cref="Vector2.Zero"/>.
        /// </summary>
        public static readonly AABB2 Empty;

        /// <summary>
        ///     The centre position of this <see cref="AABB2" />.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        ///     The distance from the <see cref="Position" /> point along both axes to any point on the boundary of this
        ///     <see cref="AABB2" />.
        /// </summary>
        public Vector2 HalfSize;

        /// <summary>
        ///     The width and height of this <see cref="AABB2" />.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public Vector2 Size => HalfSize * 2;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AABB2" /> structure from the specified center position and
        ///     size.
        /// </summary>
        /// <param name="position">The center position.</param>
        /// <param name="size">The width and height.</param>
        [ExcludeFromCodeCoverage]
        public AABB2(Vector2 position, Vector2 size)
        {
            Position = position;
            HalfSize = size * 0.5f;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AABB2" /> structure from the specified center position and
        ///     size.
        /// </summary>
        /// <param name="x">The center position X-coordinate.</param>
        /// <param name="y">The center position Y-coordinate.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        [ExcludeFromCodeCoverage]
        public AABB2(float x, float y, float width, float height)
        {
            Position = new Vector2(x, y);
            HalfSize = new Vector2(width * 0.5f, height * 0.5f);
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> from a minimum point and maximum point.
        /// </summary>
        /// <param name="minimum">The minimum point.</param>
        /// <param name="maximum">The maximum point.</param>
        /// <param name="result">The resulting bounding rectangle.</param>
        public static void CreateFrom(Vector2 minimum, Vector2 maximum, out AABB2 result)
        {
            result.Position = new Vector2((maximum.X + minimum.X) * 0.5f, (maximum.Y + minimum.Y) * 0.5f);
            result.HalfSize = new Vector2((maximum.X - minimum.X) * 0.5f, (maximum.Y - minimum.Y) * 0.5f);
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> from a minimum point and maximum point.
        /// </summary>
        /// <param name="minimum">The minimum point.</param>
        /// <param name="maximum">The maximum point.</param>
        /// <returns>The resulting <see cref="AABB2" />.</returns>
        [ExcludeFromCodeCoverage]
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
            var firstMinimum = first.Position - first.HalfSize;
            var firstMaximum = first.Position + first.HalfSize;
            var secondMinimum = second.Position - second.HalfSize;
            var secondMaximum = second.Position + second.HalfSize;

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
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
        public AABB2 Union(AABB2 aabb)
        {
            return Union(this, aabb);
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that is in common between two <see cref="AABB2" /> structures.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <param name="result">The resulting axis-aligned-bounding-box that is in common between both
        ///     <paramref name="first" /> and <paramref name="second" />, if they intersect; otherwise,
        ///     <see cref="Empty"/>.</param>
        public static void Intersection(ref AABB2 first, ref AABB2 second, out AABB2 result)
        {
            var firstMinimum = first.Position - first.HalfSize;
            var firstMaximum = first.Position + first.HalfSize;
            var secondMinimum = second.Position - second.HalfSize;
            var secondMaximum = second.Position + second.HalfSize;

            var minimum = Vector2.Max(firstMinimum, secondMinimum);
            var maximum = Vector2.Min(firstMaximum, secondMaximum);

            result = maximum.X < minimum.X || maximum.Y < minimum.Y ? default : CreateFrom(minimum, maximum);
        }

        /// <summary>
        ///     Computes the <see cref="AABB2" /> that is in common between two <see cref="AABB2" /> structures.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     A <see cref="AABB2" /> that is in common between both <paramref name="first" /> and
        ///     <paramref name="second" />, if they intersect; otherwise, <see cref="Empty"/>.
        /// </returns>
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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
            if (Math.Abs(first.Position.X - second.Position.X) > first.HalfSize.X + second.HalfSize.X)
            {
                return false;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Math.Abs(first.Position.Y - second.Position.Y) > first.HalfSize.Y + second.HalfSize.Y)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Determines whether the two specified <see cref="AABB2" /> structures intersect.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="first" /> intersects with the <see cref="second" />; otherwise, <c>false</c>.
        /// </returns>
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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
        ///     <see cref="Position" /> and <see cref="HalfSize" /> fields of the two <see cref="AABB2" /> structures
        ///     are equal.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <see cref="Position" /> and <see cref="HalfSize" /> fields of the two
        ///     <see cref="AABB2" /> structures are equal; otherwise, <c>false</c>.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool operator ==(AABB2 first, AABB2 second)
        {
            return first.Equals(ref second);
        }

        /// <summary>
        ///     Compares two <see cref="AABB2" /> structures. The result specifies whether the values of the
        ///     <see cref="Position" /> and <see cref="HalfSize" /> fields of the two <see cref="AABB2" /> structures
        ///     are unequal.
        /// </summary>
        /// <param name="first">The first axis-aligned-bounding-box.</param>
        /// <param name="second">The second axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if the <see cref="Position" /> and <see cref="HalfSize" /> fields of the two
        ///     <see cref="AABB2" /> structures are unequal; otherwise, <c>false</c>.
        /// </returns>
        [ExcludeFromCodeCoverage]
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
        ///     otherwise,
        ///     <c>false</c>.
        /// </returns>
        public bool Equals(ref AABB2 aabb)
        {
            return aabb.Position == Position && aabb.HalfSize == HalfSize;
        }

        /// <summary>
        ///     Indicates whether this <see cref="AABB2" /> is equal to another <see cref="AABB2" />.
        /// </summary>
        /// <param name="aabb">The axis-aligned-bounding-box.</param>
        /// <returns>
        ///     <c>true</c> if this <see cref="AABB2" /> is equal to the <paramref name="aabb" />;
        ///     otherwise, <c>false</c>.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public bool Equals(AABB2 aabb)
        {
            return Equals(ref aabb);
        }

        /// <summary>
        ///     Returns a value indicating whether this <see cref="AABB2" /> is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to make the comparison with.</param>
        /// <returns>
        ///     <c>true</c> if this  <see cref="AABB2" /> is equal to <paramref name="obj" />; otherwise, <c>false</c>.
        /// </returns>
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable desired.")]
        public override int GetHashCode()
        {
            return HashCode.Combine(Position, HalfSize);
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
        ///     Performs an explicit conversion from a <see cref="AABB2" /> to a <see cref="Rectangle" />. Values are
        ///     truncated from <see cref="float" /> to <see cref="int" />.
        /// </summary>
        /// <param name="aabb">The bounding rectangle.</param>
        /// <returns>
        ///     The resulting <see cref="Rectangle" />.
        /// </returns>
        public static explicit operator Rectangle(AABB2 aabb)
        {
            var minimum = aabb.Position - aabb.HalfSize;
            return new Rectangle((int)minimum.X, (int)minimum.Y, (int)aabb.HalfSize.X * 2, (int)aabb.HalfSize.Y * 2);
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this <see cref="AABB2" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this <see cref="AABB2" />.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"Position: ({Position.X},{Position.Y}), Size: {Size}";
        }

        [ExcludeFromCodeCoverage]
        internal string DebugDisplayString => ToString();
    }
}
