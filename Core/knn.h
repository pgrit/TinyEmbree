#pragma once

#include <vector>
#include <queue>

#include "api.h"

extern "C" {

struct Point {
    float x, y, z;
};

struct Neighbor {
    unsigned int primID;
    float d;
};

TINY_EMBREE_API void* NewKnnAccelerator();
TINY_EMBREE_API void ReleaseKnnAccelerator(void* accelerator);

TINY_EMBREE_API void SetKnnPoints(void* accelerator, Point* data, unsigned int numPoints);

TINY_EMBREE_API void* NewKnnQueryCache();
TINY_EMBREE_API void ReleaseKnnQueryCache(void* cache);

TINY_EMBREE_API Neighbor* KnnQuery(void* accelerator, void* cache, const Point* pos, float radius,
    unsigned int k, unsigned int* numFound, float* furthest);

} // extern "C"