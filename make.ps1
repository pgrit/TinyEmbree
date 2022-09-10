# Download the prebuilt binaries for TBB and Embree from GitHub
if (-not(Test-Path -Path "prebuilt" -PathType Container))
{
    $renderLibVersion = "0.1.1"
    if ([environment]::OSVersion::IsWindows())
    {
        Invoke-WebRequest -Uri "https://github.com/pgrit/RenderLibs/releases/download/v$renderLibVersion/RenderLibs-v$renderLibVersion.zip" -OutFile "prebuilt.zip"
        Expand-Archive "prebuilt.zip" -DestinationPath ./prebuilt
        rm prebuilt.zip
    }
    else
    {
        wget -q "https://github.com/pgrit/RenderLibs/releases/download/v$renderLibVersion/RenderLibs-v$renderLibVersion.zip" -O "prebuilt.zip"
        Expand-Archive "prebuilt.zip" -DestinationPath ./prebuilt
        rm prebuilt.zip
    }
}

# Copy the shared libraries to the Runtimes folder for packaging in .NET
mkdir runtimes

mkdir runtimes/linux-x64
mkdir runtimes/linux-x64/native
cp prebuilt/linux/lib/libembree3.so.3 runtimes/linux-x64/native/
cp prebuilt/linux/lib/libtbb.so.12.8 runtimes/linux-x64/native/libtbb.so.12

mkdir runtimes/win-x64
mkdir runtimes/win-x64/native
cp prebuilt/win/bin/embree3.dll runtimes/win-x64/native/
cp prebuilt/win/bin/tbb12.dll runtimes/win-x64/native/

mkdir runtimes/osx-x64
mkdir runtimes/osx-x64/native
cp prebuilt/osx/lib/libembree3.3.dylib runtimes/osx-x64/native/
cp prebuilt/osx/lib/libtbb.12.8.dylib runtimes/osx-x64/native/libtbb.12.dylib

mkdir runtimes/osx-arm64
mkdir runtimes/osx-arm64/native
cp prebuilt/osx-arm64/lib/libembree3.3.dylib runtimes/osx-arm64/native/
cp prebuilt/osx/lib/libtbb.12.8.dylib runtimes/osx-arm64/native/libtbb.12.dylib

mkdir build
cd build

if ([environment]::OSVersion::IsMacOS())
{
    cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_OSX_ARCHITECTURES="x86_64" ..
    if (-not $?) { throw "CMake configure failed" }
    cmake --build . --config Release
    if (-not $?) { throw "Build failed" }

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

cd ..

# Copy the shared library in the correct runtimes subdirectory
mv runtimes/TinyEmbreeCore.dll runtimes/win-x64/native
mv runtimes/libTinyEmbreeCore.so runtimes/linux-x64/native
mv runtimes/libTinyEmbreeCore.dylib runtimes/osx-x64/native
mv runtimes/libTinyEmbreeCore.dylib runtimes/osx-arm64/native

dotnet test