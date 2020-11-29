// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

namespace Ankura
{
    public sealed class ResourceCreatedEventArgs : EventArgs
    {
        public object? Resource { get; }

        internal ResourceCreatedEventArgs(object? resource)
        {
            Resource = resource;
        }
    }
}
