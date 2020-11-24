<p align="center">
  <b>Ankura</b> - XNA re-imagined.</a>
</p>
<p align="center">
    <img src="https://github.com/craftworkgames/Ankura/workflows/CI/CD/badge.svg"/>
</p>

## Background

While there is `MonoGame` and `FNA`, both are laying bricks ontop of the `XNA` API. The focus of `Ankura` is take the good things of `XNA` and dump the rest.

## Developers: Building from Source

### Prerequisites

1. Download and install [.NET 5](https://dotnet.microsoft.com/download).
2. If you are on Windows, [install Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10).
3. Optional: If you are on Windows, [install Windows terminal](https://docs.microsoft.com/en-us/windows/terminal/get-started).
4. Run `bash ./get-fna-libs.sh` in terminal / powershell / command prompt.

### Visual Studio / Rider / MonoDevelop

Open `./src/dotnet/Ankura.sln`

### Command Line Interface (CLI)

`dotnet build ./src/dotnet/Ankura.sln`

## Versioning

`Ankura` uses [calendar versioning](https://calver.org) and [semantic versioning](https://semver.org) (or combination thereof) where appropriate. For example, the version scheme used for some libraries is `YYYY.MM.DD` and for others its `MAJOR.MINOR.PATCH-TAG`.

### Semantic Versioning

`Ankura` uses [`GitVersion`](https://github.com/GitTools/GitVersion) to determine the exact semantic version for each build with [GitHub actions](https://docs.github.com/en/free-pro-team@latest/actions/guides/about-continuous-integration) (automated workflows). 

### Releases

Git tags are releases; when a new tag is created, the version is automatically bumped automatically to the specified tag version.
For a complete list of the release versions, see the [tags on this repository](https://github.com/craftworkgames/Ankura/tags).

## NuGet Packages

Packages are uploaded to my personal feed: `https://www.myget.org/F/lithiumtoast/api/v3/index.json`. This includes rolling builds from pull-requests and tags (releases).

## License

`Ankura` is licensed under the Microsoft Public License (MS-PL). See the [LICENSE file](LICENSE) for details.

`Ankura` uses LzxDecoder.cs, released under a dual MS-PL/LGPL license.
See lzxdecoder.LICENSE for details.

`Ankura` uses code from the Mono.Xna project, released under the MIT license.
See monoxna.LICENSE for details.

`Ankura` uses FAudio, FNA3D, SDL2-CS, and Theorafile, all of which are released under the zib license. See license files in `./ext` for details.