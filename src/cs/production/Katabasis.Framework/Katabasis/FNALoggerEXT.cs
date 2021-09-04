// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using C2CS;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "CA2211", Justification = "Hooks.")]
	public static unsafe class FNALoggerEXT
	{
		/* Use to spit out useful information to the player/dev */
		public static Action<string>? LogInfo;

		/* Use when something sketchy happens, but isn't deadly */
		public static Action<string>? LogWarn;

		/* Use when something has gone horribly, horribly wrong */
		public static Action<string>? LogError;

		private static readonly FNA3D.FNA3D_LogFunc LogInfoFunc = new() { Pointer = &FNA3DLogInfo };
		private static readonly FNA3D.FNA3D_LogFunc LogWarnFunc = new() { Pointer = &FNA3DLogWarn };
		private static readonly FNA3D.FNA3D_LogFunc LogErrorFunc = new() { Pointer = &FNA3DLogError };

		internal static void Initialize()
		{
			/* Don't overwrite application log hooks! */
			LogInfo ??= Console.WriteLine;
			LogWarn ??= Console.WriteLine;
			LogError ??= Console.WriteLine;

			/* Try to hook into the FNA3D logging system */
			try
			{
				FNA3D.FNA3D_HookLogFunctions(
					LogInfoFunc,
					LogWarnFunc,
					LogErrorFunc);
			}
			catch (DllNotFoundException)
			{
				/* Nothing to see here... */
			}
		}

		[UnmanagedCallersOnly]
		private static void FNA3DLogInfo(CString8U msg) => LogInfo!(msg);

		[UnmanagedCallersOnly]
		private static void FNA3DLogWarn(CString8U msg) => LogWarn!(msg);

		[UnmanagedCallersOnly]
		private static void FNA3DLogError(CString8U msg)
		{
			LogError!(msg);
			throw new InvalidOperationException(msg);
		}
	}
}
