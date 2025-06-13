using System.Runtime.InteropServices;

using Unity.Burst;
using Unity.Mathematics;

namespace SF.Utilities.Procedural.Meshes
{
    /* Reason for having a UInt16 Triangle
      
       Example three data sets of 32 bit
       96 bits altogether
       Two can be changed to 16 bits
       Now total is 64 bits.

       Current hardware meshes are now commonly having undreds of thousands of vertices rendered on screen.
       A 33%% data reduction scales very quickly on modern hardware for performance improvements.
     */
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile]
    public struct TriangleUInt16
    {
        /// <summary>
        /// The 16 bit indices for the Traingle vertexes.
        /// </summary>
        public ushort a, b, c;

        /// <summary>
        /// Used in some Reinterpret function calls to provide a samller data memory usage.
        /// In the SingleMeshStream.Setup there is an example usage of this.
        /// <see cref="SingleMeshStream.Setup(UnityEngine.Mesh.MeshData, UnityEngine.Bounds, int, int)"/>
        /// </summary>
        /// <param name="t"></param>
        public static implicit operator TriangleUInt16(int3 t) => new TriangleUInt16 
        {
            a = (ushort)t.x,
            b = (ushort)t.y,
            c = (ushort)t.z,       
        };
    }
}
