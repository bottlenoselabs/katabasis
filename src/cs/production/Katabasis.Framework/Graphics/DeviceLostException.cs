// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Runtime.Serialization;

namespace bottlenoselabs.Katabasis
{
	[Serializable]
	public sealed class DeviceLostException : Exception
	{
		public DeviceLostException()
		{
		}

		private DeviceLostException(SerializationInfo info, StreamingContext context)
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
