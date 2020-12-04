# Architecture

## C#

Using C# for game development is going to raise some eyebrows or roll some eyes. The reaction comes from that fact that the technology is tradtionally not very well suited for anything but enterprise solutions. Just like how people make a distinguish for "webdev"-elopers, "gamedev"-elopers make a distinguish for these "bizdev"-elopers. The divide and hate is based on difference in values between these different communities of developers based on delivering good enough software for their customers' demands.

The problem is that for gamedev, the bar is higher for "good enough"; game developers are consistently pushing the envelope of technology for greater imursion of virtual worlds, better visuals, lower power consumption, and more intelligent bots. This drive for the community of game developers to aim towards real-time systems and the community of business developers to aim towards enterpise systems is the reason why C# (or even Java) is not traditionally choosen for game development. 

However, modern technology makes C# a sweet spot for game development. The [**Just-In-Time** (JIT)](https://en.wikipedia.org/wiki/Just-in-time_compilation) compiler makes for quick iteration speed of *development* builds and the [**Ahead-Of-Time** (AOT)](https://en.wikipedia.org/wiki/Ahead-of-time_compilation) makes for native speed of *release* builds comparable to C/C++. This means that games made with C# can be released to players who are expecting fast startups, rock-solid runtime speeds, and small download sizes. For more information see the following GitHub links:  

- https://github.com/dotnet/corert
- https://github.com/dotnet/runtime/issues/40430
- https://github.com/dotnet/runtime/issues/41522
- https://github.com/dotnet/runtimelab/issues/248
- https://github.com/dotnet/runtimelab/issues/336


## Platforms

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
Console Nintendo Switch|:x:|:x:|:x:|:x:|:x:|:construction:|:x:
Console Xbox One|:x:|:x:|:construction:|:construction:|:x:|:x:|:x:
Console PlayStation 4|:x:|:x:|:x:|:x:|:x:|:construction:|:x:

- :mortar_board:: Suited for learning purposes; not recommended for releases  
- :construction:: Not officially supported; looking to expand support