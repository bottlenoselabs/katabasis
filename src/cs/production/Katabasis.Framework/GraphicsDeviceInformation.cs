// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
namespace bottlenoselabs.Katabasis
{
	public class GraphicsDeviceInformation
	{
		public GraphicsAdapter? Adapter { get; set; }

		public GraphicsProfile GraphicsProfile { get; set; }

		public PresentationParameters? PresentationParameters { get; set; }

		public GraphicsDeviceInformation Clone() =>
			new() {Adapter = Adapter, GraphicsProfile = GraphicsProfile, PresentationParameters = PresentationParameters?.Clone()};
	}
}
