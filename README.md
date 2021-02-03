# TinyEmbree

A very simple C# wrapper around the [Embree](https://www.embree.org/) ray tracing kernels. Currently only supports simple triangle meshes with no motion blur etc.

<a href="https://www.nuget.org/packages/TinyEmbree/">
<img src="https://buildstats.info/nuget/TinyEmbree" />
</a>

## Dependencies

- [.NET 5.0](https://dotnet.microsoft.com/) (or newer)
- a C++11 (or newer) compiler
- CMake
- Embree3
- TBB

You can install them with a package manager of your choice, but you need to make sure that they can be found by CMake. A simple solution is to use [vcpkg](https://github.com/microsoft/vcpkg):

```
vcpkg install embree3
```

## Building on Windows and Linux

First, we need to compile the C++ dependency wrapper. The process is the usual:

```
mkdir build
cd build
cmake .. -DCMAKE_BUILD_TYPE=Release
cmake --build . --config Release
```

The .dll / .so files will be copied to the `Dist` folder. 
Note that, currently in order to create the Nuget package, you might need to copy the Embree or TBB .so files manually to the Dist folder.

To compile and run the tests (optional):
```
dotnet test
```

That's it. Simply add a reference to `TinyEmbree/TinyEmbree.csproj` to your project and you should be up and running.

## Building on other platforms

In theory, the package works on any platform.
However, the native dependencies have to be built for each.
Currently, the workflow has been set up and tested for x86-64 versions of Windows, Linux (Ubuntu 20.04) and macOS 10.15.
Other platforms need to be built from source.
For these, the [TinyEmbree.csproj](TinyEmbree/TinyEmbree.csproj) file needs to be adjusted, instructions can be found in the comments of that file.
The process should be a simple copy&paste operation, provided nothing goes south when building the C++ library.

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