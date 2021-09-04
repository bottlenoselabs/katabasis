// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

// using System;
// using System.Diagnostics.CodeAnalysis;
// using System.Numerics;
// using System.Runtime.CompilerServices;
// using JetBrains.Annotations;
//
// namespace Katabasis.Extended
// {
//     /// <summary>
//     ///     A two dimensional line segment defined by two points, a starting point and an ending point.
//     /// </summary>
//     /// <seealso cref="IEquatable{T}" />
//     [PublicAPI]
//     public struct Line2 : IEquatable<Line2>
//     {
//         /// <summary>
//         ///     The starting point of this <see cref="Line2" />.
//         /// </summary>
//         public Vector2 Start;
//
//         /// <summary>
//         ///     The ending point of this <see cref="Line2" />.
//         /// </summary>
//         public Vector2 End;
//
//         /// <summary>
//         ///     Initializes a new instance of the <see cref="Line2" /> structure from the specified starting and ending
//         ///     points.
//         /// </summary>
//         /// <param name="start">The starting point.</param>
//         /// <param name="end">The ending point.</param>
//         public Line2(Vector2 start, Vector2 end)
//         {
//             Start = start;
//             End = end;
//         }
//
//         /// <summary>
//         ///     Initializes a new instance of the <see cref="Line2" /> structure.
//         /// </summary>
//         /// <param name="x1">The starting x-coordinate.</param>
//         /// <param name="y1">The starting y-coordinate.</param>
//         /// <param name="x2">The ending x-coordinate.</param>
//         /// <param name="y2">The ending y-coordinate.</param>
//         public Line2(float x1, float y1, float x2, float y2)
//             : this(new Vector2(x1, y1), new Vector2(x2, y2))
//         {
//         }
//
//         /// <summary>
//         ///     Computes the closest point on this <see cref="Line2" /> to a specified point.
//         /// </summary>
//         /// <param name="point">The point.</param>
//         /// <returns>The closest point on this <see cref="Line2" /> to the <paramref name="point" />.</returns>
//         public Vector2 ClosestPointTo(Vector2 point)
//         {
//             // Computes the parameterized position: d(t) = Start + t * (End – Start)
//
//             var startToEnd = End - Start;
//             var startToPoint = point - Start;
//             // Project arbitrary point onto the line segment, deferring the division
//             var t = startToEnd.Dot(startToPoint);
//             // If outside segment, clamp t (and therefore d) to the closest endpoint
//             if (t <= 0)
// 			{
// 				return Start;
// 			}
//
// 			// Always nonnegative since denominator = (||vector||)^2
//             var denominator = startToEnd.Dot(startToEnd);
//             if (t >= denominator)
// 			{
// 				return End;
// 			}
//
// 			// The point projects inside the [Start, End] interval, must do deferred division now
//             t /= denominator;
//             startToEnd *= t;
//             return new Vector2(Start.X + startToEnd.X, Start.Y + startToEnd.Y);
//         }
//
//         /// <summary>
//         ///     Computes the squared distance from this <see cref="Line2" /> to a specified point.
//         /// </summary>
//         /// <param name="point">The point.</param>
//         /// <returns>The squared distance from this <see cref="Line2" /> to a specified point.</returns>
//         public float SquaredDistanceTo(Vector2 point)
//         {
//             var startToEnd = End - Start;
//             var startToPoint = point - Start;
//             var endToPoint = point - End;
//             // Handle cases where the point projects outside the line segment
//             var dot = startToPoint.Dot(startToEnd);
//             var startToPointDistanceSquared = startToPoint.Dot(startToPoint);
//             if (dot <= 0.0f)
// 			{
// 				return startToPointDistanceSquared;
// 			}
//
//             var startToEndDistanceSquared = startToEnd.Dot(startToEnd);
//             if (dot >= startToEndDistanceSquared)
// 			{
// 				endToPoint.Dot(endToPoint);
// 			}
//
// 			// Handle the case where the point projects onto the line segment
//             return startToPointDistanceSquared - (dot * dot / startToEndDistanceSquared);
//         }
//
//         /// <summary>
//         ///     Computes the distance from this <see cref="Line2" /> to a specified point.
//         /// </summary>
//         /// <param name="point">The point.</param>
//         /// <returns>The distance from this <see cref="Line2" /> to a specified point.</returns>
//         public float DistanceTo(Vector2 point)
//         {
//             return (float) Math.Sqrt(SquaredDistanceTo(point));
//         }
//
//         /// <summary>
//         ///     Determines whether this <see cref="Line2" /> intersects with the specified <see cref="AABB2" />.
//         /// </summary>
//         /// <param name="rectangle">The bounding box.</param>
//         /// <param name="intersectionPoint">
//         ///     When this method returns, contains the point of intersection, if an intersection was found; otherwise,
//         ///     <see cref="Vector2.Zero" />.
//         /// </param>
//         /// <returns>
//         ///     <c>true</c> if this <see cref="Line2" /> intersects with <paramref name="rectangle" />; otherwise,
//         ///     <c>false</c>.
//         /// </returns>
//         public bool Intersects(RectangleF rectangle, out Vector2 intersectionPoint)
//         {
//             // Real-Time Collision Detection, Christer Ericson, 2005. Chapter 5.3; Basic Primitive Tests - Intersecting Lines, Rays, and (Directed Segments). pg 179-181
//
//             var minimumPoint = rectangle.TopLeft;
//             var maximumPoint = rectangle.BottomRight;
//             var minimumDistance = float.MinValue;
//             var maximumDistance = float.MaxValue;
//
//             var direction = End - Start;
//             if (
//                 !MathUtilities.IntersectsSlab(Start.X, direction.X, minimumPoint.X, maximumPoint.X, ref minimumDistance,
//                     ref maximumDistance))
//             {
//                 intersectionPoint = default;
//                 return false;
//             }
//
//             if (
//                 !MathUtilities.IntersectsSlab(Start.Y, direction.Y, minimumPoint.Y, maximumPoint.Y, ref minimumDistance,
//                     ref maximumDistance))
//             {
//                 intersectionPoint = default;
//                 return false;
//             }
//
//             // Segment intersects the 2 slabs.
//
//             if (minimumDistance <= 0)
//             {
//                 intersectionPoint = Start;
//             }
//             else
//             {
//                 intersectionPoint = minimumDistance * direction;
//                 intersectionPoint.X += Start.X;
//                 intersectionPoint.Y += Start.Y;
//             }
//
//             return true;
//         }
//
//         /// <summary>
//         ///     Determines whether this <see cref="Line2" /> intersects with the specified <see cref="AABB2" />.
//         /// </summary>
//         /// <param name="aabb">The bounding box.</param>
//         /// <param name="intersectionPoint">
//         ///     When this method returns, contains the point of intersection, if an intersection was found; otherwise,
//         ///     <see cref="Vector2.Zero" />.
//         /// </param>
//         /// <returns>
//         ///     <c>true</c> if this <see cref="Line2" /> intersects with <paramref name="aabb" />; otherwise,
//         ///     <c>false</c>.
//         /// </returns>
//         public bool Intersects(AABB2 aabb, out Vector2 intersectionPoint)
//         {
//             // Real-Time Collision Detection, Christer Ericson, 2005. Chapter 5.3; Basic Primitive Tests - Intersecting Lines, Rays, and (Directed Segments). pg 179-181
//
//             var minimumPoint = aabb.Center - aabb.HalfSize;
//             var maximumPoint = aabb.Center + aabb.HalfSize;
//             var minimumDistance = float.MinValue;
//             var maximumDistance = float.MaxValue;
//
//             var direction = End - Start;
//             if (
//                 !MathUtilities.IntersectsSlab(Start.X, direction.X, minimumPoint.X, maximumPoint.X, ref minimumDistance,
//                     ref maximumDistance))
//             {
//                 intersectionPoint = default;
//                 return false;
//             }
//
//             if (
//                 !MathUtilities.IntersectsSlab(Start.Y, direction.Y, minimumPoint.Y, maximumPoint.Y, ref minimumDistance,
//                     ref maximumDistance))
//             {
//                 intersectionPoint = default;
//                 return false;
//             }
//
//             // Segment intersects the 2 slabs.
//
//             if (minimumDistance <= 0)
//                 intersectionPoint = Start;
//             else
//             {
//                 intersectionPoint = minimumDistance*direction;
//                 intersectionPoint.X += Start.X;
//                 intersectionPoint.Y += Start.Y;
//             }
//
//             return true;
//         }
//
//         /// <summary>
//         ///     Indicates whether this <see cref="Line2" /> is equal to another <see cref="Line2" />.
//         /// </summary>
//         /// <param name="other">The segment.</param>
//         /// <returns>
//         ///     <c>true</c> if this <see cref="Line2" /> is equal to the <paramref name="other" />; otherwise, <c>false</c>.
//         /// </returns>
//         public bool Equals(Line2 other)
//         {
//             return Equals(ref other);
//         }
//
//         /// <summary>
//         ///     Indicates whether this <see cref="Line2" /> is equal to another <see cref="Line2" />.
//         /// </summary>
//         /// <param name="line">The segment.</param>
//         /// <returns>
//         ///     <c>true</c> if this <see cref="Line2" /> is equal to the <paramref name="line" /> parameter; otherwise,
//         ///     <c>false</c>.
//         /// </returns>
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public bool Equals(ref Line2 line)
//         {
//             return (Start == line.Start) && (End == line.End);
//         }
//
//         /// <summary>
//         ///     Returns a value indicating whether this <see cref="Line2" /> is equal to a specified object.
//         /// </summary>
//         /// <param name="obj">The object to make the comparison with.</param>
//         /// <returns>
//         ///     <c>true</c> if this  <see cref="Line2" /> is equal to <paramref name="obj" />; otherwise, <c>false</c>.
//         /// </returns>
//         public override bool Equals(object? obj)
//         {
//             return obj is Line2 line && Equals(line);
//         }
//
//         /// <summary>
//         ///     Compares two <see cref="Line2" /> structures. The result specifies
//         ///     whether the values of the <see cref="Start" /> and <see cref="End" />
//         ///     fields of the two <see cref='Line2' />
//         ///     structures are equal.
//         /// </summary>
//         /// <param name="first">The first segment.</param>
//         /// <param name="second">The second segment.</param>
//         /// <returns>
//         ///     <c>true</c> if the <see cref="Start" /> and <see cref="End" />
//         ///     fields of the two <see cref="Line2" />
//         ///     structures are equal; otherwise, <c>false</c>.
//         /// </returns>
//         public static bool operator ==(Line2 first, Line2 second)
//         {
//             return first.Equals(ref second);
//         }
//
//         /// <summary>
//         ///     Compares two <see cref="Line2" /> structures. The result specifies
//         ///     whether the values of the <see cref="Start" /> and <see cref="End" />
//         ///     fields of the two <see cref="Line2" />
//         ///     structures are unequal.
//         /// </summary>
//         /// <param name="first">The first point.</param>
//         /// <param name="second">The second point.</param>
//         /// <returns>
//         ///     <c>true</c> if the <see cref="Start" /> and <see cref="End" />
//         ///     fields of the two <see cref="Line2" />
//         ///     structures are unequal; otherwise, <c>false</c>.
//         /// </returns>
//         public static bool operator !=(Line2 first, Line2 second)
//         {
//             return !(first == second);
//         }
//
//         /// <summary>
//         ///     Returns a hash code of this <see cref="Line2" /> suitable for use in hashing algorithms and data
//         ///     structures like a hash table.
//         /// </summary>
//         /// <returns>
//         ///     A hash code of this <see cref="Line2" />.
//         /// </returns>
//         [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable desired.")]
//         public override int GetHashCode()
//         {
//             unchecked
//             {
//                 return (Start.GetHashCode()*397) ^ End.GetHashCode();
//             }
//         }
//
//         /// <summary>
//         ///     Returns a <see cref="string" /> that represents this <see cref="Line2" />.
//         /// </summary>
//         /// <returns>
//         ///     A <see cref="string" /> that represents this <see cref="Line2" />.
//         /// </returns>
//         public override string ToString()
//         {
//             return $"{Start} -> {End}";
//         }
//
//         internal string DebugDisplayString => ToString();
//     }
// }
