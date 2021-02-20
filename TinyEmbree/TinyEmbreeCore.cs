using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TinyEmbree {
    internal static class TinyEmbreeCore {
        [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InitScene();

        [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FinalizeScene(IntPtr scene);

        [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AddTriangleMesh(IntPtr scene, Vector3[] vertices, int numVerts,
                                                 int[] indices, int numIdx, float[] texCoords = null,
                                                 float[] shadingNormals = null);

#pragma warning disable CS0649 // The field is never assigned to (only returned from native function call)
        public readonly struct MinimalHitInfo {
            public readonly uint meshId;
            public readonly uint primId;
            public readonly float u;
            public readonly float v;
            public readonly float distance;
        }
#pragma warning restore CS0649 // The field is never assigned to

        [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TraceSingle(IntPtr scene, in Ray ray, out MinimalHitInfo hit);

        [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsOccluded(IntPtr scene, in Ray ray, float maxDistance);

        [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteScene(IntPtr scene);
    }
}
