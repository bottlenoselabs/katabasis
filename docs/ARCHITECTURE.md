# Architecture

## C#

### Problem

Using C# for game development is going to raise some eyebrows or roll some eyes. The reaction comes from that fact that the technology of .NET, along with it's best practices and standards developed by the community [since 2001](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history), are *tradtionally* not very well suited for developing games. There is a quite different set of values amoung developer communities (enterprise, gamedev, webdev, etc) for delivering good enough software for their customers' demands. These values in terms of the software's artifacts are explained by the .NET Foundation in [**form factors**](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md). C# is traditionally used for creating enterprise solutions as exlained in the [Global and General Purpose form factor](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md#global-and-general-purpose). However for developing games, the bar is higher for "good enough". Game developers are consistently pushing the limits of technology for better visuals, lower power consumption, and more intelligence. Players are expecting small binary downloads with real-time input and real-time output at 60 frames per second or more. This drive for the community of game developers to aim towards real-time systems and the community of enterprise developers to aim towards non-real-time systems is the reason why C# (or even Java) is *traditionally* not choosen for game development.

### Solution

*Modern* technology of .NET makes C# a sweet spot for game development. Games are a special category of applications as explained in the [Native AOT form factor](https://github.com/dotnet/designs/blob/main/accepted/2020/form-factors.md#native-aot-form-factors). The [**Just-In-Time** (JIT)](https://en.wikipedia.org/wiki/Just-in-time_compilation) compiler makes for quick *development* builds for prototyping and tinkering. The [**Ahead-Of-Time** (AOT)](https://en.wikipedia.org/wiki/Ahead-of-time_compilation) makes for native speed of *publish* builds comparable to C/C++. This means that developers can iterate on ideas quickly while players can expect fast startups, rock-solid frames, and small download sizes.

However, there is one catch. Developers will need to slowly abandon some traditional standards and best practices for developing *enterprise applications* in favour of some more modern ones for developing *games*. See the [GAMEDEV-CODING-BEST-PRACTICES.md](GAMEDEV-CODING-BEST-PRACTICES.md) for more details.

## Target Platforms

Since the internals of Katabasis are C or Zig libraries, Katabasis is able to target any device or platform. However, not every platform is currently officially supported. The following table shows platforms against 3D graphics APIs and state of current support.

Platform|OpenGL|OpenGLES/WebGL|Direct3D11|Direct3D12|Metal|Vulkan|WebGPU
:---|:---:|:---:|:---:|:---:|:---:|:---:|:---:
Desktop Windows|:mortar_board:|:x:|:white_check_mark:|:construction:|:x:|:construction:|:x:
Desktop macOS|:mortar_board:|:x:|:x:|:x:|:white_check_mark:|:construction:|:x:
Desktop Linux|:white_check_mark:|:x:|:x:|:x:|:x:|:construction:|:x:
Mobile iOS|:x:|:x:|:x:|:x:|:construction:|:construction:|:x:
Mobile Android|:x:|:construction:|:x:|:x:|:x:|:construction:|:x:
Browser WebAssembly|:x:|:construction:|:x:|:x:|:x:|:question:|:construction:
Micro-console tvOS|:x:|:x:|:x:|:x:|:construction:|:construction:|:x:
Console Nintendo|:construction:|:construction:|:x:|:x:|:x:|:construction:|:x:
Console Xbox|:x:|:x:|:construction:|:construction:|:x:|:question:|:x:
Console PlayStation|:x:|:x:|:x:|:x:|:x:|:question:|:x:

- :x: Not practically possible.
- :white_check_mark: Supported.
- :mortar_board: Suited for learning purposes; not recommended for publishing.
- :construction: Under construction or not yet officially supported; looking to expand support.
- :question: Unknown or NDA classified information.