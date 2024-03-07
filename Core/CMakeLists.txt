add_library(TinyEmbreeCore SHARED)

if (WIN32)
    target_compile_definitions(TinyEmbreeCore INTERFACE TINY_EMBREE_DLL)
    target_compile_definitions(TinyEmbreeCore PRIVATE TINY_EMBREE_DLL TINY_EMBREE_EXPORTS)
endif()

# Load the prebuilt binaries and headers for Embree and TBB
if(WIN32)
    set(CMAKE_PREFIX_PATH ${CMAKE_SOURCE_DIR}/prebuilt/win CACHE PATH " ")
elseif(APPLE)
    if (CMAKE_OSX_ARCHITECTURES MATCHES "arm64")
        set(CMAKE_PREFIX_PATH ${CMAKE_SOURCE_DIR}/prebuilt/osx-arm64 CACHE PATH " ")
        # TBB is a universal binary, but Embree is not. Must make sure to keep them appart so we don't confuse
        # CMake and wind up linking to the wrong one.
        set(TBB_DIR ${CMAKE_SOURCE_DIR}/prebuilt/osx/lib/cmake/TBB CACHE PATH " ")
    else()
        set(CMAKE_PREFIX_PATH ${CMAKE_SOURCE_DIR}/prebuilt/osx CACHE PATH " ")
    endif()
else()
    set(CMAKE_PREFIX_PATH ${CMAKE_SOURCE_DIR}/prebuilt/linux CACHE PATH " ")
endif()

find_package(embree 4 REQUIRED)

target_include_directories(TinyEmbreeCore
    PRIVATE
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

# Copy the compiled shared library, and only that, to the runtimes folder
if(WIN32)
    set(RUNTIMES_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../runtimes/win-x64/native/)
elseif(APPLE)
    if (CMAKE_OSX_ARCHITECTURES MATCHES "arm64")
        set(RUNTIMES_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../runtimes/osx-arm64/native/)
    else()
        set(RUNTIMES_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../runtimes/osx-x64/native/)
    endif()
else()
    set(RUNTIMES_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../runtimes/linux-x64/native/)
endif()

add_custom_command(
    TARGET TinyEmbreeCore POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:TinyEmbreeCore> ${RUNTIMES_DIR}
)

# Test / benchmark target
# add_executable(TinyBench)
# target_sources(TinyBench PRIVATE cbench.cpp)
# target_include_directories(TinyBench
#     PRIVATE
#         ${CMAKE_CURRENT_SOURCE_DIR}
#         ${CMAKE_CURRENT_SOURCE_DIR}/../External/embree/include
# )
# target_link_libraries(TinyBench PRIVATE TinyEmbreeCore embree)
# set_target_properties(TinyBench
#     PROPERTIES
#         CXX_STANDARD 11
#         CXX_STANDARD_REQUIRED YES
#         CXX_EXTENSIONS NO
# )