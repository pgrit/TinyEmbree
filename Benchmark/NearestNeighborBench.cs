using TinyEmbree.Benchmark;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace TinyEmbree.Benchmark;

class NearestNeighborBench<T> where T : NearestNeighborSearch {
    NearestNeighborBench(T accel) { this.accel = accel; }

    void GenerateDataSet(int numElements, float scale) {
        points = new List<Vector3>(numElements);
        rng = new Random();
        for (int i = 0; i < numElements; ++i)
            points.Add(RandomPoint(scale));
    }

    Vector3 RandomPoint(float scale) {
        var x = (float)rng.NextDouble() * scale * 2 - scale;
        var y = (float)rng.NextDouble() * scale * 2 - scale;
        var z = (float)rng.NextDouble() * scale * 2 - scale;
        return new Vector3(x, y, z);
    }

    void AddData() {
        accel.Clear();
        for (int i = 0; i < points.Count; ++i) {
            accel.AddPoint(points[i], i);
        }
    }

    long BuildAccelerationStructure() {
        var stop = Stopwatch.StartNew();
        accel.Build();
        stop.Stop();
        return stop.ElapsedMilliseconds;
    }

    bool QueryAndValidate(int numQueries, int k, float r, float scale) {
        bool valid = true;
        for (int j = 0; j < numQueries; ++j) {
            var p = RandomPoint(scale);
            var groundTruth = BruteForceNearest(p, k, r);

            var result = accel.QueryNearest(p, k, r, out var maxR);

            // Validate against brute-force
            float gtMax = 0.0f;
            for (int i = 0; i < k; ++i) {
                Debug.Assert(result[i] == groundTruth[i]);
                if (result[i] != groundTruth[i]) valid = false;
                gtMax = float.Max(gtMax, (points[result[i]] - p).Length());
            }

            if (float.Abs(gtMax - maxR) > 1e-4f) valid = false;
        }
        return valid;
    }

    long Query(int numQueries, int k, float r, float scale) {
        var stop = Stopwatch.StartNew();
        for (int j = 0; j < numQueries; ++j) {
            accel.QueryNearest(RandomPoint(scale), k, r, out var maxR);
        }
        stop.Stop();
        return stop.ElapsedMilliseconds;
    }

    int[] BruteForceNearest(Vector3 p, int k, float r) {
        var result = new List<int>();
        var distances = new List<float>();

        for (int i = 0; i < points.Count; ++i) {
            float distSquared = (points[i] - p).LengthSquared();
            if (MathF.Sqrt(distSquared) > r)
                continue;
            int idx = distances.BinarySearch(distSquared);
            if (idx < 0) idx = ~idx;
            else for (; idx > 0 && distances[idx-1] == distSquared; --idx) { }
            distances.Insert(idx, distSquared);
            result.Insert(idx, i);
        }
        return result.ToArray()[0..k];
    }

    Random rng;
    List<Vector3> points;
    readonly T accel;

    public static void Benchmark_10_Nearest(int numRepeats, bool validate, T accel, int num) {
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        int numPoints = num;
        float scale = 100.0f;
        int numNearest = 10;
        int numQueries = num;

        long buildTime = 0;
        long queryTime = 0;
        for (int i = 0; i < numRepeats; ++i) {
            var bench = new NearestNeighborBench<T>(accel);
            bench.GenerateDataSet(numPoints, scale);
            bench.AddData();

            System.Console.WriteLine("Building acceleration structure...");
            buildTime += bench.BuildAccelerationStructure();

            if (validate && !bench.QueryAndValidate(10, numNearest, float.MaxValue, scale))
                System.Console.WriteLine("Validation FAILED: results differ from brute force ground truth!");

            System.Console.WriteLine("Querying 10 nearest neighbors...");
            queryTime += bench.Query(numQueries, numNearest, float.MaxValue, scale);
        }

        System.Console.WriteLine($"Building with {numPoints} points took {buildTime / (float)numRepeats}ms on average.");
        System.Console.WriteLine($"Querying {numQueries} times took {queryTime / (float)numRepeats}ms on average.");
    }
}