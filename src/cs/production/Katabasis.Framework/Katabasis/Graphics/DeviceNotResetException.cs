// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Runtime.Serialization;

namespace Katabasis
{
	[Serializable]
	public sealed class DeviceNotResetException : Exception
	{
		public DeviceNotResetException()
		{
		}

		private DeviceNotResetException(SerializationInfo info, StreamingContext context)
		{
		}

		public DeviceNotResetException(string message)
			: base(message)
		{
		}

		public DeviceNotResetException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
