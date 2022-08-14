# C2C2 Update

### Written by [@lithiumtoast](https://github.com/lithiumtoast) on Monday, August 15th, 2022

### Deploy source code to NuGet and MyGet

After some thought and experimentation I decided to use go back to NuGet, but hear we out!

I was previously avoiding NuGet before because of the following:

1. When something goes wrong invesigation is possible because, you have the source code.
2. Contributing back upstream by creating pull requests is within reach because, you have the source code.
3. Tweaking or making large changes for your needs is achievable because, you have the source code.
 
Point 1 above is really the show stopper; the other points are nice to have and support open source. I will always recommend building from source. However, for better or worse, most C# folks expect to use NuGet and the conviences it offers. This had be thinking that there are two kind of broad group of C# developers which form a spectrum: 

- Beginner or average C# developers.
- Open source C# developers.

The trouble is here is that the bar of entry of doing things the "open source way", especially for beginners of C#, is not always **the best user experience**. If I think back to my first days of programming, I would be at first completely lost with using Git and all the other knowledge / skills required and thus possible give up. I think it is essential to kick-start the dopamine feedback loop for programming; to kick-start "flow" which will more likely nurture maturing and learning more as the journey of programming continues.

So, what if there was a way to have consumers of a NuGet package use the source code instead of a compiled binary?

I learned something interesting after some experimentation: it is possible to package source code files (`.cs`) and have them added to the consumer project for build.

This technique is not completely pro open-source as the consumer does not have a Git repository, and thus point 2 above is not really possible. Point 3 above is possible but be careful with your NuGet caches on disk. Anyways this technique I found, I will call it a compromise to which using NuGet is no longer a show stopper.

Building from source has some additional complexity for C# folks to use Katabasis: building native C/C+ libraries from source. This technique also addresses this nicely as the C/C++ built libraries can be delivered though a NuGet package. This means that the C# developer does not have to install a C/C++ toolchain such as Clang or MSVC. Instead these built native libraries are built and packaged using a GitHub Actions workflow.

Finally, I upload packages to a MyGet feed for pre-releases to test things out before I publish the packages to NuGet. Once the packages are on NuGet.org, they are "sticky" and can never really be truely deleted.

#### How it works

A C# project packaged as a NuGet package has an identifier `package-id`. This identifier is usually the name of C# project prefixed with the organization name. E.g. `bottlenoselabs.Katabasis.Framework`.

By adding a `package-id.props` and `package-id.targets` file and including them for packaging under the `/build` directory of the NuGet package, these files allow the consumer of the NuGet package to run some custom MSBuild extension logic when the package is installed.

It's here in this custom MSBuild logic that we can tell MSBuild to add some additional `.cs` files to the build.

Example: `bottlenoselabs.Katabasis.Framework.props`
```xml
<Project>

    <!-- NuGet package references -->
    <ItemGroup>
        <PackageReference Include="JetBrains.Annotations" Version="2022.1.0" />
    </ItemGroup>

    <!-- Compile content files from the NuGet package after it is installed (expanded on disk) -->
    <PropertyGroup>
        <KatabasisFrameworkContentDirectoryPath>$([System.IO.Path]::GetFullPath('$(MSBuildThisFileDirectory)/../content'))</KatabasisFrameworkContentDirectoryPath>
    </PropertyGroup>
    <ItemGroup>
        <Content Include="$(KatabasisFrameworkContentDirectoryPath)/**/*.*">
            <Pack>false</Pack>
            <Link>Properties/bottlenoselabs/Katabasis.Framework/%(RecursiveDir)%(Filename)%(Extension)</Link>
        </Content>
        <Content Remove="$(KatabasisFrameworkContentDirectoryPath)/**/*.cs">
        </Content>
        <Compile Include="$(KatabasisFrameworkContentDirectoryPath)/**/*.cs" >
            <Link>Properties/bottlenoselabs/Katabasis.Framework/%(RecursiveDir)%(Filename)%(Extension)</Link>
        </Compile>
    </ItemGroup>

</Project>
```

As you can see above I add the `.cs` files to be compiled *by the consumer project* and that's it. 

I optionally have these files "linked" under the `Properties` folder. This purely just for cosmetics as I group things under this folder which are to be tucked away but can still be seen from VisualStudio or Rider if the folder is expanded. This is important because at a quick glance it's obvious if things are working or not as the folder/files should appear there.

I repeat this technique for the native libraries such as the `.dll` / `.dylib` / `.so` files for shipping the compiled C/C++ libraries.

The end result is that the bar of entry is lower for getting setup for Katabasis the first time while still maintaining some advantages of open source. Thus it is more simple for those who are beginners or average C# developers that are accustomed to using NuGet. Those who are more experienced or wish to follow "the open source way" are still more than welcome to not use NuGet and build directly from source.
