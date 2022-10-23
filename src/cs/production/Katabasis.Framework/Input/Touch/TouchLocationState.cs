// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchlocationstate.aspx
	[PublicAPI]
	public enum TouchLocationState
	{
		Invalid,
		Released,
		Pressed,
		Moved
	}
}
