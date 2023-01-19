using System.Text.Json.Serialization;

namespace TinyEmbree;

/// <summary>
/// Statistic counters for the number of ray tracing operations and their results.
/// </summary>
public struct RayTracerStats {
    /// <summary>
    /// Number of shadow rays that have been traced
    /// </summary>
    [JsonInclude] public uint NumShadowRays;

    /// <summary>
    /// Number of "normal" (i.e., nearest hit) rays that have been traced
    /// </summary>
    [JsonInclude] public uint NumRays;

    /// <summary>
    /// Number of shadow rays that were occluded
    /// </summary>
    [JsonInclude] public uint NumOccluded;

    /// <summary>
    /// Number of "normal" rays that intersected something
    /// </summary>
    [JsonInclude] public uint NumRayHits;
}
