// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using Xunit;

namespace bottlenoselabs.Katabasis.Extended.Tests.Math
{
    public class AABB2Should : MathTestBase
    {
        public static TheoryData<bool, AABB2, AABB2> IntersectTestData => new()
        {
            // always intersects with itself when width/height is non-negative
            { true, AABB2.Empty, AABB2.Empty },
            { true, new AABB2(0, 0, 50, 50), new AABB2(0, 0, 50, 50) },
            { true, new AABB2(25, 25, 25, 25), new AABB2(25, 25, 25, 25) },
            { true, new AABB2(-25, -25, 25, 25), new AABB2(-25, -25, 25, 25) },

            // overlaps
            { true, new AABB2(25, 25, 25, 25), new AABB2(0, 0, 50, 50) },
            { true, new AABB2(-25, -25, 25, 25), new AABB2(0, 0, 50, 50) },

            // contains
            { true, new AABB2(0, 0, 0, 0), new AABB2(0, 0, 1, 1) },

            // never intersect with negative size
            { false, new AABB2(0, 0, -50, -50), new AABB2(0, 0, -50, -50) },

            // disjoint
            { false, new AABB2(-100, -100, 25, 25), new AABB2(0, 0, 50, 50) },
        };

        [Theory]
        [MemberData(nameof(IntersectTestData), MemberType = typeof(AABB2Should))]
        public void Intersect(bool expectedValue, AABB2 first, AABB2 second)
        {
            var actualValue = AABB2.Intersects(ref first, ref second);
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
