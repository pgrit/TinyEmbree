namespace TinyEmbree;

/// <summary>
/// A ray that should be intersected with the scene
/// </summary>
public struct Ray {
    /// <summary>
    /// Origin of the ray in world space
    /// </summary>
    public Vector3 Origin;

    /// <summary>
    /// Direction of the ray, does not need to be normalized
    /// </summary>
    public Vector3 Direction;

    /// <summary>
    /// Only hit points that are further away than this value will be reported
    /// </summary>
    public float MinDistance;

    /// <summary>
    /// Computes a point at the given distance along the ray.
    /// </summary>
    /// <param name="t">The distance as a multiple of the length of the direction vector</param>
    /// <returns>Point along the ray in world space</returns>
    public Vector3 ComputePoint(float t) => Origin + t * Direction;
}
