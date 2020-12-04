// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Runtime.InteropServices;

namespace Katabasis
{
    // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.instanceplaylimitexception.aspx
    [Serializable]
    public sealed class InstancePlayLimitException : ExternalException
    {
        public InstancePlayLimitException()
        {
        }

        public InstancePlayLimitException(string message)
            : base(message)
        {
        }

        public InstancePlayLimitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
