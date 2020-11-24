// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ankura
{
    public static class Native
    {
        private static IEnumerable<string>? _searchDirectories;

        public static void SetDllImportResolver(Assembly assembly)
        {
            NativeLibrary.SetDllImportResolver(assembly, Resolver);
        }

        [SuppressMessage("ReSharper", "CommentTypo", Justification = "Flags.")]
        public static IntPtr LoadLibrary(string libraryFilePath)
        {
            EnsureIs64BitArchitecture();

            var platform = GetRuntimePlatform();
            return platform switch
            {
                RuntimePlatform.Linux => libdl.dlopen(libraryFilePath, 0x101),
                RuntimePlatform.Windows => Kernel32.LoadLibrary(libraryFilePath),
                RuntimePlatform.macOS => libSystem.dlopen(libraryFilePath, 0x101),
                _ => IntPtr.Zero
            };
        }

        public static bool FreeLibrary(IntPtr libraryHandle)
        {
            var platform = GetRuntimePlatform();
            return platform switch
            {
                RuntimePlatform.Linux => libdl.dlclose(libraryHandle) == 0,
                RuntimePlatform.Windows => Kernel32.FreeLibrary(libraryHandle) != 0,
                RuntimePlatform.macOS => libSystem.dlclose(libraryHandle) == 0,
                _ => false
            };
        }

        public static IntPtr GetLibraryFunctionPointer(IntPtr libraryHandle, string functionName)
        {
            var platform = GetRuntimePlatform();
            return platform switch
            {
                RuntimePlatform.Linux => libdl.dlsym(libraryHandle, functionName),
                RuntimePlatform.Windows => Kernel32.GetProcAddress(libraryHandle, functionName),
                RuntimePlatform.macOS => libSystem.dlsym(libraryHandle, functionName),
                _ => IntPtr.Zero
            };
        }

        public static T GetLibraryFunction<T>(IntPtr libraryHandle)
        {
            return GetLibraryFunction<T>(libraryHandle, string.Empty);
        }

        public static T GetLibraryFunction<T>(IntPtr libraryHandle, string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
            {
                functionName = typeof(T).Name;
                if (functionName.StartsWith("d_", StringComparison.Ordinal))
                {
                    functionName = functionName.Substring(2);
                }
            }

            var functionHandle = GetLibraryFunctionPointer(libraryHandle, functionName);
            if (functionHandle == IntPtr.Zero)
            {
                throw new Exception($"Could not find a function with the given name '{functionName}' in the library.");
            }

            return Marshal.GetDelegateForFunctionPointer<T>(functionHandle);
        }

        public static RuntimePlatform GetRuntimePlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RuntimePlatform.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return RuntimePlatform.macOS;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return RuntimePlatform.Linux;
            }

            return RuntimePlatform.Unknown;
        }

        public static string GetLibraryFileExtension(RuntimePlatform platform)
        {
            return platform switch
            {
                RuntimePlatform.Windows => ".dll",
                RuntimePlatform.macOS => ".dylib",
                RuntimePlatform.Linux => ".so",
                RuntimePlatform.Android => throw new NotImplementedException(),
                RuntimePlatform.iOS => throw new NotImplementedException(),
                RuntimePlatform.Unknown => throw new NotSupportedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }

        public static string GetRuntimeIdentifier(RuntimePlatform platform)
        {
            return platform switch
            {
                RuntimePlatform.Windows => "win-x64",
                RuntimePlatform.macOS => "osx-x64",
                RuntimePlatform.Linux => "linux-x64",
                RuntimePlatform.Android => throw new NotImplementedException(),
                RuntimePlatform.iOS => throw new NotImplementedException(),
                RuntimePlatform.Unknown => throw new NotSupportedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }

        private static IEnumerable<string> GetSearchDirectories()
        {
            if (_searchDirectories != null)
            {
                return _searchDirectories;
            }

            var platform = GetRuntimePlatform();
            var runtimeIdentifier = GetRuntimeIdentifier(platform);

            return _searchDirectories = new[]
            {
                Environment.CurrentDirectory,
                AppDomain.CurrentDomain.BaseDirectory!,
                $"libs/{runtimeIdentifier}",
                $"runtimes/{runtimeIdentifier}/native"
            };
        }

        private static string GetLibraryPath(string libraryName)
        {
            var platform = GetRuntimePlatform();
            var libraryPrefix = platform == RuntimePlatform.Windows ? string.Empty : "lib";
            var libraryFileExtension = GetLibraryFileExtension(platform);
            var libraryFileName = $"{libraryPrefix}{libraryName}";

            var directories = GetSearchDirectories();
            foreach (var directory in directories)
            {
                if (TryFindLibraryPath(directory, libraryFileExtension, libraryFileName, out var result))
                {
                    return result;
                }
            }

            throw new Exception($"Could not find the library path for {libraryName}.");
        }

        private static bool TryFindLibraryPath(
            string directoryPath,
            string libraryFileExtension,
            string libraryFileNameWithoutExtension,
            out string result)
        {
            var searchPattern = $"*{libraryFileExtension}";
            var filePaths = Directory.EnumerateFiles(directoryPath, searchPattern);
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                if (fileName.StartsWith(libraryFileNameWithoutExtension))
                {
                    result = filePath;
                    return true;
                }
            }

            result = string.Empty;
            return false;
        }

        private static IntPtr Resolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var libraryFilePath = GetLibraryPath(libraryName);
            var libraryHandle = LoadLibrary(libraryFilePath);
            return libraryHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureIs64BitArchitecture()
        {
            var runtimeArchitecture = RuntimeInformation.OSArchitecture;
            if (runtimeArchitecture == Architecture.Arm || runtimeArchitecture == Architecture.X86)
            {
                throw new NotSupportedException("32-bit architecture is not supported.");
            }
        }
    }
}
