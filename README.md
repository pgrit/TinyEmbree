![Build](https://github.com/pgrit/TinyEmbree/workflows/Build/badge.svg)
<a href="https://www.nuget.org/packages/TinyEmbree/">
![NuGet Downloads](https://img.shields.io/nuget/dt/TinyEmbree)
</a>

# TinyEmbree

A very simple C# wrapper around the [Embree](https://www.embree.org/) ray tracing kernels. Currently only supports simple triangle meshes with no motion blur etc.
Embree and our wrapper are automatically compiled for x86-64 Windows, Linux, and macOS with support for AVX and AVX2, and for arm64 macOS. On these platforms, you can directly use the [Nuget package](https://www.nuget.org/packages/TinyEmbree/).
Other platforms need to compile both Embree and our C++ wrapper code from source, instructions are below.

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

## Building from source

There is a PowerShell script that downloads the prebuilt binaries for Embree and TBB ([RenderLibs](https://github.com/pgrit/RenderLibs/)). It currently supports x64 Windows and Linux, and x64 and arm64 OSX. You can run it via
```
pwsh ./make.ps1
```
provided that [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) is installed. The script is rather simple (download and extract zip, run CMake, copy binaries) and you can also manually perform the same steps.

If compilation was successful, the C# wrapper is also automatically built and tested by the script.
