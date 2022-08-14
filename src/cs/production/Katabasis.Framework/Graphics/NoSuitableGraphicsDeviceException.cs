// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Runtime.Serialization;

namespace bottlenoselabs.Katabasis
{
	[Serializable]
	public sealed class NoSuitableGraphicsDeviceException : Exception
	{
		public NoSuitableGraphicsDeviceException()
		{
		}

		private NoSuitableGraphicsDeviceException(SerializationInfo info, StreamingContext context)
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
