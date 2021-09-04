// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class RenderTargetCube : TextureCube, IRenderTarget
	{
		private readonly IntPtr _glColorBuffer;
		private readonly IntPtr _glDepthStencilBuffer;

		public RenderTargetCube(
			int size,
			bool mipMap,
			SurfaceFormat preferredFormat,
			DepthFormat preferredDepthFormat,
			int preferredMultiSampleCount = 0,
			RenderTargetUsage usage = RenderTargetUsage.DiscardContents)
			: base(
				size,
				mipMap,
				preferredFormat)
		{
			var graphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			DepthStencilFormat = preferredDepthFormat;
			MultiSampleCount = FNA3D.FNA3D_GetMaxMultiSampleCount(
				graphicsDevice.Device,
				(FNA3D.FNA3D_SurfaceFormat)Format,
				MathHelper.ClosestMSAAPower(preferredMultiSampleCount));

			RenderTargetUsage = usage;

			if (MultiSampleCount > 0)
			{
				_glColorBuffer = (IntPtr)FNA3D.FNA3D_GenColorRenderbuffer(
					graphicsDevice.Device,
					Size,
					Size,
					(FNA3D.FNA3D_SurfaceFormat)Format,
					MultiSampleCount,
					(FNA3D.FNA3D_Texture*)_texture);
			}

			// If we don't need a depth buffer then we're done.
			if (DepthStencilFormat == DepthFormat.None)
			{
				return;
			}

			_glDepthStencilBuffer = (IntPtr)FNA3D.FNA3D_GenDepthStencilRenderbuffer(
				graphicsDevice.Device,
				Size,
				Size,
				(FNA3D.FNA3D_DepthFormat)DepthStencilFormat,
				MultiSampleCount);
		}

		public bool IsContentLost => false;

		public DepthFormat DepthStencilFormat { get; }

		public int MultiSampleCount { get; }

		public RenderTargetUsage RenderTargetUsage { get; }

		int IRenderTarget.Width => Size;

		int IRenderTarget.Height => Size;

		IntPtr IRenderTarget.DepthStencilBuffer => _glDepthStencilBuffer;

		IntPtr IRenderTarget.ColorBuffer => _glColorBuffer;

#pragma warning disable 0067
		// We never lose data, but lol XNA4 compliance -flibit
		public event EventHandler<EventArgs>? ContentLost;
#pragma warning restore 0067

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (_glColorBuffer != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeRenderbuffer(GraphicsDevice.Device, (FNA3D.FNA3D_Renderbuffer*)_glColorBuffer);
				}

				if (_glDepthStencilBuffer != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeRenderbuffer(GraphicsDevice.Device, (FNA3D.FNA3D_Renderbuffer*)_glDepthStencilBuffer);
				}
			}

			base.Dispose(disposing);
		}
	}
}
