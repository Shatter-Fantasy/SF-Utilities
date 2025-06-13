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
    /// Used to create a multi style mesh stream for generating meshes procedurallyvia jobified code.
    /// </summary>
    public struct MultiMeshStream : IMeshStreams
    {

        /* For the NativeArrays we NativeDisableContainerSafetyRestriction. 
            This is becuase during the job we are touching multiple sections of the same block of memory.
            Unity does not allow this normally, unless we say to disable certain safety restrictions on 
            the low level memory side of things.
         */

        #region Vertex NativeArray data streams

        /// <summary>
        /// The NativeArray streams will hold the raw memory of the data that will be injected 
        /// into a mesh data for procedural generation. These streams match the data inside of the Vertex struct.
        /// <see cref="Mesh.MeshData"/> 
        /// <see cref="Vertex"/>
        /// </summary>

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float3> _positionStream, _normalStream;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float4> _tangentStream;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float2> _texCoord0Stream;

        #endregion

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
                VertexAttribute.Normal, dimension: 3, // x,y,z
                stream: 1
            );
            // Set up descriptor for tangents
            descriptor[2] = new VertexAttributeDescriptor(
                VertexAttribute.Tangent, dimension: 4, // x,y,z,w. w is for flipping binormals if needed.
                stream: 2
            );

            // Set up descriptor for uv texture coordninates.
            descriptor[3] = new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, dimension: 2, // uv
                stream: 3
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
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount)
            {
                bounds = bounds,
                vertexCount = vertexCount
            },
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices);

            /*  Set the mesh data stream with the NativeArray of VertexData.
             *  MeshData.GetVertexData() allows taking an an int that reprsents the 
             *  index of the data stream we want to read/write to.
             *  This is why we set a stream: 1, stream: 2, stream: 3 value to the descriptor declarations of the VertexAttributeDescriptors.
            */

            _positionStream = meshData.GetVertexData<float3>();
            _normalStream = meshData.GetVertexData<float3>(1);
            _tangentStream = meshData.GetVertexData<float4>(2);
            _texCoord0Stream = meshData.GetVertexData<float2>(3);

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
        public void SetVertex(int index, Vertex vertex) 
        {
            _positionStream[index] = vertex.Position;
            _normalStream[index] = vertex.Normal;
            _tangentStream[index] = vertex.Tangent;
            _texCoord0Stream[index] = vertex.TexCoord0;
        }

        /// <summary>
        /// Store the Triangle raw data of the int3 into the correct spot in memory for the mesh stream.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="triangle"></param>
        public void SetTriangle(int index, int3 triangle) => _traingles[index] = triangle;
    }
}
