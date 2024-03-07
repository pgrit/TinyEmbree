#pragma once

#include <functional>
#include <cassert>
#include <cmath>
#include <iostream>

#include <embree4/rtcore.h>

#include "knn.h"
#include "common.h"

inline bool operator<(Neighbor const& a, Neighbor const& b) {
    return a.d < b.d;
}

namespace tinyembree {

template <class T>
struct Adapter : public T {
    typename T::container_type &GetContainer() { return this->c; }
};

struct KNNResult {
    KNNResult(unsigned int num_knn) {
        k = num_knn;
        knn.GetContainer().reserve(num_knn);
    }

    KNNResult() { k = 0; }

    unsigned int k;
    Adapter<std::priority_queue<Neighbor>> knn;
};

template<typename T>
inline float Distance(const Point& a, const T& b) {
    float x = a.x - b.x;
    float y = a.y - b.y;
    float z = a.z - b.z;
    return std::sqrt(x * x + y * y + z * z);
}

inline void PointBoundsFunc(const struct RTCBoundsFunctionArguments* args) {
    const Point* points = (const Point*) args->geometryUserPtr;
    RTCBounds* bounds_o = args->bounds_o;
    const Point& point = points[args->primID];
    bounds_o->lower_x = point.x;
    bounds_o->lower_y = point.y;
    bounds_o->lower_z = point.z;
    bounds_o->upper_x = point.x;
    bounds_o->upper_y = point.y;
    bounds_o->upper_z = point.z;
}

struct Info {
    Point* points;
    KNNResult* result;
};

inline bool PointQueryFunc(struct RTCPointQueryFunctionArguments* args) {
    RTCPointQuery* query = (RTCPointQuery*)args->query;
    const auto* info = (Info*) args->userPtr;
    const unsigned int primID = args->primID;
    const Point& point = info->points[primID];
    const float d = Distance(point, *query);

    assert(args->query);
    KNNResult* result = info->result;

    if (d < query->radius && (result->knn.size() < result->k || d < result->knn.top().d)) {
        Neighbor neighbor;
        neighbor.primID = primID;
        neighbor.d = d;

        if (result->knn.size() == result->k)
            result->knn.pop();

        result->knn.push(neighbor);

        if (result->knn.size() == result->k) {
            query->radius = result->knn.top().d;
            return true;
        }
    }

    return false;
}

class PointQuery {
    Point* points;
    RTCDevice device;
    RTCScene scene;
    bool isInit;

public:
    PointQuery() {
        device = initializeDevice();
        isInit = false;
    }

    ~PointQuery() {
        if (isInit) {
            rtcReleaseScene(scene);
        }
        rtcReleaseDevice(device);
    }

    void KnnQuery(const Point* pos, float radius, KNNResult* result) {
        RTCPointQuery query;
        query.x = pos->x;
        query.y = pos->y;
        query.z = pos->z;
        query.radius = radius;
        query.time = 0.f;
        RTCPointQueryContext context;
        rtcInitPointQueryContext(&context);

        Info info { points, result };
        rtcPointQuery(scene, &query, &context, PointQueryFunc, (void*)(&info));
    }

    void SetPoints(Point* data, unsigned int num_points) {
        if (isInit)
            rtcReleaseScene(scene);

        scene = rtcNewScene(device);
        isInit = true;

        RTCGeometry geom = rtcNewGeometry(device, RTC_GEOMETRY_TYPE_USER);
        unsigned int geomID = rtcAttachGeometry(scene, geom);

        points = data;
        rtcSetGeometryUserPrimitiveCount(geom, num_points);
        rtcSetGeometryUserData(geom, data);
        rtcSetGeometryBoundsFunction(geom, PointBoundsFunc, nullptr);

        rtcCommitGeometry(geom);
        rtcReleaseGeometry(geom);

        rtcCommitScene(scene);
    }
};

} // namespace tinyembree