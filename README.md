![Build](https://github.com/pgrit/TinyEmbree/workflows/Build/badge.svg)
<a href="https://www.nuget.org/packages/TinyEmbree/">
<img src="https://buildstats.info/nuget/TinyEmbree" />
</a>

# TinyEmbree

A very simple C# wrapper around the [Embree](https://www.embree.org/) ray tracing kernels. Currently only supports simple triangle meshes with no motion blur etc.
Embree and our wrapper are automatically compiled for x86-64 Windows, Linux, and macOS, with support for AVX and AVX2. On these platforms, you can directly use the [Nuget package](https://www.nuget.org/packages/TinyEmbree/).
Other platforms need to compile from source, instructions are below.

## Usage example

The following creates a trivial scene consisting of a single quad and intersects it with a ray:

```C#
Raytracer rt = new();

rt.AddMesh(new(
    new Vector3[] { // vertices
        new(-1, 0, -1),
        new( 1, 0, -1),
        new( 1, 0,  1),
        new(-1, 0,  1)
    }, new int[] { // indices
        0, 1, 2,
        0, 2, 3
    }
));

rt.CommitScene(); // builds the acceleration structures

// Trace a single ray
Hit hit = rt.Trace(new Ray {
    Origin = new(-0.5f, -10, 0),
    Direction = new(0, 1, 0),
    MinDistance = 1.0f
});

if (hit) {
    Console.WriteLine($"Hit: {hit.PrimId}, {hit.Distance}");
}
```

## Dependencies

- [.NET 5.0](https://dotnet.microsoft.com/) (or newer)
- a C++11 (or newer) compiler
- CMake

Embree is included as a submodule, so make sure to clone using `--recursive`, or run

```
git submodule update --init --recursive
```

## Building the C++ wrapper

The process is the usual:

```
mkdir build
cd build
cmake .. -DCMAKE_BUILD_TYPE=Release
cmake --build . --config Release
```

The .dll / .so / .dylib files will be copied to the `Runtimes` folder. 

## Testing the C# wrapper

The `TinyEmbree/TinyEmbree.csproj` project file automatically copies the shared libraries from the `Runtimes` directory.
If you are compiling on a platform other than x86-64 Linux, Windows, or macOS, you need to add entries with the correct runtime identifiers [to the project file](TinyEmbree/TinyEmbree.csproj). The correct RID can be found here: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog

To compile and run the tests:
```
dotnet test
```

That's it. Simply add a reference to `TinyEmbree/TinyEmbree.csproj` to your project and you should be up and running.
