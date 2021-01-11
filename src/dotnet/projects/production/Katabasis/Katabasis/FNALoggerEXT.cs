// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ObjCRuntime;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "CA2211", Justification = "Hooks.")]
	public static class FNALoggerEXT
	{
		/* Use to spit out useful information to the player/dev */
		public static Action<string>? LogInfo;

		/* Use when something sketchy happens, but isn't deadly */
		public static Action<string>? LogWarn;

		/* Use when something has gone horribly, horribly wrong */
		public static Action<string>? LogError;

		private static readonly FNA3D.FNA3D_LogFunc LogInfoFunc = FNA3DLogInfo;
		private static readonly FNA3D.FNA3D_LogFunc LogWarnFunc = FNA3DLogWarn;
		private static readonly FNA3D.FNA3D_LogFunc LogErrorFunc = FNA3DLogError;

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

		[MonoPInvokeCallback(typeof(FNA3D.FNA3D_LogFunc))]
		private static void FNA3DLogInfo(IntPtr msg) => LogInfo!(UTF8_ToManaged(msg));

		[MonoPInvokeCallback(typeof(FNA3D.FNA3D_LogFunc))]
		private static void FNA3DLogWarn(IntPtr msg) => LogWarn!(UTF8_ToManaged(msg));

		[MonoPInvokeCallback(typeof(FNA3D.FNA3D_LogFunc))]
		private static void FNA3DLogError(IntPtr msg)
		{
			string err = UTF8_ToManaged(msg);
			LogError!(err);
			throw new InvalidOperationException(err);
		}

		private static unsafe string UTF8_ToManaged(IntPtr s)
		{
			/* We get to do str len ourselves! */
			var ptr = (byte*)s;
			while (*ptr != 0)
			{
				ptr++;
			}

			string result = Encoding.UTF8.GetString(
				(byte*)s,
				(int)(ptr - (byte*)s));

			return result;
		}
	}
}
