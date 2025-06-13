using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

namespace SF.Utilities.Procedural.Meshes
{
    public delegate JobHandle MeshJobScheduleDelegate(
        Mesh mesh,
        Mesh.MeshData meshData,
        int resolution,
        JobHandle dependency
    );

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MeshJob<TGenerator, TStream> : IJobFor
        where TGenerator : struct, IMeshGenerator
        where TStream: struct, IMeshStreams
    {


        private TGenerator _generator;

        /* We are only writing to the streams when generating the mesh and don't read from them.
         * This allows us to use [WriteOnly] for better performance.
        */
        [WriteOnly]
        private TStream _streams;

        public void Execute(int index) => _generator.Execute(index, _streams);

        public static JobHandle ScheduleParallel (
            Mesh mesh,
            Mesh.MeshData meshData, 
            int resolution,
            JobHandle dependency 
        )
        {
            // Set up a new mesh job.
            var job = new MeshJob<TGenerator, TStream>();

            // The resolution needs to be set before the Streams.SetUp is called.
            // This is due to VertexCount, IndexCount, and JobLength adjusting to the resolution value.
            job._generator.Resolution = resolution;

            // Set up and initialize the MeshData before using them in the jobs.
            job._streams.Setup(
                    meshData,
                    mesh.bounds = job._generator.Bounds,
                    job._generator.VertexCount, // These are set per shape type for the mesh
                    job._generator.IndexCount // These are set per shape type for the mesh
            );

            // Schedule the jobs to run parallel than returnt he job handles created for the jobs.
            // This allow us to get the job handle and view the completed job data later on.
            return job.ScheduleParallel(job._generator.JobLength, 1, dependency);
        }
    }
}
