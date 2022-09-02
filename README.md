<p align="center">
  <b>Katabasis</b> - <a href=https://en.wikipedia.org/wiki/Microsoft_XNA>XNA</a> re-imagined.</br>The software developer journeys to the underworld in search of augmented knowledge of game development.</a>
</p>
<p align="center">
    <img src="https://github.com/lithiumtoast/katabasis/actions/workflows/develop.yml/badge.svg" />
</p>

Katabasis is built for [programmers](https://en.wikipedia.org/wiki/Programmer) to make games using C#. It's not an engine, but rather some fundemental APIs glued together to form a *framework*. These include: playing audio, rendering of 3D and 2D graphics; and handling input such as mouse, keyboard, gamepads.

## News

To see what's new, check the development log! [docs/dev/LOG.md](docs/dev/LOG.md)  
Lastest update: 
- [2022-08-15: NuGet packages revisited](docs/dev/2022-08-15_nuget-packages-revisited.md)

## Background: Why?

### Problem

For creating games with C#, there are a [couple options](https://dotnet.microsoft.com/apps/games/engines). However, the majority are engines, which offer you pre-made wheels to build your car. To those who want to just create a game without focusing too much on programming, like designers or artists, this is desired. But, for those who want to take [an epic journey to hell and back for augmented knowledge](https://en.wikipedia.org/wiki/Katabasis#Trip_into_the_underworld) of programming, engines can be unsatisfactory. The options which are not engines are [MonoGame](https://github.com/MonoGame/MonoGame) and [FNA](https://github.com/FNA-XNA/FNA). However, both are laying bricks ontop of the [XNA](https://en.wikipedia.org/wiki/Microsoft_XNA) API. This heritage makes it more difficult than it needs to be for game developers. This is especially true considering the advancements in technology since [XNA](https://en.wikipedia.org/wiki/Microsoft_XNA) was in active development.

### Solution

Katabasis is a fork of [FNA](https://github.com/FNA-XNA/FNA) adapted to be more modern and simple. The idea is to take and improve upon the good things of the XNA API and dump the rest. Some inspiration is also borrowed from other frameworks such as [LÖVE](https://love2d.org) and [libGDX](https://libgdx.badlogicgames.com). The promise is that by using Katabasis to build your game you will: have fun programming, adopt a growth mindset, and enter the gateway to computer science.

## Developers: Documentation

For documentation, see [docs/README.md](docs/README.md). This includes frequently answered questions (FAQ), architecture, development logs, how to build from source, NuGet packages, samples, migration guides from MonoGame/FNA, versioning, and more!

## License

Katabasis is licensed under the Microsoft Public License (`MS-PL`). This is because a constraint of forking [FNA](https://github.com/FNA-XNA/FNA). There are a few exceptions to this detailed below. See the [LICENSE](LICENSE) file for details with this license and this main product.

Katabasis uses [SDL2](https://github.com/libsdl-org/SDL), [FNA3D](https://github.com/FNA-XNA/FNA3D), [FAudio](https://github.com/FNA-XNA/FAudio), and [Theorafile](https://github.com/FNA-XNA/Theorafile), all of which are released under the Zlib-Libpng License (`zlib`).
