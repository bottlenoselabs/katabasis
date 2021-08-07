// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
namespace Katabasis
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
