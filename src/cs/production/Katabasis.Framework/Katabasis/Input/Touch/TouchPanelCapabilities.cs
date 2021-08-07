// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
namespace Katabasis
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchpanelcapabilities.aspx
	public readonly struct TouchPanelCapabilities
	{
		public bool IsConnected { get; }

		public int MaximumTouchCount { get; }

		internal TouchPanelCapabilities(
			bool isConnected,
			int maximumTouchCount)
			: this()
		{
			IsConnected = isConnected;
			MaximumTouchCount = maximumTouchCount;
		}
	}
}
