// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using bottlenoselabs;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class RenderTarget2D : Texture2D, IRenderTarget
	{
		private readonly IntPtr _glColorBuffer;
		private readonly IntPtr _glDepthStencilBuffer;

		public RenderTarget2D(
			int width,
			int height,
			bool mipMap,
			SurfaceFormat preferredFormat,
			DepthFormat preferredDepthFormat)
			: this(
				width,
				height,
				mipMap,
				preferredFormat,
				preferredDepthFormat,
				0,
				RenderTargetUsage.DiscardContents)
		{
		}

		public RenderTarget2D(
			int width,
			int height,
			bool mipMap = false,
			SurfaceFormat preferredFormat = SurfaceFormat.Color,
			DepthFormat preferredDepthFormat = DepthFormat.None,
			int preferredMultiSampleCount = 0,
			RenderTargetUsage usage = RenderTargetUsage.DiscardContents)
			: base(
				width,
				height,
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
					Width,
					Height,
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
				Width,
				Height,
				(FNA3D.FNA3D_DepthFormat)DepthStencilFormat,
				MultiSampleCount);
		}

		public bool IsContentLost => false;

		public DepthFormat DepthStencilFormat { get; }

		public int MultiSampleCount { get; }

		public RenderTargetUsage RenderTargetUsage { get; }

		/// <inheritdoc />
		IntPtr IRenderTarget.DepthStencilBuffer => _glDepthStencilBuffer;

		/// <inheritdoc />
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
