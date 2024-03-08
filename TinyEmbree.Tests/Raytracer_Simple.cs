using TinyEmbree;
using System.Numerics;
using Xunit;

namespace SeeSharp.Core.Tests.Geometry {
    public class Raytracer_Simple {
        [Fact]
        public void SimpleQuad_ShouldBeIntersected() {

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

            TriangleMesh mesh = new(vertices, indices);

            using var rt = new Raytracer();
            rt.AddMesh(mesh);
            rt.CommitScene();

            Hit hit = rt.Trace(new Ray {
                Origin = new Vector3(-0.5f, -10, 0),
                Direction = new Vector3(0, 1, 0),
                MinDistance = 1.0f
            });

            Assert.Equal(10.0f, hit.Distance, 0);
            Assert.Equal(1u, hit.PrimId);
            Assert.Equal(mesh, hit.Mesh);
            Assert.Equal(1u, rt.Stats.NumRays);
            Assert.Equal(1u, rt.Stats.NumRayHits);
        }

        [Fact]
        public void SimpleQuad_ShadowRay() {

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

            TriangleMesh mesh = new(vertices, indices);

            using var rt = new Raytracer();
            rt.AddMesh(mesh);
            rt.CommitScene();

            Hit a = new() {
                Position = new Vector3(-0.5f, -10, 0),
            };

            Hit b = new() {
                Position = new Vector3(-0.5f, 10, 0),
            };

            Hit c = new() {
                Position = new Vector3(-0.5f, -20, 0),
            };

            Assert.True(rt.IsOccluded(a, b));
            Assert.True(rt.IsOccluded(b, a));
            Assert.True(!rt.IsOccluded(a, c));
            Assert.True(!rt.IsOccluded(c, a));
        }

        [Fact]
        public void SimpleQuad_ShouldBeMissed() {
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

            TriangleMesh mesh = new(vertices, indices);

            using Raytracer rt = new();
            rt.AddMesh(mesh);
            rt.CommitScene();

            Hit hit = rt.Trace(new Ray {
                Origin = new Vector3(-0.5f, -10, 0),
                Direction = new Vector3(0, -1, 0),
                MinDistance = 1.0f
            });

            Assert.False(hit);
            Assert.Equal(1u, rt.Stats.NumRays);
            Assert.Equal(0u, rt.Stats.NumRayHits);
        }
    }
}
