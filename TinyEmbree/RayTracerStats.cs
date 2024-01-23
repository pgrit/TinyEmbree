using System.Linq;

namespace TinyEmbree;

/// <summary>
/// Statistic counters for the number of ray tracing operations and their results.
/// </summary>
public struct RayTracerStats {
    /// <summary>
    /// Number of shadow rays that have been traced
    /// </summary>
    public ulong NumShadowRays { get; set; }

    /// <summary>
    /// Number of "normal" (i.e., nearest hit) rays that have been traced
    /// </summary>
    public ulong NumRays { get; set; }

    /// <summary>
    /// Number of shadow rays that were occluded
    /// </summary>
    public ulong NumOccluded { get; set; }

    /// <summary>
    /// Number of "normal" rays that intersected something
    /// </summary>
    public ulong NumRayHits { get; set; }
}

internal class RayStatCounter {
    internal RayTracerStats Current => new() {
        NumShadowRays = (ulong)numShadow.Values.Sum(v => (long)v),
        NumRays = (ulong)numRay.Values.Sum(v => (long)v),
        NumOccluded = (ulong)numOccluded.Values.Sum(v => (long)v),
        NumRayHits = (ulong)numHit.Values.Sum(v => (long)v)
    };

    readonly ThreadLocal<ulong> numShadow = new(true);
    readonly ThreadLocal<ulong> numRay = new(true);
    readonly ThreadLocal<ulong> numOccluded = new(true);
    readonly ThreadLocal<ulong> numHit = new(true);

    internal void NotifyShadowRay() => numShadow.Value++;
    internal void NotifyRay() => numRay.Value++;
    internal void NotifyOccluded() => numOccluded.Value++;
    internal void NotifyHit() => numHit.Value++;
}
