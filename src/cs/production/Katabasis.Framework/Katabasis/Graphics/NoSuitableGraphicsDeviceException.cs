// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Runtime.Serialization;

namespace Katabasis
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
