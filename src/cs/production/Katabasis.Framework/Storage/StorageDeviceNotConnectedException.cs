// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[PublicAPI]
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
