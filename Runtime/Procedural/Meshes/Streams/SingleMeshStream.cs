using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace SF.Utilities.Procedural.Meshes
{
    /// <summary>
    /// Used to create a single style mesh stream for generating meshes procedurallyvia jobified code.
    /// </summary>
    public struct SingleMeshStream : IMeshStreams
    {
        /*  Setting LayoutKind.Sequential makes the field order in memory fixed.
            Sometimes the compiler moves things around during compilation to try and improve performance.
            In this instance it could break the memory layout.

            Don't ever do this unless you know for sure you will never overwrite a section of memory 
            that is being read currently.
        */

        /// <summary>
        /// Used to store a vertex data in a single stream in a fixed layout for reading in memory streams.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Stream0
        {
            public float3 Position, Normal;
            public float4 Tangent;
            public float2 TexCoord0;
        }


        /* For both the NativeArrays we NativeDisableContainerSafetyRestriction. 
            This is becuase during the job we are touching multiple sections of the same block of memory.
            Unity does not allow this normally, unless we say to disable certain safety restrictions on 
            the low level memory side of things.
         */

        /// <summary>
        /// Array of vertex data to be used as a set of memory stream data for the MeshData.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<Stream0> _stream0;

        /// <summary>
        /// The native container for holding the indices of the triangles that point to the Vertex to use for that corner of the traingle.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<TriangleUInt16> _traingles;

        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount)
        {

            // Native array of Vertex data
            // NativeArrayOptions.UninitializedMemory is used because we don't want to waste performance settinga default when we will be guranting data will be set.
            // We use the value four for length because we are using four different vertex data points.
            // These four points match the Vertex struct data layout.

            var descriptor = new NativeArray<VertexAttributeDescriptor>(
                4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );

            // Set up the descriptor for vertex position.
            descriptor[0] = new VertexAttributeDescriptor(dimension: 3);
            
            // set up descriptor for normals
            descriptor[1] = new VertexAttributeDescriptor(
                VertexAttribute.Normal, dimension: 3 // x,y,z
            );
            // Set up descriptor for tangents
            descriptor[2] = new VertexAttributeDescriptor(
                VertexAttribute.Tangent, dimension: 4 // x,y,z,w. w is for flipping binormals if needed.
            );

            // Set up descriptor for uv texture coordninates.
            descriptor[3] = new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, dimension: 2 // uv
            );

            // Alocate the vertex streams on our mesh via SetVertexBufferParams on the MeshData
            meshData.SetVertexBufferParams(vertexCount, descriptor);
            // No longer need the descriptor so we can dispose it to release memory.
            descriptor.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            meshData.subMeshCount = 1;
            // Setting a submesh to allow for raw index, vertex, and mesh data buffer computing.
            // We don't want the bounds to be recalculated or indices to be validated This will 
            // cause an argument exception. This happens because by this point before the job runs
            // there is no index buffer set with the arbitrary data it needs.
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount) { 
                                    bounds = bounds,
                                    vertexCount = vertexCount
                },
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices);

            /*  Set the mesh stream with the NativeArray of VertexData.
                Note this is a NativeArray<int> being returned.
                Because LayoutKind.Sequential is used for the Stream0 struct
                The int's just fill it up in order of the data fields.
                The ints are just direct pointers into the raw vertex buffer data 
                This allows data transfer without memory allocations, data copying, or converstions. 
            */
            _stream0 = meshData.GetVertexData<Stream0>();

            /* Gets the data pointers types and tells how many bytes of the NativeArray should be used to split
             * the data into it's true type. So turning the data at the memory pointer into three ints or an int3.
             * The int3 acts as the three indices for a trinagle in a mesh.
             */
            _traingles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
        }

        /// <summary>
        /// Copys the <see cref="Vertex"/> data into the stream.
        /// </summary>
        /// <remarks>
        ///     MethodImplOptions.AggressiveInlining is to prevent burst from inserting call instructions if we 
        ///     do soem data type conversion such as 16 to 32 and vie verca.
        /// 
        ///     We could just use an explicit sequential layout, but the approach we are implementing 
        ///     allows for more flexability to add new data to a vertex without adjusting the data stream.
        ///     Burst will do a lot of the optmizations for us behind the scenes when copying Vertex to the Stream0.
        ///     Example vertex color data could be added down the line.
        /// </remarks>
        /// <param name="index"></param>
        /// <param name="vertex"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex vertex) => _stream0[index] = new Stream0
        {
            Position = vertex.Position,
            Normal = vertex.Normal,
            Tangent = vertex.Tangent,
            TexCoord0 = vertex.TexCoord0
        };

        /// <summary>
        /// Store the Triangle raw data of the int3 into the correct spot in memory for the mesh stream.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="triangle"></param>
        public void SetTriangle(int index, int3 triangle) => _traingles[index] = triangle;
    }
}
