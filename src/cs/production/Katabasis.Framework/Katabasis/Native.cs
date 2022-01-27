// // <auto-generated/>
// // Copyright (c) Lucas Girouard-Stranks (https://github.com/lithiumtoast). All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the Git repository root directory (https://github.com/lithiumtoast/native-tools-cs) for full license information.
//
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Reflection;
// using System.Runtime.InteropServices;
//
// #nullable enable
//
// namespace Katabasis
// {
//     internal static partial class Native
//     {
//         private static IEnumerable<string>? _librarySearchDirectories;
//
//         /// <summary>
//         ///     Sets a callback for resolving native library imports for a specified <see cref="Assembly" />.
//         /// </summary>
//         /// <param name="assembly">The <see cref="Assembly" />.</param>
//         public static void SetDllImportResolverCallback(Assembly assembly)
//         {
//             NativeLibrary.SetDllImportResolver(assembly, Resolver);
//         }
//
//         private static string GetLibraryFileExtension(RuntimeOperatingSystem operatingSystem)
//         {
//             return operatingSystem switch
//             {
//                 RuntimeOperatingSystem.Windows => ".dll",
//                 RuntimeOperatingSystem.macOS => ".dylib",
//                 RuntimeOperatingSystem.Linux => ".so",
//                 RuntimeOperatingSystem.Android => ".so",
//                 RuntimeOperatingSystem.iOS => ".dylib",
//                 RuntimeOperatingSystem.Unknown => throw new NotSupportedException(),
//                 _ => throw new ArgumentOutOfRangeException(nameof(operatingSystem), operatingSystem, null)
//             };
//         }
//
//         private static string GetRuntimeIdentifier(RuntimeOperatingSystem platform)
//         {
//             return platform switch
//             {
//                 RuntimeOperatingSystem.Windows => Environment.Is64BitProcess ? "win-x64" : "win-x86",
//                 RuntimeOperatingSystem.macOS => "osx-x64",
//                 RuntimeOperatingSystem.Linux => "linux-x64",
//                 RuntimeOperatingSystem.Android => throw new NotImplementedException(),
//                 RuntimeOperatingSystem.iOS => throw new NotImplementedException(),
//                 RuntimeOperatingSystem.Unknown => throw new NotSupportedException(),
//                 _ => throw new ArgumentOutOfRangeException(nameof(RuntimeOperatingSystem), platform, null)
//             };
//         }
//
//         private static IEnumerable<string> GetSearchDirectories(RuntimeOperatingSystem platform)
//         {
//             if (_librarySearchDirectories != null)
//             {
//                 return _librarySearchDirectories;
//             }
//
//             var runtimeIdentifier = GetRuntimeIdentifier(platform);
//
//             var librarySearchDirectories = new List<string>
//             {
//                 Environment.CurrentDirectory,
//                 AppDomain.CurrentDomain.BaseDirectory,
//                 Path.GetFullPath($"libs/{runtimeIdentifier}"),
//                 Path.GetFullPath($"runtimes/{runtimeIdentifier}/native")
//             };
//
//             return _librarySearchDirectories = librarySearchDirectories.ToArray();
//         }
//
//         private static bool TryGetLibraryPath(RuntimeOperatingSystem operatingSystem, string libraryName, out string libraryFilePath)
//         {
//             var libraryPrefix = operatingSystem == RuntimeOperatingSystem.Windows ? string.Empty : "lib";
//             var libraryFileExtension = GetLibraryFileExtension(operatingSystem);
//             var libraryFileName = $"{libraryPrefix}{libraryName}{libraryFileExtension}";
//
//             var directories = GetSearchDirectories(operatingSystem);
//             foreach (var directory in directories)
//             {
//                 var filePath = Path.Combine(directory, libraryFileName);
//                 if (!File.Exists(filePath))
//                 {
//                     continue;
//                 }
//
//                 libraryFilePath = filePath;
//                 return true;
//             }
//
//             libraryFilePath = string.Empty;
//             return false;
//         }
//
//         private static IntPtr Resolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
//         {
//             IntPtr libraryHandle;
//
//             var operatingSystem = Runtime.OperatingSystem;
//             if (TryGetLibraryPath(operatingSystem, libraryName, out var libraryFilePath))
//             {
//                 libraryHandle = NativeLibrary.Load(libraryFilePath);
//                 return libraryHandle;
//             }
//
//             if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out libraryHandle))
//             {
//                 return libraryHandle;
//             }
//
//             throw new Exception(
//                 $"Could not find the native library: {libraryName}. Did you forget to place a native library in the expected file path?");
//         }
//     }
// }
