# 2020-09-06: FNA3D and other native libraries

### Written by [@lithiumtoast](https://github.com/lithiumtoast) on Monday, September 6th, 2021

This time we are going to take a look at replacing the C# bindings for the some the internal infrastructure of `FNA` including `FNA3D` and `SDL` by using `C2CS`. Also going to discuss `ImGui`. This work is important because it lays down the road for interopability of any C library from C#. This is how we can use many solid C libraries for things such as graphics, networking, or ECS in the future. It's how the XNA graphics API will be replaced with something more modern.

### SDL

Work is done here: https://github.com/lithiumtoast/sdl-cs. These automatic bindings replace the manual bindings: https://github.com/flibitijibibo/SDL2-CS. In the beginning, the automatic SDL C# bindings were generated for each platform such as Windows, Linux, and macOS. However, this is not a good idea because what we really want is to do is have one C# facade library. Why? Because maintaining multiple C# projects for each platform does not scale. It's easier and more sane to target one single C# project and then have different native libraries for the platform specifics. The native library is built from source using the `library.sh` bash script. Right now things are only setup for desktop targets: Windows, macOS, and Linux (Ubuntu), but it should be relatively straight forward to continue the work from here for more targets.

### FNA3D

Work is done here: https://github.com/lithiumtoast/FNA3D-cs. These automatic bindings replace the manual bindings which are integrated in the source code of FNA. The tricky part with FNA3D is that is dynamic link with SDL. That is more or less figured out, but perhaps there may be better ways to handle the problem discovered moving forward. The `library.sh` bash script builds the native SDL library first (required for FNA3D) then the FNA3D native library.

### ImGui

Work is done here: https://github.com/lithiumtoast/imgui-cs. These automatic bindings are used for an optional `Katabasis.ImGui` library which can be used for your game. If you are not familiar with `ImGui` check out https://github.com/ocornut/imgui. It's of my opinion that `ImGui` is something of a standard library that should probably be used for every development of a game. Maybe `ImGui` is not appropriate for the the user facing interface but it's really great for the user interface of developers.

### Results

All existing samples work as expected. A new sample has been created for `ImGui`. This is a major step forward for swapping out the XNA graphics API using a different C library.

### Future work

- Bindings for `FAudio` and `theorafile`. This will replace the majority of all manual C# bindings with automatic ones.
- Bindings for `Refresh`: https://github.com/thatcosmonaut/Refresh. This will allow to target a Vulkan only graphics backend. Vulkan support on macOS is fairly good. This would allow for Vulkan to be accross all desktop targets.
