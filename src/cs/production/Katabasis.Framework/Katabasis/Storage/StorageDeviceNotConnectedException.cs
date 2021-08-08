// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Not used?")]
	public class StorageDeviceNotConnectedException : ExternalException
	{
		public StorageDeviceNotConnectedException()
		{
		}

		public StorageDeviceNotConnectedException(string message)
			: base(message)
		{
		}

		public StorageDeviceNotConnectedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
