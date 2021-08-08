// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.ObjectModel;

namespace Katabasis
{
	public sealed class GraphicsAdapter
	{
		private static ReadOnlyCollection<GraphicsAdapter>? _adapters;

		internal GraphicsAdapter(
			DisplayModeCollection modes,
			string name,
			string description)
		{
			SupportedDisplayModes = modes;
			DeviceName = name;
			Description = description;
			UseNullDevice = false;
			UseReferenceDevice = false;
		}

		public DisplayMode CurrentDisplayMode => FNAPlatform.GetCurrentDisplayMode(Adapters.IndexOf(this));

		public DisplayModeCollection SupportedDisplayModes { get; }

		public string Description { get; }

		public int DeviceId => throw new NotImplementedException();

		public string DeviceName { get; }

		public bool IsDefaultAdapter => this == DefaultAdapter;

		public bool IsWideScreen
		{
			get
			{
				/* Common non-widescreen modes: 4:3, 5:4, 1:1
				 * Common widescreen modes: 16:9, 16:10, 2:1
				 * XNA does not appear to account for rotated displays on the desktop
				 */
				const float limit = 4.0f / 3.0f;
				var aspect = CurrentDisplayMode.AspectRatio;
				return aspect > limit;
			}
		}

		public IntPtr MonitorHandle => throw new NotImplementedException();

		public int Revision => throw new NotImplementedException();

		public int SubSystemId => throw new NotImplementedException();

		public bool UseNullDevice { get; set; }

		public bool UseReferenceDevice { get; set; }

		public int VendorId => throw new NotImplementedException();

		public static GraphicsAdapter DefaultAdapter => Adapters[0];

		public static ReadOnlyCollection<GraphicsAdapter> Adapters =>
			_adapters ??= new ReadOnlyCollection<GraphicsAdapter>(FNAPlatform.GetGraphicsAdapters());

		public bool IsProfileSupported(GraphicsProfile graphicsProfile) =>
			/* TODO: This method could be genuinely useful!
	         * Maybe look into the difference between Reach/HiDef and add the
	         * appropriate properties to the GLDevice.
	         * -flibit
	         */
			true;

		public bool QueryRenderTargetFormat(
			GraphicsProfile graphicsProfile,
			SurfaceFormat format,
			DepthFormat depthFormat,
			int multiSampleCount,
			out SurfaceFormat selectedFormat,
			out DepthFormat selectedDepthFormat,
			out int selectedMultiSampleCount)
		{
			/* Per the OpenGL 3.0 Specification, section 3.9.1,
			 * under "Required Texture Formats". These are the
			 * formats required for renderbuffer support.
			 *
			 * TODO: Per the 4.5 Specification, section 8.5.1,
			 * RGB565, RGB5_A1, RGBA4 are also supported.
			 * -flibit
			 */
			if (format != SurfaceFormat.Color &&
			    format != SurfaceFormat.Rgba1010102 &&
			    format != SurfaceFormat.Rg32 &&
			    format != SurfaceFormat.Rgba64 &&
			    format != SurfaceFormat.Single &&
			    format != SurfaceFormat.Vector2 &&
			    format != SurfaceFormat.Vector4 &&
			    format != SurfaceFormat.HalfSingle &&
			    format != SurfaceFormat.HalfVector2 &&
			    format != SurfaceFormat.HalfVector4 &&
			    format != SurfaceFormat.HdrBlendable)
			{
				selectedFormat = SurfaceFormat.Color;
			}
			else
			{
				selectedFormat = format;
			}

			selectedDepthFormat = depthFormat;
			selectedMultiSampleCount = 0; // Okay, sure, sorry.

			return format == selectedFormat &&
			       depthFormat == selectedDepthFormat &&
			       multiSampleCount == selectedMultiSampleCount;
		}

		public bool QueryBackBufferFormat(
			GraphicsProfile graphicsProfile,
			SurfaceFormat format,
			DepthFormat depthFormat,
			int multiSampleCount,
			out SurfaceFormat selectedFormat,
			out DepthFormat selectedDepthFormat,
			out int selectedMultiSampleCount)
		{
			selectedFormat = SurfaceFormat.Color; // Seriously?
			selectedDepthFormat = depthFormat;
			selectedMultiSampleCount = 0; // Okay, sure, sorry.

			return format == selectedFormat &&
			       depthFormat == selectedDepthFormat &&
			       multiSampleCount == selectedMultiSampleCount;
		}

		internal static void AdaptersChanged() => _adapters = null;
	}
}
