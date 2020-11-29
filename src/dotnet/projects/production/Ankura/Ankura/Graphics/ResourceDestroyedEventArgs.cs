// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

namespace Ankura
{
    public sealed class ResourceDestroyedEventArgs : EventArgs
    {
        public string Name { get; }

        public object? Tag { get; }

        internal ResourceDestroyedEventArgs(string name, object? tag)
        {
            Name = name;
            Tag = tag;
        }
    }
}
