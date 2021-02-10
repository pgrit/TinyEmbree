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

# Enable only AVX2 for faster github actions deployment
set(EMBREE_MAX_ISA                "NONE" CACHE STRING " " FORCE)
set(EMBREE_ISA_AVX2               ON  CACHE BOOL " " FORCE)
set(EMBREE_ISA_SSE2               OFF CACHE BOOL " " FORCE)
set(EMBREE_ISA_SSE42              OFF CACHE BOOL " " FORCE)
set(EMBREE_ISA_AVX512             OFF CACHE BOOL " " FORCE)
set(EMBREE_ISA_AVX                OFF CACHE BOOL " " FORCE)

add_subdirectory(embree)

get_directory_property(EMBREE_VERSION_MAJOR
    DIRECTORY embree
    DEFINITION EMBREE_VERSION_MAJOR)

IF (WIN32)
    SET(embree_filename ${CMAKE_IMPORT_LIBRARY_PREFIX}${EMBREE_LIBRARY_NAME}${CMAKE_IMPORT_LIBRARY_SUFFIX})
ELSEIF (APPLE)
    SET(embree_filename ${CMAKE_SHARED_LIBRARY_PREFIX}${EMBREE_LIBRARY_NAME}.${EMBREE_VERSION_MAJOR}${CMAKE_SHARED_LIBRARY_SUFFIX})
ELSE()
    SET(embree_filename ${CMAKE_SHARED_LIBRARY_PREFIX}${EMBREE_LIBRARY_NAME}${CMAKE_SHARED_LIBRARY_SUFFIX}.${EMBREE_VERSION_MAJOR})
ENDIF()

message(${embree_filename})

# Create a command to copy the file, and a target that depends on the copy
add_custom_command(
    OUTPUT ${CMAKE_CURRENT_SOURCE_DIR}/../Runtimes/${embree_filename}
    COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:embree> ${CMAKE_CURRENT_SOURCE_DIR}/../Runtimes/${embree_filename}
    DEPENDS embree
)
add_custom_target(copy_files ALL
    DEPENDS ${CMAKE_CURRENT_SOURCE_DIR}/../Runtimes/${embree_filename}
)