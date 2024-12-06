namespace TinyEmbree;

internal static partial class TinyEmbreeCore {
    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint NewKnnAccelerator();

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ReleaseKnnAccelerator(nint accelerator);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void SetKnnPoints(nint accelerator, nint data, uint numPoints);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint NewKnnQueryCache();

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ReleaseKnnQueryCache(nint cache);

    [DllImport("TinyEmbreeCore", CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint KnnQuery(nint accelerator, nint cache, in Vector3 pos, float radius, uint k,
        out uint numFound, out float furthest);
}

/// <summary>
/// Provides a k nearest neighbor search with maximum radius.
/// Implemented via user geometry with Embree.
/// </summary>
public class NearestNeighborSearch<T> : IDisposable {
    /// <summary>
    /// Stores info on a nearby point found by a knn query
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Neighbor {
        /// <summary>
        /// User-assigned ID of this point
        /// </summary>
        public uint Id;

        /// <summary>
        /// Distance to the query location
        /// </summary>
        public float Distance;
    }

    // readonly List<Vector3> positions = [];
    readonly List<T> userData = [];
    const int INITIAL_CAPACITY = 10000;
    Vector3[] positions = new Vector3[INITIAL_CAPACITY];

    /// <returns>User data associated with the i-th point</returns>
    public T GetUserData(uint i) => userData[(int)i];

    nint accel;
    bool isBuilt;
    GCHandle positionBuffer;
    readonly ThreadLocal<nint> queryCache = new(TinyEmbreeCore.NewKnnQueryCache, trackAllValues: true);

    /// <summary>
    /// Prepares a new BVH for kNN search
    /// </summary>
    public NearestNeighborSearch() {
        accel = TinyEmbreeCore.NewKnnAccelerator();
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
            TinyEmbreeCore.ReleaseKnnAccelerator(accel);
            accel = nint.Zero;

            foreach (var cache in queryCache.Values) {
                TinyEmbreeCore.ReleaseKnnQueryCache(cache);
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
    /// <param name="userData">User data associated with this point (e.g., an index)</param>
    public void AddPoint(Vector3 position, T userData) {
        // lock (this) {
        // positions.Add(position);
        this.userData.Add(userData);
        int idx = this.userData.Count - 1;
        if (idx >= positions.Length) {
            Vector3[] newBuf = new Vector3[2 * (idx + 1)];
            positions.CopyTo(newBuf, 0);
            positions = newBuf;
        }
        positions[idx] = position;

        isBuilt = false;
        // }
    }

    /// <summary>
    /// Builds the acceleration structure based on the points added via <see cref="AddPoint(Vector3, T)"/>
    /// </summary>
    public void Build() {
        if (positionBuffer.IsAllocated)
            positionBuffer.Free();
        positionBuffer = GCHandle.Alloc(positions, GCHandleType.Pinned);
        TinyEmbreeCore.SetKnnPoints(accel, positionBuffer.AddrOfPinnedObject(), (uint)userData.Count);
        isBuilt = true;
    }

    /// <summary>
    /// Removes all points from the acceleration structure so it can be filled with a new set.
    /// </summary>
    public void Clear() {
        if (positionBuffer.IsAllocated)
            positionBuffer.Free();
        userData.Clear();
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
    /// The memory used to store these IDs is reused upon the next call of this function within the same thread.
    /// I.e., results should be consumed immediately and copied if needed.
    /// </returns>
    public unsafe Span<Neighbor> QueryNearestSorted(Vector3 position, int maxCount, float maxRadius, out float distToFurthest) {
        var neighbors = QueryNearest(position, maxCount, maxRadius, out distToFurthest);
        neighbors.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        return neighbors;
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
    /// User IDs and distances of up to maxCount points within maxRadius. Order is undefined.
    /// The memory used to store these IDs is reused upon the next call of this function within the same thread.
    /// I.e., results should be consumed immediately and copied if needed.
    /// </returns>
    public unsafe Span<Neighbor> QueryNearest(Vector3 position, int maxCount, float maxRadius, out float distToFurthest) {
        if (!isBuilt)
            throw new InvalidOperationException("Acceleration structure must be built before neighbors can be queried. Call Build()");

        nint ptr = TinyEmbreeCore.KnnQuery(accel, queryCache.Value, in position, maxRadius, (uint)maxCount,
            out uint numFound, out distToFurthest);
        return new(ptr.ToPointer(), (int)numFound);
    }

    /// <summary>
    /// Callback type for <see cref="ForAllNearest"/>.
    /// </summary>
    /// <param name="position">Position of the neighbor</param>
    /// <param name="userData">User-defined data of the neighbor</param>
    /// <param name="distance">Distance from the query point</param>
    /// <param name="numFound">Number of points found</param>
    /// <param name="distToFurthest">Distance of the furthest away point to the query location</param>
    public delegate void QueryCallback(Vector3 position, T userData, float distance, int numFound, float distToFurthest);

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
        var neighbors = QueryNearest(position, maxCount, maxRadius, out float distToFurthest);

        foreach (var n in neighbors) {
            callback(positions[(int)n.Id], userData[(int)n.Id], n.Distance, neighbors.Length, distToFurthest);
        }
    }

    /// <summary>
    /// Callback type for <see cref="ForAllNearest"/> that can transfer additional user data.
    /// </summary>
    /// <param name="position">Position of the neighbor</param>
    /// <param name="userData">User-defined ID of the neighbor</param>
    /// <param name="distance">Distance from the query point</param>
    /// <param name="numFound">Number of points found</param>
    /// <param name="distToFurthest">Distance of the furthest away point to the query location</param>
    /// <param name="queryUserData">The user-defined data that is transferred from the call site</param>
    public delegate void QueryCallback<TQuery>(Vector3 position, T userData, float distance, int numFound, float distToFurthest, ref TQuery queryUserData) where TQuery : allows ref struct;

    /// <summary>
    /// Queries the k nearest neighbors within the given radius. Thread-safe: multiple queries
    /// can be performed by different threads lock-free.
    /// Invokes the given delegate for all points found, but not necessarily in order.
    /// </summary>
    /// <param name="position">The query location</param>
    /// <param name="maxCount">Maximum number of neighbors to find ("k")</param>
    /// <param name="maxRadius">Maximum distance between any neighbor and the query point</param>
    /// <param name="callback">Delegate invoked for each neighbor</param>
    /// <param name="queryUserData">Will be passed by-reference to the delegate with each invoke</param>
    public unsafe void ForAllNearest<TQuery>(Vector3 position, int maxCount, float maxRadius, QueryCallback<TQuery> callback, ref TQuery queryUserData) where TQuery : allows ref struct{
        var neighbors = QueryNearest(position, maxCount, maxRadius, out float distToFurthest);

        foreach (var n in neighbors) {
            callback(positions[(int)n.Id], userData[(int)n.Id], n.Distance, neighbors.Length, distToFurthest, ref queryUserData);
        }
    }
}