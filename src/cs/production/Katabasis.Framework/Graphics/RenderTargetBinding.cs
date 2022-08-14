// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;

namespace bottlenoselabs.Katabasis
{
	// RenderTargetBinding structure: http://msdn.microsoft.com/en-us/library/ff434403.aspx
	public readonly struct RenderTargetBinding
	{
		public Texture? RenderTarget { get; }

		public CubeMapFace CubeMapFace { get; }

		// ReSharper disable once SuggestBaseTypeForParameter
		public RenderTargetBinding(RenderTarget2D renderTarget)
		{
			RenderTarget = renderTarget;
			CubeMapFace = CubeMapFace.PositiveX;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		public RenderTargetBinding(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
		{
			if (cubeMapFace < CubeMapFace.PositiveX || cubeMapFace > CubeMapFace.NegativeZ)
			{
				throw new ArgumentOutOfRangeException(nameof(cubeMapFace));
			}

			RenderTarget = renderTarget;
			CubeMapFace = cubeMapFace;
		}

		public static implicit operator RenderTargetBinding(RenderTarget2D renderTarget) => new(renderTarget);
	}
}
