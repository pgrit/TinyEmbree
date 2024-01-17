#include "knn.h"
#include "point_query.h"

extern "C" {

TINY_EMBREE_API void* NewKnnAccelerator() {
    return (void*) (new tinyembree::PointQuery);
}

TINY_EMBREE_API void ReleaseKnnAccelerator(void* accelerator) {
    delete ((tinyembree::PointQuery*)accelerator);
}

TINY_EMBREE_API void SetKnnPoints(void* accelerator, Point* data, unsigned int numPoints) {
    ((tinyembree::PointQuery*)accelerator)->SetPoints(data, numPoints);
}

TINY_EMBREE_API void* NewKnnQueryCache() {
    return (void*) (new tinyembree::KNNResult);
}

TINY_EMBREE_API void ReleaseKnnQueryCache(void* cache) {
    delete ((tinyembree::KNNResult*)cache);
}

TINY_EMBREE_API Neighbor* KnnQuery(void* accelerator, void* cache, const Point* pos, float radius,
                                   unsigned int k, unsigned int* numFound, float* furthest) {
    auto* c = ((tinyembree::KNNResult*)cache);
    c->k = k;
    c->knn.GetContainer().clear();
    ((tinyembree::PointQuery*)accelerator)->KnnQuery(pos, radius, c);
    *numFound = c->knn.size();
    if (*numFound == 0)
        *furthest = radius;
    else
        *furthest = c->knn.top().d;
    return c->knn.GetContainer().data();
}

} // extern "C"