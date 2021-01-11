// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Runtime.Serialization;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.nomicrophoneconnectedexception.aspx
	[Serializable]
	public sealed class NoMicrophoneConnectedException : Exception
	{
		public NoMicrophoneConnectedException()
		{
		}

		private NoMicrophoneConnectedException(SerializationInfo info, StreamingContext context)
		{
		}

		public NoMicrophoneConnectedException(string message)
			: base(message)
		{
		}

		public NoMicrophoneConnectedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
