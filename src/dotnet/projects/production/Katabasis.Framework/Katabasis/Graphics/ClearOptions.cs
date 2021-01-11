// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;

namespace Katabasis
{
	[Flags]
	public enum ClearOptions
	{
		Target = 1,
		DepthBuffer = 2,
		Stencil = 4
	}
}
