#pragma once

// Used to generate correct DLL linkage on Windows
#ifdef TINY_EMBREE_DLL
    #ifdef TINY_EMBREE_EXPORTS
        #define TINY_EMBREE_API __declspec(dllexport)
    #else
        #define TINY_EMBREE_API __declspec(dllimport)
    #endif
#else
    #define TINY_EMBREE_API
#endif

extern "C" {

struct Vector3 {
    float x, y, z;
};

struct Ray {
    Vector3 origin;
    Vector3 direction;
    float minDistance;
};

#define INVALID_MESH_ID ((unsigned int) -1)

struct Hit {
    // SurfacePoint point;
    unsigned int meshId;
    unsigned int primId;
    float u, v;
    float distance;
};

TINY_EMBREE_API int InitScene();

TINY_EMBREE_API int AddTriangleMesh(int scene, const float* vertices, int numVerts,
                                    const int* indices, int numIdx);

TINY_EMBREE_API void FinalizeScene(int scene);

TINY_EMBREE_API Hit TraceSingle(int scene, Ray ray);

}