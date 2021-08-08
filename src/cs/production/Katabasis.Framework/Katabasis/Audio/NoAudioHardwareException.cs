// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.noaudiohardwareexception.aspx
	[Serializable]
	public sealed class NoAudioHardwareException : ExternalException
	{
		public NoAudioHardwareException()
		{
		}

		private NoAudioHardwareException(SerializationInfo info, StreamingContext context)
		{
		}

		public NoAudioHardwareException(string message)
			: base(message)
		{
		}

		public NoAudioHardwareException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
