using TinyEmbree;
using System.Diagnostics;
using System.Numerics;
using System;

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

            Stopwatch stop = Stopwatch.StartNew();
            for (int k = 0; k < numTrials; ++k) {
                for (int i = 0; i < 1000000; ++i) {
                    rt.Trace(new Ray {
                        Origin = new Vector3(-0.5f, -10, 0),
                        Direction = new Vector3(0, 1, 0),
                        MinDistance = 1.0f
                    });
                }
            }
            stop.Stop();
            Console.WriteLine($"One million rays intersected in {stop.ElapsedMilliseconds / numTrials}ms");
        }

        static void Main(string[] args) {
            MeasurePinvokeOverhead(10);
        }
    }
}
