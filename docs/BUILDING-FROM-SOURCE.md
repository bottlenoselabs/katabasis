
# Building from Source

Adding Katabasis to your project by cloning the source code instead of using NuGet packages is a bit more involved. Nonetheless, it is recommended for the optimal developer experience for the following reasons:
1. When something goes wrong invesigation is possible because, you have the source code.
2. Contributing back upstream by creating pull requests is within reach because, you have the source code.
3. Tweaking or making large changes for your needs is achievable because, you have the source code.

## Prerequisites

1. Download and install [.NET 5](https://dotnet.microsoft.com/download).
2. If you are on Windows, [install Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10).
3. Optional: If you are on Windows, [install Windows terminal](https://docs.microsoft.com/en-us/windows/terminal/get-started).
4. Clone the repository wiht submodules: `git clone --recurse-submodules git@github.com:craftworkgames/Katabasis.git`.
5. Run `bash ./get-fna-libs.sh` in terminal / powershell / command prompt from the root of the repository.

## Visual Studio / Rider / MonoDevelop

Open `./src/dotnet/Katabasis.sln`

## Command Line Interface (CLI)

`dotnet build ./src/dotnet/Katabasis.sln`