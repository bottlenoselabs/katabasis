// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.instanceplaylimitexception.aspx
	[Serializable]
	public sealed class InstancePlayLimitException : ExternalException
	{
		public InstancePlayLimitException()
		{
		}

		private InstancePlayLimitException(SerializationInfo info, StreamingContext context)
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
