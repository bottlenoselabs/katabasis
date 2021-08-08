// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.Reflection;

namespace Katabasis
{
	internal static class AssemblyHelper
	{
		public static string GetDefaultWindowTitle()
		{
			// Set the window title.
			var windowTitle = string.Empty;

			// When running unit tests this can return null.
			var assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
			{
				// Use the Title attribute of the Assembly if possible.
				try
				{
					var assemblyTitleAtt = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
					if (assemblyTitleAtt != null)
					{
						windowTitle = assemblyTitleAtt.Title;
					}
				}
				catch
				{
					// Nope, wasn't possible :/
				}

				// Otherwise, fallback to the Name of the assembly.
				if (string.IsNullOrEmpty(windowTitle))
				{
					windowTitle = assembly.GetName().Name;
				}
			}

			return windowTitle ?? string.Empty;
		}
	}
}
