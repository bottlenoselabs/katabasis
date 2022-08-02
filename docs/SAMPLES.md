# Samples

To learn how to use the Katabasis check out the following samples. The samples are also used for testing purposes to ensure that Katabasis is working correctly.

## Graphics

> :notebook: The majority of the samples related to graphics have been inspired from https://github.com/floooh/sokol-samples

#|Name|Description
:---:|:---:|:---:
1|[Clear][graphics-sample-01]|[Clears the screen with a specific color.][graphics-sample-01]
2|[Triangle][graphics-sample-02]|[Draw a triangle to the screen in clip space using a vertex buffer and a index buffer.][graphics-sample-02]
3|[Rectangle][graphics-sample-03]|[Draw a rectangle to the screen in clip space using a vertex buffer and a index buffer.][graphics-sample-03]
4|[Buffer Offsets][graphics-sample-04]|[Draw a triangle and a rectangle to the screen in clip space using the same vertex buffer and and index buffer.][graphics-sample-04]
5|[Cube][graphics-sample-05]|[Draw a cube to the screen using a vertex buffer, a index buffer, and a Model-View-Projection matrix (MVP).][graphics-sample-05]
6|[Texture 2D Cube][graphics-sample-06]|[Draw a textured (2D) cube to the screen using a vertex buffer, a index buffer, and a Model-View-Projection matrix (MVP).][graphics-sample-06]
7|[Texture 2D Coordinates][graphics-sample-07]|[Draw textured rectangles to the screen with different sampler parameters.][graphics-sample-07]
8|[Dynamic Texture 2D Cube][graphics-sample-08]|[Draw a cube to the screen with streamed 2D texture data updated to the rules of Conway's Game of Life.][graphics-sample-08]
9|[Render Target (RT)][graphics-sample-09]|[Draw a non-textured cube off-screen to a render target and use the result as as the texture when drawing a cube to the screen.][graphics-sample-09]
10|[Multiple Render Targets (MRT)][graphics-sample-10]|[Draw a cube to multiple render targets and then draw them together to the screen as one.][graphics-sample-10]
11|[Texture 3D Cube][graphics-sample-11]|[Draw a textured (3D) cube to the screen using a vertex buffer, a index buffer, and a Model-View-Projection matrix (MVP).][graphics-sample-11]
12|[Particle Instancing][graphics-sample-12]|[Draw multiple particles to the screen using one immutable vertex, one immutable index buffer, and one vertex buffer with streamed instance data.][graphics-sample-12]

[graphics-sample-01]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.01-Clear/App.cs
[graphics-sample-02]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.02-Triangle/App.cs
[graphics-sample-03]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.03-Rectangle/App.cs
[graphics-sample-04]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.04-BufferOffsets/App.cs
[graphics-sample-05]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.05-Cube/App.cs
[graphics-sample-06]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.06-CubeTexture2D/App.cs
[graphics-sample-07]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.07-TextureCoordinates2D/App.cs
[graphics-sample-08]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.08-CubeDynamicTexture2D/App.cs
[graphics-sample-09]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.09-RenderTarget/App.cs
[graphics-sample-10]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.10-MultipleRenderTargets/App.cs
[graphics-sample-11]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.11-CubeTexture3D/App.cs
[graphics-sample-12]: ../src/dotnet/projects/samples/graphics/Katabasis.Samples.Graphics.12-ParticlesInstancing/App.cs

## Input

#|Name|Description
:---:|:---:|:---:
1|[Mouse][input-sample-01]|[Do logic based on the information of the mouse's state of the previous frame and of the current frame.][input-sample-01]

[input-sample-01]: ../src/dotnet/projects/samples/input/Katabasis.Samples.Input.01-Mouse/App.cs
