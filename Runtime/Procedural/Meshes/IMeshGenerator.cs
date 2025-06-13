using UnityEngine;

namespace SF.Utilities.Procedural.Meshes
{
    /// <summary>
    /// Defines the code to be executed by a C# job that generates a mesh.
    /// </summary>
    public interface IMeshGenerator
    {
        int VertexCount { get; }
        int IndexCount { get; }

        /// <summary>
        /// The axis-aligned bounding box of the mesh in it's own space. 
        /// Note changing the transform doesn't change this bound.
        /// </summary>
        Bounds Bounds { get; }

        /// <summary>
        /// The length of the job to be injected into the scheduler.
        /// </summary>
        int JobLength { get; }

        int Resolution { get; set; }

        void Execute<S>(int index, S streams) where S : struct, IMeshStreams;
    }
}
