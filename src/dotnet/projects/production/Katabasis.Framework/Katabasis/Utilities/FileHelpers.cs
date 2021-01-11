// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.IO;

namespace Katabasis
{
	internal static class FileHelpers
	{
		public static readonly char ForwardSlash = '/';
		public static readonly string ForwardSlashString = new(ForwardSlash, 1);
		public static readonly char BackwardSlash = '\\';

		public static readonly char NotSeparator = Path.DirectorySeparatorChar == BackwardSlash ? ForwardSlash : BackwardSlash;
		public static readonly char Separator = Path.DirectorySeparatorChar;

		public static string NormalizeFilePathSeparators(string name) => name.Replace(NotSeparator, Separator);

		public static string ResolveRelativePath(string filePath, string relativeFile)
		{
			// Uri accepts forward slashes
			filePath = filePath.Replace(BackwardSlash, ForwardSlash);

			// Sanitize the path of double slashes, they confuse Uri
			while (filePath.Contains("//"))
			{
				filePath = filePath.Replace("//", "/");
			}

			var hasForwardSlash = filePath.StartsWith(ForwardSlashString, StringComparison.CurrentCulture);
			if (!hasForwardSlash)
			{
				filePath = ForwardSlashString + filePath;
			}

			// Get a uri for filePath using the file:// schema and no host.
			var src = new Uri("file://" + filePath);

			var dst = new Uri(src, relativeFile);

			// The uri now contains the path to the relativeFile with relative addresses resolved... get the local path.
			var localPath = dst.LocalPath;

			if (!hasForwardSlash && localPath.StartsWith("/", StringComparison.Ordinal))
			{
				localPath = localPath.Substring(1);
			}

			// Convert the directory separator characters to the correct platform specific separator.
			return NormalizeFilePathSeparators(localPath);
		}
	}
}
