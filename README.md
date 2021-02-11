<p align="center">
  <b>Katabasis</b> - <a href=https://en.wikipedia.org/wiki/Microsoft_XNA>XNA</a> re-imagined.</br>The software developer journeys to the underworld in search of augmented knowledge of game development.</a>
</p>
<p align="center">
    <img src="https://github.com/craftworkgames/Katabasis/workflows/Build%20test%20deploy/badge.svg"/>
</p>

Katabasis is built for [programmers](https://en.wikipedia.org/wiki/Programmer) to make games using C#. It's not an engine, but rather some fundemental APIs glued together to form a *framework*. These include: playing audio, rendering of 3D and 2D graphics; and handling input such as mouse, keyboard, gamepads.

## News

To see what's new, check the development log! [docs/dev/LOG.md](docs/dev/LOG.md)  
Lastest update: [2021-02-10: The vision for Katabasis; Part 1: My past](2021-02-10_vision-for-katabasis-part-1-my-past)

## Background: Why?

### Problem

For creating games with C#, there are a [couple options](https://dotnet.microsoft.com/apps/games/engines). However, the majority are engines, which offer you pre-made wheels to build your car. To those who want to just create a game without focusing too much on programming, like designers or artists, this is desired. But, for those who want to take [an epic journey to hell and back for augmented knowledge](https://en.wikipedia.org/wiki/Katabasis#Trip_into_the_underworld) of programming, engines can be unsatisfactory. The options which are not engines are [MonoGame](https://github.com/MonoGame/MonoGame) and [FNA](https://github.com/FNA-XNA/FNA). However, both are laying bricks ontop of the [XNA](https://en.wikipedia.org/wiki/Microsoft_XNA) API. This heritage makes it more difficult than it needs to be for game developers. This is especially true considering the advancements in technology since [XNA](https://en.wikipedia.org/wiki/Microsoft_XNA) was in active development.

### Solution

Katabasis is a fork of [FNA](https://github.com/FNA-XNA/FNA) adapted to be more modern and simple. The idea is to take and improve upon the good things of the XNA API and dump the rest. Some inspiration is also borrowed from other frameworks such as [LÖVE](https://love2d.org) and [libGDX](https://libgdx.badlogicgames.com). The promise is that by using Katabasis to build your game you will: have fun programming, adopt a growth mindset, and enter the gateway to computer science.

## Developers: Documentation

For documentation, see [docs/README.md](docs/README.md). This includes architecture, development logs, how to build from source, NuGet packages, samples, migration guides from MonoGame/FNA, and versioning.

## License

Katabasis is licensed under the Microsoft Public License (`MS-PL`). This is because a constraint of forking [FNA](https://github.com/FNA-XNA/FNA). There are a few exceptions to this detailed below. See the [LICENSE](LICENSE) file for details with this license and this main product.

Katabasis uses [FAudio](https://github.com/FNA-XNA/FAudio), [FNA3D](https://github.com/FNA-XNA/FNA3D), [SDL2-CS](https://github.com/flibitijibibo/SDL2-CS), and [Theorafile](https://github.com/FNA-XNA/Theorafile), all of which are released under the Zlib-Libpng License (`zlib`). This includes the following source code files: [`SDL2.cs`](src/dotnet/projects/production/Katabasis.Framework/SDL2/SDL2.cs), [`SDL2_image.cs`](src/dotnet/projects/production/Katabasis.Framework/SDL2/SDL2_image.cs), [`SDL_mixer.cs`](src/dotnet/projects/production/Katabasis.Framework/SDL2/SDL2_mixer.cs), [`SDL2_ttf.cs`](src/dotnet/projects/production/Katabasis.Framework/SDL2/SDL2_ttf.cs), [`FAudio.cs`](src/dotnet/projects/production/Katabasis.Framework/FAudio.cs), [`Theorafile.cs`](src/dotnet/projects/production/Katabasis.Framework/Theorafile.cs). (`FNA3D` C# code is licensed under `MS-PL` and is a part of Katabasis while the external dynamic library file of `FNA3D` is licensed under `zlib`.) See the linked reposotories for details with the `zlib` license and these products.
