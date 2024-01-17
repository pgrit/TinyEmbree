param(
    [string] $renderLibVersion = "0.2.0",
    [string] $localRenderLibPath = $null
)

function Ensure-Dir {
    param(
        [string] $path
    )
    New-Item -ItemType Directory -Force $path > $null
}

if ($localRenderLibPath)
{
    echo "Using local prebuilt Embree from $localRenderLibPath"
    Remove-Item -Recurse -Force ./prebuilt
    Copy-Item -Force -Recurse -Path $localRenderLibPath -Destination ./prebuilt
}
elseif (-not(Test-Path -Path "prebuilt" -PathType Container))
{
    # Download the prebuilt binaries for TBB and Embree from GitHub
    Invoke-WebRequest -Uri "https://github.com/pgrit/RenderLibs/releases/download/v$renderLibVersion/RenderLibs-v$renderLibVersion.zip" -OutFile "prebuilt.zip"
    Expand-Archive "prebuilt.zip" -DestinationPath ./prebuilt
    rm prebuilt.zip
}

# Copy the shared libraries to the Runtimes folder for packaging in .NET
Ensure-Dir runtimes

Ensure-Dir runtimes/linux-x64
Ensure-Dir runtimes/linux-x64/native
cp prebuilt/linux/lib/libembree3.so.3 runtimes/linux-x64/native/
cp prebuilt/linux/lib/libtbb.so.12.8 runtimes/linux-x64/native/libtbb.so.12

Ensure-Dir runtimes/win-x64
Ensure-Dir runtimes/win-x64/native
cp prebuilt/win/bin/embree3.dll runtimes/win-x64/native/
cp prebuilt/win/bin/tbb12.dll runtimes/win-x64/native/

Ensure-Dir runtimes/osx-x64
Ensure-Dir runtimes/osx-x64/native
cp prebuilt/osx/lib/libembree3.1.dylib runtimes/osx-x64/native/
cp prebuilt/osx/lib/libtbb.12.8.dylib runtimes/osx-x64/native/libtbb.12.dylib

Ensure-Dir runtimes/osx-arm64
Ensure-Dir runtimes/osx-arm64/native
cp prebuilt/osx-arm64/lib/libembree3.1.dylib runtimes/osx-arm64/native/
cp prebuilt/osx/lib/libtbb.12.8.dylib runtimes/osx-arm64/native/libtbb.12.dylib

Ensure-Dir build
cd build

try
{
    if ([environment]::OSVersion::IsMacOS())
    {
        cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_OSX_ARCHITECTURES="x86_64" ..
        if (-not $?) { throw "CMake configure failed" }
        cmake --build . --config Release
        if (-not $?) { throw "Build failed" }

        # Empty the build folder first to avoid cache issues
        rm -rf *

        cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_OSX_ARCHITECTURES="arm64" ..
        if (-not $?) { throw "CMake configure failed" }
        cmake --build . --config Release
        if (-not $?) { throw "Build failed" }
    }
    else
    {
        cmake -DCMAKE_BUILD_TYPE=Release ..
        if (-not $?) { throw "CMake configure failed" }

        cmake --build . --config Release
        if (-not $?) { throw "Build failed" }
    }
}
finally
{
    cd ..
}

# Test the C# wrapper
dotnet build
if (-not $?) { throw "Build failed" }

dotnet test
if (-not $?) { throw "Tests failed" }