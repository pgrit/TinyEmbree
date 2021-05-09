add_library(TinyEmbreeCore SHARED)

if (WIN32)
    target_compile_definitions(TinyEmbreeCore INTERFACE TINY_EMBREE_DLL)
    target_compile_definitions(TinyEmbreeCore PRIVATE TINY_EMBREE_EXPORTS)
endif()

target_include_directories(TinyEmbreeCore
    PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/../External/embree/include
        ${CMAKE_CURRENT_SOURCE_DIR}
)

target_sources(TinyEmbreeCore
    PUBLIC
        "api.h"
        "raytrace.h"
        "knn.h"

    PRIVATE
        "common.h"

        "raytrace.cpp"
        "scene.h"
        "scene.cpp"

        "knn.cpp"
        "point_query.h"
)

# The embree shared library will be in the same folder
if(WIN32)
elseif(APPLE)
    set_target_properties(TinyEmbreeCore
        PROPERTIES
            INSTALL_RPATH "@loader_path"
            BUILD_WITH_INSTALL_RPATH ON)
else()
    set_target_properties(TinyEmbreeCore
        PROPERTIES
            INSTALL_RPATH "$ORIGIN"
            BUILD_WITH_INSTALL_RPATH ON)
endif()

target_link_libraries(TinyEmbreeCore PRIVATE embree)

set_target_properties(TinyEmbreeCore
    PROPERTIES
        CXX_STANDARD 11
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

add_custom_command(
    TARGET TinyEmbreeCore POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy
        $<TARGET_FILE:TinyEmbreeCore>
        ${CMAKE_CURRENT_SOURCE_DIR}/../Runtimes/ )

# Test / benchmark target
add_executable(TinyBench)
target_sources(TinyBench PRIVATE cbench.cpp)
target_include_directories(TinyBench
    PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}
        ${CMAKE_CURRENT_SOURCE_DIR}/../External/embree/include
)
target_link_libraries(TinyBench PRIVATE TinyEmbreeCore embree)
set_target_properties(TinyBench
    PROPERTIES
        CXX_STANDARD 11
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)