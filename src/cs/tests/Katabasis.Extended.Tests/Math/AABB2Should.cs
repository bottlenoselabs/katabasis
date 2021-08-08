// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.

using System.Numerics;
using Xunit;

namespace Katabasis.Extended.Tests.Math
{
    public class AABB2Should
    {
        [Fact]
        public void Intersects()
        {
            AABB2 first;
            first.Center = new Vector2(50, 50);
            first.HalfSize = new Vector2(25, 25);

            AABB2 second;
            second.Center = new Vector2(50, 50);
            second.HalfSize = new Vector2(25, 25);

            var x = first.Intersects(second);
            Assert.True(x);
        }
    }
}
