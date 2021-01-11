// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;

namespace Katabasis
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
