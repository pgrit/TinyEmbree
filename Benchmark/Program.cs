using TinyEmbree;
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

namespace Benchmark {
    class Program {
        static void MeasurePinvokeOverhead(int numTrials) {
            var vertices = new Vector3[] {
                new Vector3(-1, 0, -1),
                new Vector3( 1, 0, -1),
                new Vector3( 1, 0,  1),
                new Vector3(-1, 0,  1)
            };

            var indices = new int[] {
                0, 1, 2,
                0, 2, 3
            };

            TriangleMesh mesh = new TriangleMesh(vertices, indices);

            var rt = new Raytracer();
            rt.AddMesh(mesh);
            rt.CommitScene();

            Random rng = new(1337);
            Stopwatch stop = Stopwatch.StartNew();
            for (int k = 0; k < numTrials; ++k) {
                for (int i = 0; i < 1000000; ++i) {
                    rt.Trace(new Ray {
                        Origin = new Vector3 (
                            (float) rng.NextDouble(),
                            (float) rng.NextDouble(),
                            (float) rng.NextDouble()),
                        Direction = new Vector3 (
                            (float) rng.NextDouble(),
                            (float) rng.NextDouble(),
                            (float) rng.NextDouble()),
                        MinDistance = 0.0f
                    });
                }
            }
            stop.Stop();
            Console.WriteLine($"One million rays intersected in {stop.ElapsedMilliseconds / numTrials}ms");
            long traceCost = stop.ElapsedMilliseconds / numTrials;

            stop = Stopwatch.StartNew();
            for (int k = 0; k < numTrials; ++k) {
                for (int i = 0; i < 1000000; ++i) {
                    new Vector3 (
                        (float) rng.NextDouble(),
                        (float) rng.NextDouble(),
                        (float) rng.NextDouble()
                    );
                    new Vector3 (
                        (float) rng.NextDouble(),
                        (float) rng.NextDouble(),
                        (float) rng.NextDouble()
                    );
                }
            }
            stop.Stop();
            Console.WriteLine($"RNG overhead: {stop.ElapsedMilliseconds / numTrials}ms");
            long rngCost = stop.ElapsedMilliseconds / numTrials;

            Console.WriteLine($"Pure cost for tracing + overhead: {traceCost-rngCost}ms");
        }

        static void ComplexScene(int numTrials) {
            // the scene is compressed to avoid git issues
            if (!File.Exists("../Data/breakfast_room.obj"))
                ZipFile.ExtractToDirectory("../Data/breakfast_room.zip", "../Data");

            Stopwatch stop = Stopwatch.StartNew();
            List<TriangleMesh> meshes = new();
            Vector3 min = Vector3.One * float.MaxValue;
            Vector3 max = -Vector3.One * float.MaxValue;
            Assimp.AssimpContext context = new();
            var scene = context.ImportFile("../Data/breakfast_room.obj",
                Assimp.PostProcessSteps.GenerateNormals | Assimp.PostProcessSteps.JoinIdenticalVertices |
                Assimp.PostProcessSteps.PreTransformVertices | Assimp.PostProcessSteps.Triangulate);
            foreach (var m in scene.Meshes) {
                var material = scene.Materials[m.MaterialIndex];
                string materialName = material.Name;

                Vector3[] vertices = new Vector3[m.VertexCount];
                for (int i = 0; i < m.VertexCount; ++i)
                    vertices[i] = new(m.Vertices[i].X, m.Vertices[i].Y, m.Vertices[i].Z);

                meshes.Add(new(vertices, m.GetIndices()));

                min.X = MathF.Min(min.X, m.BoundingBox.Min.X);
                min.Y = MathF.Min(min.X, m.BoundingBox.Min.Y);
                min.Z = MathF.Min(min.X, m.BoundingBox.Min.Z);

                max.X = MathF.Max(max.X, m.BoundingBox.Max.X);
                max.Y = MathF.Max(max.X, m.BoundingBox.Max.Y);
                max.Z = MathF.Max(max.X, m.BoundingBox.Max.Z);
            }

            var diagonal = max - min;

            Console.WriteLine($"Scene loaded in {stop.ElapsedMilliseconds}ms");
            stop.Restart();

            var rt = new Raytracer();
            foreach (var m in meshes) rt.AddMesh(m);
            rt.CommitScene();

            Console.WriteLine($"Acceleration structures built in {stop.ElapsedMilliseconds}ms");

            Random rng = new(1337);
            Vector3 NextVector() => new Vector3 (
                (float) rng.NextDouble(),
                (float) rng.NextDouble(),
                (float) rng.NextDouble());

            stop.Restart();
            float averageDistance = 0;
            for (int k = 0; k < numTrials; ++k) {
                for (int i = 0; i < 1000000; ++i) {
                    var hit = rt.Trace(new Ray {
                        Origin = NextVector() * diagonal + min,
                        Direction = NextVector(),
                        MinDistance = 0.0f
                    });

                    if (hit)
                        averageDistance += hit.Distance / 1000000 / numTrials;
                }
            }
            stop.Stop();
            Console.WriteLine($"One million closest hits found in {stop.ElapsedMilliseconds / numTrials}ms");
            Console.WriteLine($"Average distance: {averageDistance}");

            stop.Restart();
            float averageVisibility = 0;
            for (int k = 0; k < numTrials; ++k) {
                for (int i = 0; i < 1000000; ++i) {
                    bool occluded = rt.IsOccluded(new ShadowRay(new Ray {
                        Origin = NextVector() * diagonal + min,
                        Direction = NextVector(),
                        MinDistance = 0.0f
                    }, maxDistance: (float) rng.NextDouble() * averageDistance * 5));

                    if (!occluded) averageVisibility += 1.0f / 1000000.0f / numTrials;
                }
            }
            stop.Stop();
            Console.WriteLine($"One million any hits found in {stop.ElapsedMilliseconds / numTrials}ms");
            Console.WriteLine($"Average visibility: {averageVisibility}");
        }

        static void Main(string[] args) {
            Console.WriteLine("Tracing rays in a typical test scene...");
            ComplexScene(10);
            Console.WriteLine("Tracing rays with a single polygon...");
            MeasurePinvokeOverhead(10);
        }
    }
}
