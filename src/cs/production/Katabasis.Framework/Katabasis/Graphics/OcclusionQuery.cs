// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using bottlenoselabs;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class OcclusionQuery : GraphicsResource
	{
		private readonly IntPtr _query;

		public OcclusionQuery(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice;
			_query = (IntPtr)FNA3D.FNA3D_CreateQuery(GraphicsDevice.Device);
		}

		public bool IsComplete =>
			FNA3D.FNA3D_QueryComplete(GraphicsDevice.Device, (FNA3D.FNA3D_Query*)_query) == 1;

		public int PixelCount =>
			FNA3D.FNA3D_QueryPixelCount(GraphicsDevice.Device, (FNA3D.FNA3D_Query*)_query);

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				FNA3D.FNA3D_AddDisposeQuery(GraphicsDevice.Device, (FNA3D.FNA3D_Query*)_query);
			}

			base.Dispose(disposing);
		}

		public void Begin() => FNA3D.FNA3D_QueryBegin(GraphicsDevice.Device, (FNA3D.FNA3D_Query*)_query);

		public void End() => FNA3D.FNA3D_QueryEnd(GraphicsDevice.Device, (FNA3D.FNA3D_Query*)_query);
	}
}
