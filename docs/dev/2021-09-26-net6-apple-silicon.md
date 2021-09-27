# 2020-09-12: FAudio and some ImGui

### Written by [@lithiumtoast](https://github.com/lithiumtoast) on Sunday, September 26th, 2021

- Update from .NET 5 to .NET 6.
- Support for macOS Apple Silicon.

### .NET 5 to .NET 6

No changes really required other than updating `.csproj` to target .NET 6; easy.

### Apple Silicon

.NET 6 offers support for `osx-arm64` runtime identifier; a runtime identifier is the operating system + the CPU architecture. You can think of it as a "native platform".

Anyways, some re-structuring was done to accomodate `osx-arm64` and to make future runtime identifiers easier in the future. The `Katabasis.Native` C# project hosts the all the native C# bindings and has references to the following meta packages (they hold the native libraries depending on the runtime identifier, they don't hold any C# code):

- `win-x64`: Windows x86_64; your standard Windows gaming desktop or laptop. If you target this runtime, you will see `.dll` files from this meta-project make it into your game's build.
- `osx-x64`: macOS x86_64; x86_64 is the architecture of Intel's 64-bit CPUs, sometimes also simply referred to as x64. It is the architecture for all Intel Macs, laptops or desktops, shipped between 2005 and 2021 (before Apple Silicon). If you target this runtime, you will see `.dylib` files from this meta-project in your game's build.
- `osx-arm64`: macOS arm64; arm64 is the architecture used by newer Macs built on Apple Silicon, shipped in late 2020 and beyond. If you target this runtime, you will see `.dylib` files from this meta-project in your game's build, but be careful they are not compatible with `osx-x64` by default. (You could technically bundle both architectures into the same `.dylib`, something which is unique to how the Darwin operating system works, but I don't do that because it increases the file size.)
- `linux-x64`: Linux x86_64; x86_64 CentOS, Debian, Fedora, Ubuntu, and derivative distro. If you target this runtime, you will see `.so` files from this meta-project make it into your game's build.

### Future target runtime identifiers

A complete list of of runtime identifiers (RIDs) can be found in [this json file](https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.NETCore.Platforms/src/runtime.json). The file gets updated over time with runtime identifiers that get supported by .NET. The following, at this time of writing, are candidates for future "target platforms" of Katabasis:

- `ios-arm64`: iOS arm64; the current 64-bit ARM CPU architecture, as used since the iPhone 5S and later (6, 6S, SE, 7, 8, etc), the iPad Air, iPad Air 2 and iPad Pro, with the A7 and later chips.
- `android-arm64`: Android arm64; Android devices version 5.0 (Lollipop) and above which is like ~98% market share and increasing.
- `win-arm64`: Windows arm64; Windows machines with a 64-bit ARM CPU architecture such as Surface Pro X+.

Is it not clear at this time if Microsoft would introduce runtime identifiers to support .NET for consoles such as Switch, Xbox, or PlayStation.
