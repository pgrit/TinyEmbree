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