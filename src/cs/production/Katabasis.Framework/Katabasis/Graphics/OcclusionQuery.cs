// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class OcclusionQuery : GraphicsResource
	{
		private readonly IntPtr _query;

		public OcclusionQuery(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice;
			_query = FNA3D.FNA3D_CreateQuery(GraphicsDevice.GLDevice);
		}

		public bool IsComplete =>
			FNA3D.FNA3D_QueryComplete(GraphicsDevice.GLDevice, _query) == 1;

		public int PixelCount =>
			FNA3D.FNA3D_QueryPixelCount(GraphicsDevice.GLDevice, _query);

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				FNA3D.FNA3D_AddDisposeQuery(GraphicsDevice.GLDevice, _query);
			}

			base.Dispose(disposing);
		}

		public void Begin() => FNA3D.FNA3D_QueryBegin(GraphicsDevice.GLDevice, _query);

		public void End() => FNA3D.FNA3D_QueryEnd(GraphicsDevice.GLDevice, _query);
	}
}
