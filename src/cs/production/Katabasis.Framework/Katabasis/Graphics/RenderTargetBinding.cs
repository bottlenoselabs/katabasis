// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;

namespace Katabasis
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
