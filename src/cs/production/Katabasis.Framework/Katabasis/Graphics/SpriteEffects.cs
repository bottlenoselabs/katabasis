// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[Flags]
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public enum SpriteEffects
	{
		None = 0,
		FlipHorizontally = 1,
		FlipVertically = 2
	}
}
