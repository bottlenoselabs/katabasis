## Developers: NuGet Packages

For an example of how to use the NuGet packages, please see https://github.com/bottlenoselabs/katabasis-game-template.

Katabasis is structured into a couple NuGet packages.

#### Source code

These packages should be added to your C# library projects which you want to have the APIs available.

- `Katabasis.Framework`

#### Native C/C++ binaries

These packages should be added to your C# executable project. You may add all of them but you only really need the one which the suffix RID matches your operating system.

- `Katabasis.Bedrock.win-x64`
- `Katabasis.Bedrock.osx`
- `Katabasis.Bedrock.linux-x64`

For more information about RIDs, why these C/C++ are necessary, and how it all works please see [ARCHITECTURE.md](ARCHITECTURE.md).

### Sources

By default for C# projects, the source for NuGet.org is already used and available. Additionally, the MyGet.org source can be added for pre-releases which are used for testing and development purposes of Katabasis.

- Releases: `https://api.nuget.org/v3/index.json`
- Development: `https://www.myget.org/F/bottlenoselabs/api/v3/index.json`