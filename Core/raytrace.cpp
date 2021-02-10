#include "raytrace.h"
#include "scene.h"

extern "C" {

TINY_EMBREE_API void* InitScene() {
    auto scn = new tinyembree::Scene();
    scn->Init();
    return (void*) scn;
}

TINY_EMBREE_API int AddTriangleMesh(void* scene, const float* vertices, int numVerts,
                                    const int* indices, int numIdx) {
    auto scn = (tinyembree::Scene*) scene;
    return scn->AddMesh(vertices, indices, numVerts, numIdx / 3);
}

TINY_EMBREE_API void FinalizeScene(void* scene) {
    auto scn = (tinyembree::Scene*) scene;
    scn->Finalize();
}

TINY_EMBREE_API Hit TraceSingle(void* scene, Ray ray) {
    auto scn = (tinyembree::Scene*) scene;
    return scn->Intersect(ray);
}

TINY_EMBREE_API void DeleteScene(void* scene) {
    auto scn = (tinyembree::Scene*) scene;
}

}