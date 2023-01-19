namespace TinyEmbree;

/// <summary>
/// A shadow ray, i.e., a ray that is used to query if two points are mutually visible.
/// </summary>
public readonly struct ShadowRay {
    /// <summary>
    /// The underlying ray
    /// </summary>
    public readonly Ray Ray;

    /// <summary>
    /// The distance between the other point, measured from the ray origin, in multiples
    /// of the ray direction length.
    /// </summary>
    public readonly float MaxDistance;

    /// <summary>
    /// Generates a new shadow ray given a ray and a maximum travel distance
    /// </summary>
    /// <param name="ray">The ray</param>
    /// <param name="maxDistance">
    ///     Distance to the other point, measured in multiples of the ray direction vector length
    /// </param>
    public ShadowRay(Ray ray, float maxDistance) {
        Ray = ray;
        MaxDistance = maxDistance;
    }
}
