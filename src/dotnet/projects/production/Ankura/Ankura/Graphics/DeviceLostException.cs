// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

namespace Ankura
{
    [Serializable]
    public sealed class DeviceLostException : Exception
    {
        public DeviceLostException()
        {
        }

        public DeviceLostException(string message)
            : base(message)
        {
        }

        public DeviceLostException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
