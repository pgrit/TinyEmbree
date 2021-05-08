using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace TinyEmbree {
    /// <summary>
    /// Wrapper around some of Embree's ray-triangle intersection functions.
    /// Supports closest-hit and any-hit (shadow rays) with proper offsets to avoid
    /// self intersections.
    /// </summary>
    public class Raytracer : IDisposable {
        IntPtr scene;
        bool isReady = false;

        void Free() {
            if (scene != IntPtr.Zero) {
                TinyEmbreeCore.DeleteScene(scene);
                scene = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Releases the unmanaged Embree resources
        /// </summary>
        ~Raytracer() => Free();

        /// <summary>
        /// Releases the unmanaged Embree resources
        /// </summary>
        public void Dispose() {
            Free();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new, empty, scene with an Embree BVH
        /// </summary>
        public Raytracer() {
            scene = TinyEmbreeCore.InitScene();
        }

        /// <summary>
        /// Adds a triangle mesh to the scene. If the mesh is modified after it was added,
        /// the modifications will NOT be visible to Embree.
        /// </summary>
        /// <param name="mesh">A triangle mesh</param>
        public void AddMesh(TriangleMesh mesh) {
            uint meshId = (uint)TinyEmbreeCore.AddTriangleMesh(scene, mesh.Vertices, mesh.NumVertices,
                mesh.Indices, mesh.NumFaces * 3);
            meshMap[meshId] = mesh;
        }

        /// <summary>
        /// Builds the acceleration structure
        /// </summary>
        public void CommitScene() {
            TinyEmbreeCore.FinalizeScene(scene);
            isReady = true;
        }

        /// <summary>
        /// Traces a ray and finds the closest hit point (if any)
        /// </summary>
        /// <param name="ray">A ray</param>
        /// <returns>The closest hit point or an invalid hit point</returns>
        public Hit Trace(Ray ray) {
            Debug.Assert(isReady);

            TinyEmbreeCore.TraceSingle(scene, in ray, out var minHit);

            if (minHit.meshId == uint.MaxValue)
                return new Hit();

            Hit hit = new() {
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

        static Vector3 OffsetPoint(Hit from, Vector3 dir) {
            float sign = Vector3.Dot(dir, from.Normal) < 0.0f ? -1.0f : 1.0f;
            return from.Position + sign * from.ErrorOffset * from.Normal;
        }

        /// <summary>
        /// Creates a shadow ray that connects two surface points 
        /// (with proper offsets agains self intersection)
        /// </summary>
        /// <param name="from">The first surface point</param>
        /// <param name="to">The second surface point</param>
        /// <returns>A shadow ray that is ready to be traced</returns>
        public static ShadowRay MakeShadowRay(Hit from, Hit to) {
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

        /// <summary>
        /// Creates a shadow ray that connects two surface points 
        /// (with proper offsets agains self intersection)
        /// </summary>
        /// <param name="from">The first surface point</param>
        /// <param name="target">
        ///     A position in the scene without surface information (higher risk of self-intersection)
        /// </param>
        /// <returns>A shadow ray that is ready to be traced</returns>
        public static ShadowRay MakeShadowRay(Hit from, Vector3 target) {
            // Compute the ray with proper offsets
            var dir = target - from.Position;
            var dist = dir.Length();
            dir /= dist;
            return new ShadowRay(SpawnRay(from, dir), dist - from.ErrorOffset);
        }

        /// <summary>
        /// Generates a shadow ray from a surface point that leaves the scene in a given direction
        /// </summary>
        /// <param name="from">The surface point</param>
        /// <param name="direction">Direction</param>
        /// <returns>A shadow ray that is ready to be traced</returns>
        public static ShadowRay MakeBackgroundShadowRay(Hit from, Vector3 direction) {
            var ray = SpawnRay(from, direction);
            return new ShadowRay(ray, float.MaxValue);
        }

        /// <summary>
        /// Checks if the given shadow ray is occluded
        /// </summary>
        /// <param name="ray">The shadow ray</param>
        /// <returns>True if the shadow ray is occluded</returns>
        public bool IsOccluded(ShadowRay ray) {
            Debug.Assert(isReady);
            return TinyEmbreeCore.IsOccluded(scene, in ray.Ray, ray.MaxDistance);
        }

        /// <summary>
        /// Convenience wrapper that calls <see cref="MakeShadowRay(Hit, Hit)"/> and then
        /// <see cref="IsOccluded(ShadowRay)"/>.
        /// </summary>
        /// <returns>True if the shadow ray is occluded</returns>
        public bool IsOccluded(Hit from, Hit to) {
            var ray = MakeShadowRay(from, to);
            return IsOccluded(ray);
        }

        /// <summary>
        /// Convenience wrapper that calls <see cref="MakeShadowRay(Hit, Vector3)"/> and then
        /// <see cref="IsOccluded(ShadowRay)"/>.
        /// </summary>
        /// <returns>True if the shadow ray is occluded</returns>
        public bool IsOccluded(Hit from, Vector3 target) {
            var ray = MakeShadowRay(from, target);
            return IsOccluded(ray);
        }

        /// <summary>
        /// Convenience wrapper that calls <see cref="MakeBackgroundShadowRay"/> and then
        /// <see cref="IsOccluded(ShadowRay)"/>.
        /// </summary>
        /// <returns>True if the shadow ray is occluded</returns>
        public bool LeavesScene(Hit from, Vector3 direction) {
            var ray = MakeBackgroundShadowRay(from, direction);
            return IsOccluded(ray);
        }

        /// <summary>
        /// Generates a ray starting at a surface point, with the right offset to avoid self-intersection.
        /// </summary>
        /// <param name="from">The surface point that will be the ray origin</param>
        /// <param name="dir">Direction of the ray</param>
        /// <returns>A ray that is ready to be traced</returns>
        public static Ray SpawnRay(Hit from, Vector3 dir) {
            float sign = Vector3.Dot(dir, from.Normal) < 0.0f ? -1.0f : 1.0f;
            return new Ray {
                Origin = from.Position + sign * from.ErrorOffset * from.Normal,
                Direction = dir,
                MinDistance = from.ErrorOffset,
            };
        }

        readonly SortedList<uint, TriangleMesh> meshMap = new();
    }
}
