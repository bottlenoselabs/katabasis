# Ankura Mirgration Guide for FNA Developers

> :notebook: The following guide is for the latest version of Ankura and the latest version of FNA.

Since Ankura is a fork of FNA, the differences between Ankura and FNA is just differences between the XNA API from the side of C#; all native libraries remain the same.

## `System.Numerics`

Some core types have been removed and replaced with types from `System.Numerics`. `System.Numerics` has a bunch of optimizations and hardware intrinsics which can increase the performance of your game. This means that `System.Numerics` is now a dependency; you will need to add `using System.Numerics;` to the top of your `.cs` files to access these types.

The following types are: `Vector2`, `Vector3`, `Vector4`. The `Matrix` type is also replaced but it has new type name of `Matrix4x4`. These types all have the same blittable data layout and are basically pure drop-in replacements. One new type that might be of interest is the `Matrix3x2` which is the equivalent of a `Matrix4x4` but for 2D transformations. If you are working in 2D it's recommended to use `Matrix3x2` instead of `Matrix4x4` for smaller data footprints (24 bytes vs 64 bytes). 

The following are resolutions to compile time errors for switching to `System.Numerics`. If you find more please create an [issue](https://github.com/craftworkgames/Ankura/issues).

### 1. `Vector2.Normalize()`

With the XNA API, your code would be something like the following:

```
Vector2 myVector;
...
myVector.Normalize();
```

However, this method does not exist in `System.Numerics.Vector2`. The following would be equivalent:

```
Vector2 myVector;
...
myVector = Vector2.Normalize(myVector);
```

## Removed types

The following types have been deleted in comparision to the XNA API. If you need any of them it's recommended you copy the missing code from FNA to your project, roll your equivalents, or find a middleware library to use which has the same functionality.

- `Point`
- `Plane`
- `Ray`
- `BoundingFrustum`
- `BoundingBox`
- `BoundingSphere`
- `ContainmentType`
- `PlaneIntersectionType`
- `Curve`
- `CurveKey`
- `CurveKeyCollection`
- `CurveLoopType`
- `CurveTangent`
- `Model`
- `ModelBone`
- `ModelBoneCollection`
- `ModelEffectCollection`
- `ModelMesh`
- `ModelMeshCollection`
- `ModelMeshPart`
- `ModelMeshPartCollection`
- `GameComponent`
- `DrawableGameComponent`
- `GameComponentCollection`
- `GameComponentCollectionEventArgs`
- `IDrawable`
- `IUpdateable`
- `GameServiceContainer`
- `IGraphicsDeviceManager`
- `IGraphicsDeviceService`


### Removed math types: `Point`, `Plane`, `Ray`, `BoundingFrustum`, `BoundingBox`, `BoundingSphere`, `ContainmentType`, `PlaneIntersectionType`, `PlaneIntersectionType`, `Curve`, `CurveKey`, `CurveKeyCollection`, `CurveLoopType`, and `CurveTangent`

These types are removed simply because there names and usages are confusing for developers between contexts, especially between 2D and 3D. 

For example, `Point` is a 2D point with signed 32-bit integer components. But where is the 3D point type? What if a developer want to use a point struct with different integer bits signed or unsigned? Similiarly for `BoundingBox` which is a 2D AABB (axis-aligned-bounding-box) using 32-bit floats but where is the 2D equivalent? Sure, there is `Rectangle` but that's using 32-bit signed integer components! 

What's worse is that down-stream developers for middleware feel the effects of these bad descisions with the XNA API when designing and implementing their own API. For example, what if developers want to use the name `BoundingBox` for their 2D engine but it is already taken? Sure, they could use a different namespace, but this makes things more complex than they really need to be.

The types are simply fighting against developers more than they are helping developers.

### Removed 3D model types: `Model`, `ModelBone`, `ModelBoneCollection`, `ModelEffectCollection`, `ModelMesh`, `ModelMeshCollection`, `ModelMeshPart`, and `ModelMeshPartCollection`

These types are removed because they are for 3D games and really belong in the engine layer of the game, not the framework layer. What's worse is that these types are tightly coupled to XNA content pipeline. Removing these types is a step in the right direction to remove the infamous content pipeline.

### Removed "components" and friend types: `GameComponent`, `DrawableGameComponent`, `GameComponentCollection`, `GameComponentCollectionEventArgs`, `IDrawable`, `IUpdateable`

These types are removed because they really belong in the engine layer, not the framework layer. What's worse is that name "component" is confusing because that name is usually used in the context of [Entity Component System](https://github.com/SanderMertens/ecs-faq). `GameComponent` is really more akin to **system** than a **component** in the context of ECS. It's best to just remove these types so there is *less* confusion around ECS, which itself is already an up-hill battle for business application developers getting their feet wet into game development.

### Removed services and friends: `GameServiceContainer`, `IGraphicsDeviceManager`, `IGraphicsDeviceService`

These types are removed because an inversion of control (IoC) container and company idea of dependency injection should *not* be forced upon in the framework layer. Decide for *yourself* whether you want to use this pattern in your game. 

If you do want to use "services" and dependency injection in your game, I would highly recommend you read the following article on [Game Programming Patterns: "Service Locator"](https://gameprogrammingpatterns.com/service-locator.html).

#### `GraphicsDevice`

By removing these types mean that some types that depend on services to get a `GraphicsDevice` like `ContentManger` need to be adjusted. With Ankura, you can access the `GraphicsDevice` at any time by going through the public instance field member: `GraphicsDeviceManager.Instance.GraphicsDevice`. This is possible because with FNA a `GraphicsDevice` is never lost which allows for only one `GraphicsDevice` instance. This means that any types that take a dependecy on `GraphicsDevice` in their constructor like `ContentManager` is no longer required.

For example, before with the XNA API:

```
var graphicsDevice = Game.GraphicsDevice;
var myContentManager = new ContentManager(graphicsDevice);
```

With Ankura:
```
var myContentManager = new ContentManager();
```

This also means that the following types no longer requires a `GraphicsDevice` in it's constructor: `SpriteBatch`, `Texture2D`, `Texture3D`, `TextureCube`, `RenderTarget2D`, `RenderTargetCube`, `VertexBuffer`, `IndexBuffer`, `DynamicVertexBuffer`, `DynamicIndexBuffer`, `AlphaTestEffect`, `BasicEffect`, `DualTextureEffect`, `EnvironmentMapEffect`, `SkinnedEffect`, `SpriteEffect`, `Effect`, `Video`, and `VideoPlayer`.