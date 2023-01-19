namespace TinyEmbree;

/// <summary>
/// A simple triangle mesh that can be sent to Embree for ray tracing
/// </summary>
public class TriangleMesh {
    /// <summary>
    /// Creates a new mesh from the given vertices, indices, and optional parameters
    /// </summary>
    /// <param name="vertices">Array of triangle vertices</param>
    /// <param name="indices">Array of indices, three for each triangle</param>
    /// <param name="shadingNormals">
    ///     Optional: shading normals at each vertex. If given, needs to have the same length as vertices
    /// </param>
    /// <param name="textureCoordinates">
    ///     Optional: texture coordinates at each vertex. If given, needs to have the same length as vertices
    /// </param>
    public TriangleMesh(Vector3[] vertices, int[] indices, Vector3[] shadingNormals = null,
                        Vector2[] textureCoordinates = null) {
        Vertices = vertices;
        Indices = indices;

        Debug.Assert(indices.Length % 3 == 0, "Triangle mesh indices must be a multiple of three.");

        // Compute face normals and triangle areas
        FaceNormals = new Vector3[NumFaces];
        var surfaceAreas = new float[NumFaces];
        SurfaceArea = 0;
        for (int face = 0; face < NumFaces; ++face) {
            var v1 = vertices[indices[face * 3 + 0]];
            var v2 = vertices[indices[face * 3 + 1]];
            var v3 = vertices[indices[face * 3 + 2]];

            // Compute the normal. Winding order is CCW always.
            Vector3 n = Vector3.Cross(v2 - v1, v3 - v1);
            float len = n.Length();
            FaceNormals[face] = n / len;
            surfaceAreas[face] = len * 0.5f;

            SurfaceArea += surfaceAreas[face];
        }

        ShadingNormals = shadingNormals;
        TextureCoordinates = textureCoordinates;

        // Compute shading normals from face normals if not set
        if (ShadingNormals == null) {
            ShadingNormals = new Vector3[vertices.Length];
            for (int face = 0; face < NumFaces; ++face) {
                ShadingNormals[indices[face * 3 + 0]] = FaceNormals[face];
                ShadingNormals[indices[face * 3 + 1]] = FaceNormals[face];
                ShadingNormals[indices[face * 3 + 2]] = FaceNormals[face];
            }
        } else {
            // Ensure normalization
            for (int i = 0; i < ShadingNormals.Length; ++i)
                ShadingNormals[i] = Vector3.Normalize(ShadingNormals[i]);
        }
    }

    /// <summary>
    /// Computes the texture coordinates from the barycentric coordinates of a triangle
    /// </summary>
    /// <param name="faceIdx">
    ///     The index of the triangle within the mesh (based on the indices passed to the constructor)
    /// </param>
    /// <param name="barycentric">Barycentric coordinates within that triangle</param>
    /// <returns>Texture coordinates</returns>
    public Vector2 ComputeTextureCoordinates(int faceIdx, Vector2 barycentric) {
        if (TextureCoordinates == null)
            return new Vector2(0, 0);

        var v1 = TextureCoordinates[Indices[faceIdx * 3 + 0]];
        var v2 = TextureCoordinates[Indices[faceIdx * 3 + 1]];
        var v3 = TextureCoordinates[Indices[faceIdx * 3 + 2]];

        return barycentric.X * v2
            + barycentric.Y * v3
            + (1 - barycentric.X - barycentric.Y) * v1;
    }

    /// <summary>
    /// Computes the shading normal from the barycentric coordinates of a triangle
    /// </summary>
    /// <param name="faceIdx">
    ///     The index of the triangle within the mesh (based on the indices passed to the constructor)
    /// </param>
    /// <param name="barycentric">Barycentric coordinates within that triangle</param>
    /// <returns>Shading normal</returns>
    public Vector3 ComputeShadingNormal(int faceIdx, Vector2 barycentric) {
        var v1 = ShadingNormals[Indices[faceIdx * 3 + 0]];
        var v2 = ShadingNormals[Indices[faceIdx * 3 + 1]];
        var v3 = ShadingNormals[Indices[faceIdx * 3 + 2]];

        return Vector3.Normalize(
            barycentric.X * v2
            + barycentric.Y * v3
            + (1 - barycentric.X - barycentric.Y) * v1);
    }

    /// <summary>
    /// Computes the world space position from the barycentric coordinates of a triangle
    /// </summary>
    /// <param name="faceIdx">
    ///     The index of the triangle within the mesh (based on the indices passed to the constructor)
    /// </param>
    /// <param name="barycentric">Barycentric coordinates within that triangle</param>
    /// <returns>World space position</returns>
    public Vector3 ComputePosition(int faceIdx, Vector2 barycentric) {
        var v1 = Vertices[Indices[faceIdx * 3 + 0]];
        var v2 = Vertices[Indices[faceIdx * 3 + 1]];
        var v3 = Vertices[Indices[faceIdx * 3 + 2]];

        return barycentric.X * v2
            + barycentric.Y * v3
            + (1 - barycentric.X - barycentric.Y) * v1;
    }

    /// <summary>
    /// Vertices of the triangles
    /// </summary>
    public readonly Vector3[] Vertices;

    /// <summary>
    /// Indices of the triangles. Each consecutive set of three values references the vertices of one triangle.
    /// </summary>
    public readonly int[] Indices;

    /// <summary>
    /// Actual geometric normals of each triangle (group of three indices).
    /// Pre-computed by the constructor.
    /// </summary>
    public readonly Vector3[] FaceNormals;

    /// <summary>
    /// Total surface area of the entire mesh, pre-computed by the constructor.
    /// </summary>
    public readonly float SurfaceArea;

    /// <summary>
    /// Number of vertices in the mesh
    /// </summary>
    public int NumVertices => Vertices.Length;

    /// <summary>
    /// Number of triangles in the mesh
    /// </summary>
    public int NumFaces => Indices.Length / 3;

    /// <summary>
    /// Shading normal associated with each vertex (automatically computed if not set)
    /// </summary>
    public readonly Vector3[] ShadingNormals;

    /// <summary>
    /// Texture uv coordinates associated with each vertex
    /// </summary>
    public readonly Vector2[] TextureCoordinates;
}
