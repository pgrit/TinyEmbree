#include "raytrace.h"
#include "scene.h"

#include <tbb/parallel_for.h>

std::vector<std::unique_ptr<tinyembree::Scene>> globalScenes;

extern "C" {

TINY_EMBREE_API int InitScene() {
    globalScenes.emplace_back(new tinyembree::Scene());
    globalScenes.back()->Init();
    return int(globalScenes.size()) - 1;
}

TINY_EMBREE_API int AddTriangleMesh(int scene, const float* vertices, int numVerts,
                                    const int* indices, int numIdx) {
    return globalScenes[scene]->AddMesh(vertices, indices, numVerts, numIdx / 3);
}

TINY_EMBREE_API void FinalizeScene(int scene) {
    globalScenes[scene]->Finalize();
}

TINY_EMBREE_API Hit TraceSingle(int scene, Ray ray) {
    return globalScenes[scene]->Intersect(ray);
}

}