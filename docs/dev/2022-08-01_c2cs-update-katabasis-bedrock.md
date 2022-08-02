# C2C2 Update

### Written by [@lithiumtoast](https://github.com/lithiumtoast) on Monday, August 1st, 2022

- Update for C2CS and Katabasis project structure
- Update for Katabasis project structure and macOS Apple Silicon

### Update for C2CS and Katabasis project structure

The last year or so was re-working C2CS from "experiment phase" to "this is going to work for all known platforms". A lot was learned along with some testing outside of Katabasis like the `sokol` and `flecs` projects.

Anyways, the C2CS project https://github.com/bottlenoselabs/c2cs is one the core tools used for generating cross-platform bindings of C libraries for C#. Katabasis uses it for FNA C libraries (and other C libraries in the future) instead of using the C# bindings provided by FNA. The reason for this is to have as little dependency on the FNA team as possible and swap out for possibly some other C libraries in the future.

All the native C libraries and the generated code C# for the C libraries are now part of the [katabasis-bedrock](https://github.com/bottlenoselabs/katabasis-bedrock) project. The name "bedrock" comes from geology but most people would probably think Minecraft. The idea is that most C# developers wouldn't have the knowledge or tools to deal with C libraries and thus this layer is "hard as bedrock".

The reason for this switch instead of directly having in the main Katabasis repository is that Dependabot can make GitHub notifications kind of annoying. Anyone who is watching the Katabasis repository will get notifications from Dependabot when an upstream C library has an update. I myself sure kind of got annoyed so I believe everyone is not interested in such noisy notifications. 

The project is now structured into `Katabasis.Bedrock` for all the C# bindings and several "meta C#" projects for only the native libraries (no code) `Katabasis.Bedrock.RID_HERE`. A refresher on the most important [Runtime Identifiers](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) you probably care about:

- `win-x64`: Windows x86_64; your standard Windows gaming desktop or laptop. If you target this runtime, you will see `.dll` files from this meta-project make it into your game's build.
- `osx-x64`: macOS x86_64; x86_64 is the architecture of Intel's 64-bit CPUs, sometimes also simply referred to as x64. It is the architecture for all Intel Macs, laptops or desktops, shipped between 2005 and 2021 (before Apple Silicon). If you target this runtime, you will see `.dylib` files from this meta-project in your game's build.
- `osx-arm64`: macOS arm64; arm64 is the architecture used by newer Macs built on Apple Silicon, shipped in late 2020 and beyond. If you target this runtime, you will see `.dylib` files from this meta-project in your game's build, but be careful they are not compatible with `osx-x64` by default. (You could technically bundle both architectures into the same `.dylib`, something which is unique to how the Darwin operating system works.)
- `linux-x64`: Linux x86_64; x86_64 CentOS, Debian, Fedora, Ubuntu, and derivative distro. If you target this runtime, you will see `.so` files from this meta-project make it into your game's build.

The C# "meta" bedrock projects:

- `Katabasis.Bedrock.osx`: Apple `x64` + `arm64`. Yes, both architectures are together for convenience; you shouldn't have to care about creating two different builds of your game for Intel and Apple Silicon. 
- `Katabasis.Bedrock.win-x64`: Windows `x64`. Your standard Windows gaming desktop or laptop. If you target this runtime, you will see `.dll` files from this meta-project make it into your game's build.
- `Katabasis.Bedrock.linux-x64`: Linux `x64`. Your standard machine with most Linux distros.

More C# "meta" projects will be included in the future for future target platforms. Both Android and iOS are low hanging fruit which can be added quite easily, I just never got around to it yet.

### Update for macOS Apple Silicon

I finally got the Video playback sample working on Apple Silicon, yay. All samples as of date work as expected on Apple Silicon.