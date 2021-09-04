// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using static FNA3D;

namespace Katabasis
{
	[Serializable]
	public unsafe class PresentationParameters
	{
		internal FNA3D_PresentationParameters Parameters;

		public PresentationParameters()
		{
			BackBufferFormat = SurfaceFormat.Color;
			BackBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
			BackBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
			DeviceWindowHandle = IntPtr.Zero;
			IsFullScreen = false; // FIXME: Is this the default?
			DepthStencilFormat = DepthFormat.None;
			MultiSampleCount = 0;
			PresentationInterval = PresentInterval.Default;
			DisplayOrientation = DisplayOrientation.Default;
			RenderTargetUsage = RenderTargetUsage.DiscardContents;
		}

		public SurfaceFormat BackBufferFormat
		{
			get => (SurfaceFormat)Parameters.backBufferFormat;
			set => Parameters.backBufferFormat = (FNA3D_SurfaceFormat)value;
		}

		public int BackBufferHeight
		{
			get => Parameters.backBufferHeight;
			set => Parameters.backBufferHeight = value;
		}

		public int BackBufferWidth
		{
			get => Parameters.backBufferWidth;
			set => Parameters.backBufferWidth = value;
		}

		public Rectangle Bounds => new(0, 0, BackBufferWidth, BackBufferHeight);

		public IntPtr DeviceWindowHandle
		{
			get => (IntPtr)Parameters.deviceWindowHandle;
			set => Parameters.deviceWindowHandle = (void*)value;
		}

		public DepthFormat DepthStencilFormat
		{
			get => (DepthFormat)Parameters.depthStencilFormat;
			set => Parameters.depthStencilFormat = (FNA3D_DepthFormat)value;
		}

		public bool IsFullScreen
		{
			get => Parameters.isFullScreen == 1;
			set => Parameters.isFullScreen = (byte)(value ? 1 : 0);
		}

		public int MultiSampleCount
		{
			get => Parameters.multiSampleCount;
			set => Parameters.multiSampleCount = value;
		}

		public PresentInterval PresentationInterval
		{
			get => (PresentInterval)Parameters.presentationInterval;
			set => Parameters.presentationInterval = (FNA3D_PresentInterval)value;
		}

		public DisplayOrientation DisplayOrientation
		{
			get => (DisplayOrientation)Parameters.displayOrientation;
			set => Parameters.displayOrientation = (FNA3D_DisplayOrientation)value;
		}

		public RenderTargetUsage RenderTargetUsage
		{
			get => (RenderTargetUsage)Parameters.renderTargetUsage;
			set => Parameters.renderTargetUsage = (FNA3D_RenderTargetUsage)value;
		}

		public PresentationParameters Clone()
		{
			PresentationParameters clone = new();
			clone.BackBufferFormat = BackBufferFormat;
			clone.BackBufferHeight = BackBufferHeight;
			clone.BackBufferWidth = BackBufferWidth;
			clone.DeviceWindowHandle = DeviceWindowHandle;
			clone.IsFullScreen = IsFullScreen;
			clone.DepthStencilFormat = DepthStencilFormat;
			clone.MultiSampleCount = MultiSampleCount;
			clone.PresentationInterval = PresentationInterval;
			clone.DisplayOrientation = DisplayOrientation;
			clone.RenderTargetUsage = RenderTargetUsage;
			return clone;
		}
	}
}
