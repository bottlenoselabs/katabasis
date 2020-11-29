// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

namespace Ankura
{
    public class GraphicsDeviceInformation
    {
        public GraphicsDeviceInformation Clone()
        {
            return new GraphicsDeviceInformation
            {
                Adapter = Adapter,
                GraphicsProfile = GraphicsProfile,
                PresentationParameters = PresentationParameters?.Clone()
            };
        }

        public GraphicsAdapter? Adapter { get; set; }

        public GraphicsProfile GraphicsProfile { get; set; }

        public PresentationParameters? PresentationParameters { get; set; }
    }
}
