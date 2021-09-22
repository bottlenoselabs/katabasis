
# Building from Source

Adding Katabasis to your project by cloning the source code instead of using NuGet packages is a bit more involved. Nonetheless, it is recommended for the optimal developer experience for the following reasons:
1. When something goes wrong invesigation is possible because, you have the source code.
2. Contributing back upstream by creating pull requests is within reach because, you have the source code.
3. Tweaking or making large changes for your needs is achievable because, you have the source code.

## Prerequisites

1. Download and install [.NET 5](https://dotnet.microsoft.com/download).
2. If you are on Windows, [install Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10).
3. Clone the repository with submodules: `git clone --recurse-submodules https://github.com/lithiumtoast/katabasis.git`.
4. Run `bash ./library.sh` in terminal / powershell / command prompt from the root of the repository to download/build the native library dependencies.

## Visual Studio / Rider / MonoDevelop

Open `./src/dotnet/Katabasis.sln`

## Command Line Interface (CLI)

`dotnet build ./src/dotnet/Katabasis.sln`
