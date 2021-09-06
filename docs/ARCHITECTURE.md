# Architecture

In this document you will find information about how Katabasis is technical designed; architecture = design.

## C#

### Problem

Using C# for game development is going to raise some eyebrows or roll some eyes. The reaction comes from that fact that the technology of .NET, along with it's best practices and standards developed by the community [since 2001](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history), are *tradtionally* not very well suited for developing games. There is a quite different set of values amoung developer communities (enterprise, gamedev, webdev, etc) for delivering good enough software for their customers' demands. These values in terms of the software's artifacts are explained by the .NET Foundation in [**class of applications**](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md). C# is traditionally used for creating enterprise solutions as exlained in the [Global and General Purpose class of app](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md#global-and-general-purpose). However for developing games, the bar is higher for "good enough". Game developers are consistently pushing the limits of technology for better visuals, lower power consumption, and more intelligence. Players are expecting small binary downloads with real-time input and real-time output at 60 frames per second or more. This drive for the community of game developers to aim towards real-time systems and the community of enterprise developers to aim towards non-real-time systems is the reason why C# (or even Java) is *traditionally* not choosen for game development.

### Solution

*Modern* technology of .NET makes C# a sweet spot for game development. Games are a special category of applications as explained in the [Native AOT class of app](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md#native-aot-form-factors). The [**Just-In-Time** (JIT)](https://en.wikipedia.org/wiki/Just-in-time_compilation) compiler makes for quick *development* builds for prototyping and tinkering. The [**Ahead-Of-Time** (AOT)](https://en.wikipedia.org/wiki/Ahead-of-time_compilation) makes for native speed of *publish* builds comparable to C/C++. This means that developers can iterate on ideas quickly while players can expect fast startups, rock-solid frames, and small download sizes.

However, there is one catch. Developers will need to slowly abandon some traditional standards and best practices for developing *enterprise applications* in favour of some more modern ones for developing *games*. See the [GAMEDEV-CODING-BEST-PRACTICES.md](GAMEDEV-CODING-BEST-PRACTICES.md) for more details.

## Target platforms

Since the internals of Katabasis are C/C++/Zig libraries, Katabasis is able to target any device or platform. However, not every platform is currently officially supported. See the table below for currently supported platforms catagorized by a form factor (desktop, mobile, console, etc) and a [runtime identifier (RID)](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) from the side of the .NET platforms.

For purposes of development, only the **desktop** form factor is essential. This is because all the technical details for other form factors such as **mobile**, **console**, etc are done via native libraries using C/C++/Zig. This means that that there is only one C# project target that you as a C# developer have to worry about. If your game works for **desktop** then it will work for **mobile** and **console** in the future without changing anything C# related. When you are ready to publish your game for a specific form factor / platform you use the appopriate runtime identifier with `dotnet publish`. E.g., `dotnet publish --runtime win-x64`. This will allow you to create a build with all the correct specific native libraries that you need for the target.

Form factor|Platform|ISA|RID|.NET Version|Supported
:---:|:---:|:---:|:---:|:---:|:---:
Desktop|Windows|x64|`win-x64`|.NET 5+|✅|
Desktop|Windows|arm64|`win-arm64`|.NET 6+|⭕|
Desktop|macOS|x64|`osx-x64`|.NET 5+|✅|
Desktop|macOS|arm64|`osx-arm64`|.NET 6+|⭕|
Desktop|Linux|x64|`linux-x64`|.NET 5+|✅|
Desktop|Linux|arm64|`linux-arm64`|.NET 6+|⭕|
Mobile|iOS|arm64|`ios-arm64`|.NET 6+|⭕|
Mobile|Android|arm64|`android-arm64`|.NET 6+|⭕|
Browser|WebAssembly|wasm|`browser-wasm`|.NET 5+|⭕|
Micro-console|tvOS|arm64|`tvos-arm64`|.NET 5+|⭕|
Console|Switch|arm64|❓|❓|❓|❓
Console|Xbox|amd64|❓|❓|❓|❓
Console|PlayStation|amd64|❓|❓|❓|❓

- ✅ Supported.
- ⭕ Known to be possible and under construction or not yet officially supported; looking to expand support.
- ❓ Unknown or NDA classified information; needs more discovery.