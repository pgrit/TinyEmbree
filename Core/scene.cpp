#include <cassert>
#include <algorithm>
#include <limits>
#include <iostream>

#include "scene.h"

namespace tinyembree {

void ErrorFunction(void* userPtr, RTCError code, const char* str) {
    std::cerr << str << std::endl;
}

void Scene::Init() {
    embreeDevice = initializeDevice();
    embreeScene = rtcNewScene(embreeDevice);
    rtcSetDeviceErrorFunction(embreeDevice, ErrorFunction, nullptr);
    isInit = true;
}

int Scene::AddMesh(const float* vertexData, const int* indexData, int numVerts, int numTriangles) {
    // Create the Embree buffers
    RTCGeometry geom = rtcNewGeometry(embreeDevice, RTC_GEOMETRY_TYPE_TRIANGLE);
    float* vertices = (float*) rtcSetNewGeometryBuffer(geom,
        RTC_BUFFER_TYPE_VERTEX, 0, RTC_FORMAT_FLOAT3,
        3 * sizeof(float), numVerts);
    unsigned* indices = (unsigned*) rtcSetNewGeometryBuffer(geom,
        RTC_BUFFER_TYPE_INDEX, 0, RTC_FORMAT_UINT3,
        3 * sizeof(unsigned), numTriangles);

    // Copy vertex and index data
    std::copy(vertexData, vertexData + numVerts * 3, vertices);
    std::copy(indexData, indexData + numTriangles * 3, indices);

    rtcCommitGeometry(geom);

    int geomId = rtcAttachGeometry(embreeScene, geom);

    rtcReleaseGeometry(geom);

    return geomId;
}

void Scene::Finalize() {
    rtcCommitScene(embreeScene);
}

void Scene::Intersect(const Ray& ray, Hit& hit) {
    struct RTCRayHit rayhit;
    rayhit.ray.org_x = ray.origin.x;
    rayhit.ray.org_y = ray.origin.y;
    rayhit.ray.org_z = ray.origin.z;

    rayhit.ray.dir_x = ray.direction.x;
    rayhit.ray.dir_y = ray.direction.y;
    rayhit.ray.dir_z = ray.direction.z;

    rayhit.ray.tnear = ray.minDistance;
    rayhit.ray.tfar = std::numeric_limits<float>::infinity();

    rayhit.ray.mask = 1;
    rayhit.ray.flags = 0;
    rayhit.ray.time = 0;

    rayhit.hit.geomID = RTC_INVALID_GEOMETRY_ID;

    rtcIntersect1(embreeScene, &rayhit);

    hit = Hit {
        rayhit.hit.geomID, rayhit.hit.primID,
        rayhit.hit.u, rayhit.hit.v,
        rayhit.ray.tfar
    };
}

bool Scene::IsOccluded(const Ray& ray, float maxDistance) {
    struct RTCRay rtcray;
    rtcray.org_x = ray.origin.x;
    rtcray.org_y = ray.origin.y;
    rtcray.org_z = ray.origin.z;
    rtcray.dir_x = ray.direction.x;
    rtcray.dir_y = ray.direction.y;
    rtcray.dir_z = ray.direction.z;
    rtcray.tnear = ray.minDistance;
    rtcray.tfar = maxDistance;
    rtcray.mask = 0;
    rtcray.flags = 0;

    rtcOccluded1(embreeScene, &rtcray);

    return rtcray.tfar < maxDistance;
}

} // namespace tinyembree
