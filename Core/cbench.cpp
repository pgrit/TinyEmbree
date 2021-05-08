#include "raytrace.h"
#include "point_query.h"

#include <iostream>
#include <chrono>
#include <cstdlib>

Vector3 NextRandom3() {
    return Vector3 {
        (float)std::rand() / (float)RAND_MAX,
        (float)std::rand() / (float)RAND_MAX,
        (float)std::rand() / (float)RAND_MAX,
    };
}

void PinvokeOverhead() {
    // Intersects the same simple quad scene as the C# overhead benchmark.
    // Difference between the two is due to the PInvoke overhead in C#

    void* sceneHandle = InitScene();
    float vertices[] = {
        -1, 0, -1,
         1, 0, -1,
         1, 0,  1,
        -1, 0,  1
    };
    int indices[] = {
        0, 1, 2,
        0, 2, 3
    };
    int meshId = AddTriangleMesh(sceneHandle, vertices, sizeof(vertices) / sizeof(float),
        indices, sizeof(indices) / sizeof(int));
    FinalizeScene(sceneHandle);

    int numTrials = 10;

    // Trace a million random rays
    std::srand(1337);
    std::cout << "Tracing a million rays..." << std::endl;
    auto start = std::chrono::high_resolution_clock::now();

    for (int k = 0; k < numTrials; ++k) {
        for (int i = 0; i < 1000000; ++i) {
            Ray ray {
                NextRandom3(),
                NextRandom3(),
                0.0f
            };
            Hit hit;
            TraceSingle(sceneHandle, &ray, &hit);
        }
    }

    auto end = std::chrono::high_resolution_clock::now();
    auto elapsedMs = std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count();
    std::cout << "Done after " << elapsedMs / numTrials << "ms." << std::endl;
    long long totalCost = elapsedMs / numTrials;

    // Extract the random number overhead
    start = std::chrono::high_resolution_clock::now();
    for (int k = 0; k < numTrials; ++k) {
        for (int i = 0; i < 1000000; ++i) {
            NextRandom3();
            NextRandom3();
        }
    }
    end = std::chrono::high_resolution_clock::now();
    elapsedMs = std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count();
    std::cout << "RNG overhead " << elapsedMs / numTrials << "ms." << std::endl;
    long long rngCost = elapsedMs / numTrials;

    std::cout << "Pure cost for tracing + overhead: " << totalCost - rngCost << "ms." << std::endl;

    DeleteScene(sceneHandle);
}

void KnnTest() {
    std::cout << "Starting simple knn query" << std::endl;

    tinyembree::PointQuery query;
    std::vector<Point> coords;
    for (int i = 0; i < 100; ++i) {
        coords.push_back({3.f * i, 3.f * i + 1, 3.f * i + 2});
    }

    query.SetPoints(coords.data(), coords.size() / 3);

    tinyembree::KNNResult result { 100 };
    Point pos{1, 1, 1};
    query.KnnQuery(&pos, 3, &result);

    std::cout << "Done: " << result.knn.size() << " found" << std::endl;
}

int main() {
    PinvokeOverhead();
    KnnTest();
    return 0;
}