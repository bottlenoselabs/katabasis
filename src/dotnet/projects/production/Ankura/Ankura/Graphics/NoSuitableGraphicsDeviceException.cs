// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

namespace Ankura
{
    [Serializable]
    public sealed class NoSuitableGraphicsDeviceException : Exception
    {
        public NoSuitableGraphicsDeviceException()
        {
        }

        public NoSuitableGraphicsDeviceException(string message)
            : base(message)
        {
        }

        public NoSuitableGraphicsDeviceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
