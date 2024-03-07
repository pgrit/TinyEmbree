#pragma once

#include <vector>
#include <memory>

#include "raytrace.h"
#include "common.h"

namespace tinyembree {

class Scene {
public:
    ~Scene() {
        if (isInit) {
            rtcReleaseScene(embreeScene);
            rtcReleaseDevice(embreeDevice);
        }
    }

    void Init();

    int AddMesh(const float* vertexData, const int* indexData, int numVerts, int numTriangles);

    void Finalize();

    void Intersect(const Ray& ray, Hit& hit);

    bool IsOccluded(const Ray& ray, float maxDistance);

private:
    bool isInit = false;
    bool isFinal = false;

    RTCDevice embreeDevice;
    RTCScene embreeScene;
};


} // namespace tinyembree
