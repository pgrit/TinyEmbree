namespace TinyEmbree;

internal static class TinyEmbreeCore {

#region LINKING_ON_WIN_WORKAROUND
    static TinyEmbreeCore() {
        // Some change in Embree between v3 and v4 causes the Win linker to no longer find tbb12.dll
        // We work around this by first linking to TBB, so the linker will later just re-use the now
        // successfully found tbb12.dll
        // (the proper fix is likely deep within embree's CMake setup...)
        if (System.OperatingSystem.IsWindows())
            NativeLibrary.Load("tbb12.dll", System.Reflection.Assembly.GetExecutingAssembly(), DllImportSearchPath.SafeDirectories);
    }
#endregion LINKING_ON_WIN_WORKAROUND

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    public static extern nint InitScene();

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FinalizeScene(nint scene);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    public static extern int AddTriangleMesh(nint scene, Vector3[] vertices, int numVerts,
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
    public static extern void TraceSingle(nint scene, in Ray ray, out MinimalHitInfo hit);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool IsOccluded(nint scene, in Ray ray, float maxDistance);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DeleteScene(nint scene);
}
