using Unity.Mathematics;
using UnityEngine;

namespace SF.Utilities.Procedural.Meshes
{
    /// <summary>
    /// Used to implement procedural data initialization for meshs using jobified code.
    /// </summary>
    public interface IMeshStreams
    {
        /// <summary>
        /// Initialize <see cref="Mesh.MeshData"/> to be used in execution of jobified code.
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="bounds"></param>
        /// <param name="vertexCount"></param>
        /// <param name="indexCount"></param>
        void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount);

        /// <summary>
        /// Copies a <see cref="Vertex"/> data struct into a mesh's vertex buffer. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        void SetVertex(int index, Vertex data);

        /// <summary>
        /// Sets the traingles index buffer data for a mesh.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="triangle"></param>
        void SetTriangle(int index, int3 triangle);
    }
}
