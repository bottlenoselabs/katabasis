// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.IO;

namespace Katabasis
{
	public static class TitleContainer
	{
		public static Stream OpenStream(string name)
		{
			string safeName = FileHelpers.NormalizeFilePathSeparators(name);
			return File.OpenRead(Path.IsPathRooted(safeName) ? safeName : Path.Combine(TitleLocation.Path, safeName));
		}

		internal static IntPtr ReadToPointer(string name, out ulong size)
		{
			string safeName = FileHelpers.NormalizeFilePathSeparators(name);
			return FNAPlatform.ReadFileToPointer(Path.IsPathRooted(safeName) ? safeName : Path.Combine(TitleLocation.Path, safeName), out size);
		}
	}
}
