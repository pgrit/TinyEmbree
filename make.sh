# Download the prebuilt binaries for TBB and Embree from GitHub
if [ ! -d "Prebuilt" ]
then
    wget -q "https://github.com/pgrit/RenderLibs/TODO" -O "prebuilt.zip"
    unzip "prebuilt.zip" -d ./Prebuilt
    rm "prebuilt.zip"
fi

# Copy the shared libraries to the Runtimes folder for packaging in .NET
mkdir Runtimes

cp Prebuilt/linux/lib/libembree3.so.3 Runtimes/
cp Prebuilt/linux/lib/libtbb.so.12 Runtimes/

cp Prebuilt/win/bin/embree3.dll Runtimes/
cp Prebuilt/win/bin/tbb12.dll Runtimes/

cp Prebuilt/osx/lib/libembree3.3.dylib Runtimes/
cp Prebuilt/osx/lib/libtbb12.dylib Runtimes/

mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
cmake --build . --config Release
cd ..

dotnet test
