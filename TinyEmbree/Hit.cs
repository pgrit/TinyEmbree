using System.Numerics;

namespace TinyEmbree {
    public struct Hit {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 BarycentricCoords;
        public Mesh Mesh;
        public uint PrimId;
        public float ErrorOffset;
        public float Distance;

        public static implicit operator bool(Hit hit)
            => hit.Mesh != null;

        public Vector3 ShadingNormal => Mesh.ComputeShadingNormal((int)PrimId, BarycentricCoords);

        public Vector2 TextureCoordinates => Mesh.ComputeTextureCoordinates((int)PrimId, BarycentricCoords);
    }
}
