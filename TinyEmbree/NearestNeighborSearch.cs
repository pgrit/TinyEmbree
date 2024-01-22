namespace TinyEmbree;

/// <summary>
/// Provides a k nearest neighbor search with maximum radius.
/// Implemented via user geometry with Embree.
/// </summary>
public class NearestNeighborSearch : IDisposable {
    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    static extern nint NewKnnAccelerator();

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    static extern void ReleaseKnnAccelerator(nint accelerator);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    static extern void SetKnnPoints(nint accelerator, nint data, uint numPoints);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    static extern nint NewKnnQueryCache();

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    static extern void ReleaseKnnQueryCache(nint cache);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    static extern nint KnnQuery(nint accelerator, nint cache, in Vector3 pos, float radius, uint k,
        out uint numFound, out float furthest);

    [StructLayout(LayoutKind.Sequential)]
    struct Neighbor {
        public uint id;
        public float distance;
    }

    readonly List<Vector3> positions = new();
    readonly List<int> ids = new();

    nint accel;
    bool isBuilt;
    GCHandle positionBuffer;
    readonly ThreadLocal<nint> queryCache = new(() => NewKnnQueryCache(), trackAllValues: true);

    /// <summary>
    /// Prepares a new BVH for kNN search
    /// </summary>
    public NearestNeighborSearch() {
        accel = NewKnnAccelerator();
    }

    /// <summary>
    /// Only called if Dispose was not invoked. This will lead to a memory leak since the finalizer order
    /// is undefined. The ThreadLocal caches may or may not already be disposed, so there is no way to
    /// access their native memory pointers anymore.
    /// </summary>
    ~NearestNeighborSearch() {
        Console.WriteLine("MEMORY LEAK: NearestNeighborSearch Finalizer called by GC, " +
            "thread-local data not disposed correctly. Use Dispose().");
    }

