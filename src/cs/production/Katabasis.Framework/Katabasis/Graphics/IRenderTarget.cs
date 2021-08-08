// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;

namespace Katabasis
{
	internal interface IRenderTarget
	{
		int Width { get; }

		int Height { get; }

		int LevelCount { get; }

		RenderTargetUsage RenderTargetUsage { get; }

		DepthFormat DepthStencilFormat { get; }

		IntPtr DepthStencilBuffer { get; }

		IntPtr ColorBuffer { get; }

		int MultiSampleCount { get; }
	}
}
