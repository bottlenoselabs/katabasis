// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Ankura
{
    [SuppressUnmanagedCodeSecurity]
    internal static class Kernel32
    {
        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr module, string procName);

        [DllImport("kernel32")]
        public static extern int FreeLibrary(IntPtr module);
    }
}
