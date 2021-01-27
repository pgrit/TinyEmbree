namespace TinyEmbree {
    public readonly struct ShadowRay {
        public readonly Ray Ray;
        public readonly float MaxDistance;
        public ShadowRay(Ray ray, float maxDistance) {
            Ray = ray;
            MaxDistance = maxDistance;
        }
    }
}
