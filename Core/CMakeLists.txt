add_library(TinyEmbreeCore SHARED)

find_package(embree 3.0 CONFIG REQUIRED)

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

# libtbb and libembree are located in the same folder
IF(WIN32)
ELSEIF(APPLE)
  SET_TARGET_PROPERTIES(TinyEmbreeCore PROPERTIES INSTALL_RPATH "@rpath")
ELSE()
  SET_TARGET_PROPERTIES(TinyEmbreeCore PROPERTIES INSTALL_RPATH "$ORIGIN")
ENDIF()

target_link_libraries(TinyEmbreeCore PRIVATE embree)

set_target_properties(TinyEmbreeCore PROPERTIES
                      RUNTIME_OUTPUT_DIRECTORY_DEBUG ${CMAKE_CURRENT_SOURCE_DIR}/../Runtime
                      RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_CURRENT_SOURCE_DIR}/../Runtime
                      RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO ${CMAKE_CURRENT_SOURCE_DIR}/../Runtime
                      LIBRARY_OUTPUT_DIRECTORY_DEBUG ${CMAKE_CURRENT_SOURCE_DIR}/../Runtime
                      LIBRARY_OUTPUT_DIRECTORY_RELEASE ${CMAKE_CURRENT_SOURCE_DIR}/../Runtime
                      LIBRARY_OUTPUT_DIRECTORY_RELWITHDEBINFO ${CMAKE_CURRENT_SOURCE_DIR}/../Runtime
)

set_target_properties(TinyEmbreeCore
    PROPERTIES
        CXX_STANDARD 11
        CXX_STANDARD_REQUIRED YES
        CXX_EXTENSIONS NO
)

install(TARGETS TinyEmbreeCore
    DESTINATION ${CMAKE_CURRENT_SOURCE_DIR}/../Runtime
)