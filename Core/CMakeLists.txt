add_library(TinyEmbreeCore SHARED)

find_package(embree 3.0 CONFIG REQUIRED)
find_package(TBB CONFIG REQUIRED)

if (WIN32)
    add_compile_definitions(TINY_EMBREE_DLL TINY_EMBREE_EXPORTS)
endif()

target_include_directories(TinyEmbreeCore PRIVATE ${EMBREE_INCLUDE_DIRS})
target_include_directories(TinyEmbreeCore PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})

target_sources(TinyEmbreeCore
    PUBLIC
        "raytrace.h"

    PRIVATE
        "raytrace.cpp"
        "scene.h"
        "scene.cpp"
)

target_link_libraries(TinyEmbreeCore PRIVATE embree TBB::tbb)
set_target_properties(TinyEmbreeCore PROPERTIES
                      RUNTIME_OUTPUT_DIRECTORY_DEBUG ${CMAKE_CURRENT_SOURCE_DIR}/../Dist
                      RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_CURRENT_SOURCE_DIR}/../Dist
                      RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO ${CMAKE_CURRENT_SOURCE_DIR}/../Dist
                      LIBRARY_OUTPUT_DIRECTORY_DEBUG ${CMAKE_CURRENT_SOURCE_DIR}/../Dist
                      LIBRARY_OUTPUT_DIRECTORY_RELEASE ${CMAKE_CURRENT_SOURCE_DIR}/../Dist
                      LIBRARY_OUTPUT_DIRECTORY_RELWITHDEBINFO ${CMAKE_CURRENT_SOURCE_DIR}/../Dist
)

