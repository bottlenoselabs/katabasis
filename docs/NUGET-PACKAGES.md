## Developers: NuGet Packages

Adding Katabasis to your project by installing NuGet packages is simple and straightforward. It is only recommended if you value this simplicity over everything else. This makes it a good starting point if you don't have that much experience with C# and the .NET ecosystem. It is also good if you can't be bothered to clone the source code and keep up to date with changes yourself.

### Install: Release

You can install the NuGet packages using the NuGet Package Manager UI or by using a terminal:

`dotnet add package Katabasis`

### Install: Pre-release

You can access pre-release NuGet packages of changes to the `develop` branch by adding my personal feed: `https://www.myget.org/F/lithiumtoast/api/v3/index.json`.  

To add custom feed, create a file called `NuGet.config` beside your `.sln` with the following contents:  
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
        <add key="lithiumtoast" value="https://www.myget.org/F/lithiumtoast/api/v3/index.json" />
    </packageSources>
</configuration>
```

While we are here speaking of the `NuGet.config`, you can configure where NuGet packages are restored (downloaded) with the following under the `configuration` node:  
```xml
<config>
    <add key="globalPackagesFolder" value="./YOUR/PATH" />
</config>
```

The full example of the `NuGet.config` becomes now:  
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <config>
        <add key="globalPackagesFolder" value="./YOUR/PATH" />
    </config>
    <packageSources>
        <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
        <add key="lithiumtoast" value="https://www.myget.org/F/lithiumtoast/api/v3/index.json" />
    </packageSources>
</configuration>
```

You can find more information about `NuGet.config`: https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file