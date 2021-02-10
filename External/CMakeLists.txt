set(EMBREE_ISPC_SUPPORT           OFF CACHE BOOL " " FORCE)

# Disable all unused features to shorten build times
set(EMBREE_TUTORIALS              OFF CACHE BOOL " " FORCE)
set(EMBREE_FILTER_FUNCTION        OFF CACHE BOOL " " FORCE)
set(EMBREE_IGNORE_CMAKE_CXX_FLAGS OFF CACHE BOOL " " FORCE)
set(EMBREE_GEOMETRY_QUAD          OFF CACHE BOOL " " FORCE)
set(EMBREE_GEOMETRY_CURVE         OFF CACHE BOOL " " FORCE)
set(EMBREE_GEOMETRY_GRID          OFF CACHE BOOL " " FORCE)
set(EMBREE_GEOMETRY_SUBDIVISION   OFF CACHE BOOL " " FORCE)
set(EMBREE_GEOMETRY_INSTANCE      OFF CACHE BOOL " " FORCE)
set(EMBREE_GEOMETRY_USER          OFF CACHE BOOL " " FORCE)

# We don't bother compiling and linking TBB
set(EMBREE_TASKING_SYSTEM         "INTERNAL" CACHE STRING " " FORCE)

# Enable only AVX2 so we can build a static lib for macOS (also faster build overall)
set(EMBREE_MAX_ISA                "NONE" CACHE STRING " " FORCE)
set(EMBREE_ISA_AVX2               ON  CACHE BOOL " " FORCE)
set(EMBREE_ISA_SSE2               OFF CACHE BOOL " " FORCE)
set(EMBREE_ISA_SSE42              OFF CACHE BOOL " " FORCE)
set(EMBREE_ISA_AVX512             OFF CACHE BOOL " " FORCE)
set(EMBREE_ISA_AVX                OFF CACHE BOOL " " FORCE)

# set(EMBREE_STATIC_LIB             ON  CACHE BOOL " " FORCE)

add_subdirectory(embree)