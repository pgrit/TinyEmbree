#pragma once

#include <vector>
#include <memory>
#include <embree3/rtcore.h>

#include "raytrace.h"

namespace tinyembree {

class Scene {
public:
    ~Scene() {
        // if (isInit) {
        //     rtcReleaseScene(embreeScene);
        //     rtcReleaseDevice(embreeDevice);
        // }
    }

    void Init();

    int AddMesh(const float* vertexData, const int* indexData, int numVerts, int numTriangles);

    void Finalize();

    Hit Intersect(const Ray& ray);

private:
    bool isInit = false;
    bool isFinal = false;

    RTCDevice embreeDevice;
    RTCScene embreeScene;
};


} // namespace tinyembree
