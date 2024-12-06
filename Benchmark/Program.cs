using System;
using TinyEmbree;
using TinyEmbree.Benchmark;

NearestNeighborBench<NearestNeighborSearch<int>>.Benchmark_10_Nearest(2, true, new(), 100000);

Console.WriteLine("Tracing rays in a typical test scene...");
RayTracing.ComplexScene(10);

Console.WriteLine("Tracing rays with a single polygon...");
RayTracing.MeasurePinvokeOverhead(10);