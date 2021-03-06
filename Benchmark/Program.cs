﻿using System;

namespace TinyEmbree.Benchmark {
    class Program {
        static void Main(string[] args) {
            NearestNeighborBench<NearestNeighborSearch>.Benchmark_10_Nearest(2, true, new());

            Console.WriteLine("Tracing rays in a typical test scene...");
            RayTracing.ComplexScene(10);

            Console.WriteLine("Tracing rays with a single polygon...");
            RayTracing.MeasurePinvokeOverhead(10);
        }
    }
}
