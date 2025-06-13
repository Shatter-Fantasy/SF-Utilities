using Unity.Burst;
using Unity.Mathematics;

namespace SF.Utilities.Procedural.Meshes
{
    /// <summary>
    /// Data struct for jobifying Vertex data streaming.
    /// <see cref="UnityEngine.Mesh.MeshData"/>
    /// </summary>
    [BurstCompile]
    public struct Vertex
    {
        public float3 Position, Normal;
        public float4 Tangent;
        public float2 TexCoord0;
    }
}
