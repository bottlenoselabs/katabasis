# Frequently Answered Questions (FAQ)

## What's the difference between Katabasis and FNA?

Katabasis is a fork of FNA. ImGui is built in.

As of right now there is isn't much difference as Katabasis is mostly just a vanilla fork with some minor changes, some code removed, some things cleaned up a bit, and some additional C libraries. For a guide of effective changes you need to worry about for your game code see [MIGRATION-GUIDE-FNA.md](MIGRATION-GUIDE-FNA.md).

 Under the hood the biggest difference to FNA right now is that all the C# code that binds to native libraries via `DllImport` for `SDL`, `FNA3D`, `FAudio`, `Theorafile` are using automatically generated C# code bindings using [C2CS](https://github.com/bottlenoselabs/c2cs) instead of the manually written C# code bindings provided by FNA. While there is no real "effective" advantage for using C2CS for FNA's dependecies of C libraries, in the future if FNA is no longer to be used the C2CS tool makes using other C libraries quick and easy from C#. Using C2CS does add the nice advantage that all of SDL library's functions are added to C# automatically while FNA does not consider adding C# bindings for SDL if not necessary or automatically. It is also great to add additional C libraries to Katabasis such as `imgui`.
 
 There is plans to add extensions and features in the future which will make Katabasis differentiate from FNA more such: additional 2D and 3D primitives; better content pipeline; hot reloading workflows; feaatures from MonoGame.Extended such as Tiled, etc. 