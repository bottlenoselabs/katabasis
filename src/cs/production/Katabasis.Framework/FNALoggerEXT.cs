// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using static bottlenoselabs.FNA3D;

namespace bottlenoselabs.Katabasis
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "CA2211", Justification = "Hooks.")]
	public static unsafe class FNALoggerEXT
	{
		/* Use to spit out useful information to the player/dev */
		public static Action<string>? LogInfo;

		/* Use when something sketchy happens, but isn't deadly */
		public static Action<string>? LogWarn;

		/* Use when something has gone horribly, horribly wrong */
		public static Action<string>? LogError;

		private static readonly FNA3D_LogFunc LogInfoFunc;
		private static readonly FNA3D_LogFunc LogWarnFunc;
		private static readonly FNA3D_LogFunc LogErrorFunc;

		static FNALoggerEXT()
		{
			LogInfoFunc.Data.Pointer = &FNA3DLogInfo;
			LogWarnFunc.Data.Pointer = &FNA3DLogWarn;
			LogErrorFunc.Data.Pointer = &FNA3DLogError;
		}

		internal static void Initialize()
		{
			/* Don't overwrite application log hooks! */
			LogInfo ??= Console.WriteLine;
			LogWarn ??= Console.WriteLine;
			LogError ??= Console.WriteLine;
		}

		internal static void HookFNA3D()
		{
			/* Try to hook into the FNA3D logging system */
			try
			{
				FNA3D_HookLogFunctions(
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
		private static void FNA3DLogInfo(Runtime.CString msg) => LogInfo!(msg);

		[UnmanagedCallersOnly]
		private static void FNA3DLogWarn(Runtime.CString msg) => LogWarn!(msg);

		[UnmanagedCallersOnly]
		private static void FNA3DLogError(Runtime.CString msg)
		{
			LogError!(msg);
			throw new InvalidOperationException(msg);
		}
	}
}
