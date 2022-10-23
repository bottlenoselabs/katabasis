// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[PublicAPI]
	public class PreparingDeviceSettingsEventArgs : EventArgs
	{
		public PreparingDeviceSettingsEventArgs(GraphicsDeviceInformation graphicsDeviceInformation) =>
			GraphicsDeviceInformation = graphicsDeviceInformation;

		public GraphicsDeviceInformation GraphicsDeviceInformation { get; }
	}
}
