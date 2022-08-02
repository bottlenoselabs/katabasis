# Architecture

In this document you will find information about how Katabasis is technically designed; architecture = design.

## C#

### Problem

Using C# for game development is going to raise some eyebrows or roll some eyes. The reaction comes from that fact that the technology of .NET, along with it's best practices and standards developed by the community [since 2002](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history), are *tradtionally* not very well suited for developing games. There is a quite different set of values amoung developer communities (enterprise, gamedev, webdev, etc) for delivering good enough software for their customers' demands. These values in terms of the software's artifacts are explained by the .NET Foundation in [**form factors**](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md). C# is traditionally used for creating enterprise solutions as explained in the [Global and General Purpose form factor](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md#global-and-general-purpose). However for developing games, the bar is higher for "good enough". Game developers are consistently pushing the limits of technology for better visuals, lower power consumption, and more intelligence. Players are expecting small binary downloads with real-time input and real-time output at 60 frames per second or more. This drive for the community of game developers to aim towards real-time systems and the community of enterprise developers to aim towards non-real-time systems is the reason why C# (or even Java) is *traditionally* not choosen for game development.

### Solution

*Modern* technology of .NET makes C# a sweet spot for game development. Games are a special category of applications as explained in the [Native AOT form factor](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md#native-aot-form-factors). The [**Just-In-Time** (JIT)](https://en.wikipedia.org/wiki/Just-in-time_compilation) compiler makes for quick *development* builds for prototyping and tinkering. The [**Ahead-Of-Time** (AOT)](https://en.wikipedia.org/wiki/Ahead-of-time_compilation) makes for native speed of *publish* builds comparable to C/C++. This means that developers can iterate on ideas quickly while players can expect fast startups, rock-solid frames, and small download sizes.

However, there is one catch. Developers will need to slowly abandon some traditional standards and best practices for developing *enterprise applications* in favour of some more modern ones for developing *games*. See the [GAMEDEV-CODING-BEST-PRACTICES.md](GAMEDEV-CODING-BEST-PRACTICES.md) for more details.

## Target platforms

Since the internals of Katabasis are C/C++/Zig libraries, Katabasis is able to target any device or platform. However, not every platform is currently officially supported. See the table below for currently supported platforms catagorized by a form factor (desktop, mobile, console, etc) and a [runtime identifier (RID)](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) from the side of the .NET platforms.

For purposes of development, only the **desktop** target platform is essential (Windows, macOS, Linux). This is because all the technical details for other target platforms such as **mobile** and **console** are done via native libraries using C/C++/Zig. This means that that there is only one C# project target that you as a C# developer have to worry about. If your game works for **desktop** then it will (should) work for **mobile** and **console** in the future without changing anything C# related. When you are ready to publish your game for a specific target platform you use the appopriate runtime identifier (RID) with `dotnet publish`. E.g., `dotnet publish --runtime win-x64`. This will allow you to create a build with all the correct specific native libraries that you need for the target platform. Additional options for *publish* can even allow you to distribute your game without players requiring to download the .NET runtime as a separate thing and even optionally using [NativeAOT](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT) for C/C++ comperable native binaries.

A complete list of of runtime identifiers (RIDs) can be found in [this json file](https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.NETCore.Platforms/src/runtime.json). The file gets updated over time with runtime identifiers that get supported by .NET.

Is it not clear at this time if Microsoft would introduce runtime identifiers to support .NET for consoles such as Switch, Xbox, or PlayStation. But considering that Microsoft's technology stack is open-source (hopefully it stays that way), I have faith that the community will eventually figure it out if Microsoft doesn't do it directly.

Target|Platform|RID|Supported
:---:|:---:|:---:|:---:
Desktop|Windows|`win-x64`|✅|
Desktop|Windows|`win-arm64`|✅|
Desktop|macOS|`osx-x64`|✅|
Desktop|macOS|`osx-arm64`|✅|
Desktop|Linux|`linux-x64`|✅|
Desktop|Linux|`linux-arm64`|✅|
Mobile|iOS|`ios-arm64`|⭕|
Mobile|Android|`android-arm64`|⭕|
Browser|WebAssembly|`browser-wasm`|⭕|
Micro-console|tvOS|`tvos-arm64`|⭕|
Console|Switch|❓|❓
Console|Xbox|❓|❓
Console|PlayStation|❓|❓

- ✅ Supported.
- ⭕ Known to be possible and under construction or not yet officially supported; looking to expand support.
- ❓ Unknown or NDA classified information; needs more discovery.
