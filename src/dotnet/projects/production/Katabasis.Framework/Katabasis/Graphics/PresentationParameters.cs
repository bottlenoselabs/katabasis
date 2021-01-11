// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

namespace Katabasis
{
	[Serializable]
	public class PresentationParameters
	{
		internal FNA3D.FNA3D_PresentationParameters _parameters;

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
			get => _parameters.BackBufferFormat;
			set => _parameters.BackBufferFormat = value;
		}

		public int BackBufferHeight
		{
			get => _parameters.BackBufferHeight;
			set => _parameters.BackBufferHeight = value;
		}

		public int BackBufferWidth
		{
			get => _parameters.BackBufferWidth;
			set => _parameters.BackBufferWidth = value;
		}

		public Rectangle Bounds => new(0, 0, BackBufferWidth, BackBufferHeight);

		public IntPtr DeviceWindowHandle
		{
			get => _parameters.DeviceWindowHandle;
			set => _parameters.DeviceWindowHandle = value;
		}

		public DepthFormat DepthStencilFormat
		{
			get => _parameters.DepthStencilFormat;
			set => _parameters.DepthStencilFormat = value;
		}

		public bool IsFullScreen
		{
			get => _parameters.IsFullScreen == 1;
			set => _parameters.IsFullScreen = value ? 1 : 0;
		}

		public int MultiSampleCount
		{
			get => _parameters.MultiSampleCount;
			set => _parameters.MultiSampleCount = value;
		}

		public PresentInterval PresentationInterval
		{
			get => _parameters.PresentationInterval;
			set => _parameters.PresentationInterval = value;
		}

		public DisplayOrientation DisplayOrientation
		{
			get => _parameters.DisplayOrientation;
			set => _parameters.DisplayOrientation = value;
		}

		public RenderTargetUsage RenderTargetUsage
		{
			get => _parameters.RenderTargetUsage;
			set => _parameters.RenderTargetUsage = value;
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
