add_library(TinyEmbreeCore SHARED)

# find_package(embree 3.0 CONFIG REQUIRED)

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

# The embree shared library will be in the same folder
if(WIN32)
elseif(APPLE)
    set(CMAKE_MACOSX_RPATH ON)
    set(CMAKE_BUILD_RPATH "@rpath")
else()
    set(CMAKE_BUILD_RPATH "$ORIGIN")
endif()

message(${CMAKE_BUILD_RPATH})

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