    /// <summary>
    /// Releases the unmanaged resources
    /// </summary>
    public void Dispose() {
        if (accel != nint.Zero) {
            ReleaseKnnAccelerator(accel);
            accel = nint.Zero;

            foreach (var cache in queryCache.Values) {
                ReleaseKnnQueryCache(cache);
            }
        }

        if (positionBuffer.IsAllocated)
            positionBuffer.Free();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Adds a new point to the cache. Position and id are tracked in an internal
    /// list. Thread-safe: multiple adds are supported in parallel, but never add and
    /// query or build the same structure at the same time!
    /// </summary>
    /// <param name="position">Position of the point</param>
    /// <param name="userId">ID that will be reported if the point is found in a query</param>
    public void AddPoint(Vector3 position, int userId) {
        lock (this) {
            positions.Add(position);
            ids.Add(userId);
            isBuilt = false;
        }
    }

    /// <summary>
    /// Builds the acceleration structure based on the points added via <see cref="AddPoint(Vector3, int)"/>
    /// </summary>
    public void Build() {
        if (positionBuffer.IsAllocated)
            positionBuffer.Free();
        positionBuffer = GCHandle.Alloc(positions.ToArray(), GCHandleType.Pinned);
        SetKnnPoints(accel, positionBuffer.AddrOfPinnedObject(), (uint)positions.Count);
        isBuilt = true;
    }

    /// <summary>
    /// Removes all points from the acceleration structure so it can be filled with a new set.
    /// </summary>
    public void Clear() {
        if (positionBuffer.IsAllocated)
            positionBuffer.Free();
        positions.Clear();
        ids.Clear();
        isBuilt = false;
    }

    /// <summary>
    /// Queries the k nearest neighbors within the given radius. Thread-safe: multiple queries
    /// can be performed by different threads lock-free.
    /// </summary>
    /// <param name="position">The query location</param>
    /// <param name="maxCount">Maximum number of neighbors to find ("k")</param>
    /// <param name="maxRadius">Maximum distance between any neighbor and the query point</param>
    /// <param name="distToFurthest">Distance of the furthest away point to the query location</param>
    /// <returns>
    /// Array of user IDs of the maxCount nearest neighbors within maxRadius. Sorted by
    /// ascending distance, closest neighbor is first.
    /// </returns>
    public unsafe int[] QueryNearest(Vector3 position, int maxCount, float maxRadius, out float distToFurthest) {
        if (!isBuilt)
        {
            distToFurthest = 0;
            return null;
        }

        nint ptr = KnnQuery(accel, queryCache.Value, in position, maxRadius, (uint)maxCount,
            out uint numFound, out distToFurthest);
        Span<Neighbor> neighbors = new(ptr.ToPointer(), (int)numFound);

        if (numFound == 0)
            return null;

        int[] result = new int[numFound];

        // Unfold the max heap into the correct order
        // TODO can probably do this more efficiently
        var n = neighbors.ToArray();
        Array.Sort(n, (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < numFound; ++i) {
            result[i] = ids[(int)n[i].id];
        }

        return result;
    }

    /// <summary>
    /// Callback type for <see cref="ForAllNearest"/>.
    /// </summary>
    /// <param name="position">Position of the neighbor</param>
    /// <param name="id">User-defined ID of the neighbor</param>
    /// <param name="distance">Distance from the query point</param>
    /// <param name="numFound">Number of points found</param>
    /// <param name="distToFurthest">Distance of the furthest away point to the query location</param>
    public delegate void QueryCallback(Vector3 position, int id, float distance, int numFound, float distToFurthest);

    /// <summary>
    /// Queries the k nearest neighbors within the given radius. Thread-safe: multiple queries
    /// can be performed by different threads lock-free.
    /// Invokes the given delegate for all points found, but not necessarily in order.
    /// </summary>
    /// <param name="position">The query location</param>
    /// <param name="maxCount">Maximum number of neighbors to find ("k")</param>
    /// <param name="maxRadius">Maximum distance between any neighbor and the query point</param>
    /// <param name="callback">Delegate invoked for each neighbor</param>
    public unsafe void ForAllNearest(Vector3 position, int maxCount, float maxRadius, QueryCallback callback) {
        nint ptr = KnnQuery(accel, queryCache.Value, in position, maxRadius, (uint)maxCount,
            out uint numFound, out float distToFurthest);
        Span<Neighbor> neighbors = new(ptr.ToPointer(), (int)numFound);

        if (numFound == 0)
            return;

        for (int i = 0; i < numFound; ++i) {
            int idx = (int)neighbors[i].id;
            callback(positions[idx], ids[idx], neighbors[i].distance, (int)numFound, distToFurthest);
        }
    }

    /// <summary>
    /// Callback type for <see cref="ForAllNearest"/> that can transfer additional user data.
    /// </summary>
    /// <param name="position">Position of the neighbor</param>
    /// <param name="id">User-defined ID of the neighbor</param>
    /// <param name="distance">Distance from the query point</param>
    /// <param name="numFound">Number of points found</param>
    /// <param name="distToFurthest">Distance of the furthest away point to the query location</param>
    /// <param name="userData">The user-defined data that is transferred from the call site</param>
    public delegate void QueryCallback<T>(Vector3 position, int id, float distance, int numFound, float distToFurthest, ref T userData);

    /// <summary>
    /// Queries the k nearest neighbors within the given radius. Thread-safe: multiple queries
    /// can be performed by different threads lock-free.
    /// Invokes the given delegate for all points found, but not necessarily in order.
    /// </summary>
    /// <param name="position">The query location</param>
    /// <param name="maxCount">Maximum number of neighbors to find ("k")</param>
    /// <param name="maxRadius">Maximum distance between any neighbor and the query point</param>
    /// <param name="callback">Delegate invoked for each neighbor</param>
    /// <param name="userData">Will be passed by-reference to the delegate with each invoke</param>
    public unsafe void ForAllNearest<T>(Vector3 position, int maxCount, float maxRadius, QueryCallback<T> callback, ref T userData) {
        nint ptr = KnnQuery(accel, queryCache.Value, in position, maxRadius, (uint)maxCount,
            out uint numFound, out float distToFurthest);
        Span<Neighbor> neighbors = new(ptr.ToPointer(), (int)numFound);

        if (numFound == 0)
            return;

        for (int i = 0; i < numFound; ++i) {
            int idx = (int)neighbors[i].id;
            callback(positions[idx], ids[idx], neighbors[i].distance, (int)numFound, distToFurthest, ref userData);
        }
    }
}