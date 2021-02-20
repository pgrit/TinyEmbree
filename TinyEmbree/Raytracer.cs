using System;
using System.Collections.Generic;
using System.Numerics;

namespace TinyEmbree {
    public class Raytracer : IDisposable {
        IntPtr scene;

        protected void Free() {
            if (scene != IntPtr.Zero) {
                TinyEmbreeCore.DeleteScene(scene);
                scene = IntPtr.Zero;
            }
        }

        ~Raytracer() => Free();
        public void Dispose() => Free();

        public Raytracer() {
            scene = TinyEmbreeCore.InitScene();
        }

        public void AddMesh(TriangleMesh mesh) {
            uint meshId = (uint)TinyEmbreeCore.AddTriangleMesh(scene, mesh.Vertices, mesh.NumVertices,
                mesh.Indices, mesh.NumFaces * 3);
            meshMap[meshId] = mesh;
        }

        public void CommitScene() {
            TinyEmbreeCore.FinalizeScene(scene);
        }

        public Hit Trace(Ray ray) {
            TinyEmbreeCore.MinimalHitInfo minHit;
            TinyEmbreeCore.TraceSingle(scene, in ray, out minHit);

            if (minHit.meshId == uint.MaxValue)
                return new Hit();

            Hit hit = new Hit {
                BarycentricCoords = new Vector2(minHit.u, minHit.v),
                Distance = minHit.distance,
                Mesh = meshMap[minHit.meshId],
                PrimId = minHit.primId
            };

            // Compute the position and face normal from the barycentric coordinates
            hit.Position = hit.Mesh.ComputePosition((int)hit.PrimId, hit.BarycentricCoords);
            hit.Normal = hit.Mesh.FaceNormals[hit.PrimId];

            // Compute the error offset (formula taken from Embree example renderer)
            hit.ErrorOffset = Math.Max(
                Math.Max(Math.Abs(hit.Position.X), Math.Abs(hit.Position.Y)),
                Math.Max(Math.Abs(hit.Position.Z), hit.Distance)
            ) * 32.0f * 1.19209e-07f;

            return hit;
        }

        Vector3 OffsetPoint(Hit from, Vector3 dir) {
            float sign = Vector3.Dot(dir, from.Normal) < 0.0f ? -1.0f : 1.0f;
            return from.Position + sign * from.ErrorOffset * from.Normal;
        }

        public ShadowRay MakeShadowRay(Hit from, Hit to) {
            const float shadowEpsilon = 1e-5f;

            // Compute the ray with proper offsets
            var dir = to.Position - from.Position;
            var p0 = OffsetPoint(from, dir);
            var p1 = OffsetPoint(to, -dir);
            dir = p1 - p0;

            var ray = new Ray {
                Origin = p0,
                Direction = dir,
                MinDistance = shadowEpsilon,
            };

            return new ShadowRay(ray, 1 - shadowEpsilon);
        }

        public ShadowRay MakeShadowRay(Hit from, Vector3 target) {
            // Compute the ray with proper offsets
            var dir = target - from.Position;
            var dist = dir.Length();
            dir /= dist;
            return new ShadowRay(SpawnRay(from, dir), dist - from.ErrorOffset);
        }

        public ShadowRay MakeBackgroundShadowRay(Hit from, Vector3 direction) {
            var ray = SpawnRay(from, direction);
            return new ShadowRay(ray, float.MaxValue);
        }

        public bool IsOccluded(ShadowRay ray)
        => TinyEmbreeCore.IsOccluded(scene, in ray.Ray, ray.MaxDistance);

        public bool IsOccluded(Hit from, Hit to) {
            var ray = MakeShadowRay(from, to);
            return IsOccluded(ray);
        }

        public bool IsOccluded(Hit from, Vector3 target) {
            var ray = MakeShadowRay(from, target);
            return IsOccluded(ray);
        }

        public bool LeavesScene(Hit from, Vector3 direction) {
            var ray = MakeBackgroundShadowRay(from, direction);
            // TODO use a proper optimized method here that does not compute the actual closest hit.
            var p = Trace(ray.Ray);
            bool occluded = p.Mesh != null;
            return !occluded;
        }

        public Ray SpawnRay(Hit from, Vector3 dir) {
            float sign = Vector3.Dot(dir, from.Normal) < 0.0f ? -1.0f : 1.0f;
            return new Ray {
                Origin = from.Position + sign * from.ErrorOffset * from.Normal,
                Direction = dir,
                MinDistance = from.ErrorOffset,
            };
        }

        SortedList<uint, TriangleMesh> meshMap = new();
    }
}
