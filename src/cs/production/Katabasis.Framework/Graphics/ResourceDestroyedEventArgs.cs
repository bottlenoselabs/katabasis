// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;

namespace bottlenoselabs.Katabasis
{
	public sealed class ResourceDestroyedEventArgs : EventArgs
	{
		internal ResourceDestroyedEventArgs(string name, object? tag)
		{
			Name = name;
			Tag = tag;
		}

		public string Name { get; }

		public object? Tag { get; }
	}
}
