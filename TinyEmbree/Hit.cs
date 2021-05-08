using System.Numerics;

namespace TinyEmbree {
    /// <summary>
    /// Stores the hit point information of a ray tracing operation.
    /// </summary>
    public struct Hit {
        /// <summary>
        /// Position of the hit point in world space
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Face / geometry normal at the hit point
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Barycentric coordinates within the intersected primitive (triangle)
        /// </summary>
        public Vector2 BarycentricCoords;

        /// <summary>
        /// The mesh that was intersected. If no intersection was found, this will be null.
        /// </summary>
        public TriangleMesh Mesh;

        /// <summary>
        /// Primitive (triangle) index
        /// </summary>
        public uint PrimId;

        /// <summary>
        /// Offset that should be used if new rays are traced from the hit point, to 
        /// avoid self-intersection issues.
        /// </summary>
        public float ErrorOffset;

        /// <summary>
        /// Distance between the hit point and the origin of the ray
        /// </summary>
        public float Distance;

        /// <summary>
        /// Returns true if the hit point is valid, i.e., the ray actually intersected something
        /// </summary>
        /// <param name="hit">The hit point</param>
        public static implicit operator bool(Hit hit) => hit.Mesh != null;

        /// <summary>
        /// The shading normal at the hit point, computed on-the-fly
        /// </summary>
        public Vector3 ShadingNormal => Mesh.ComputeShadingNormal((int)PrimId, BarycentricCoords);

        /// <summary>
        /// The texture coordinates at the hit point, computed on-the-fly
        /// </summary>
        public Vector2 TextureCoordinates => Mesh.ComputeTextureCoordinates((int)PrimId, BarycentricCoords);
    }
}
