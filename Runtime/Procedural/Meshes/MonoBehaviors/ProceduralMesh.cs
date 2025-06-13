using UnityEngine;
using UnityEngine.Rendering;

namespace SF.Utilities.Procedural.Meshes
{
    /// <summary>
    /// A component that can inject data from a <see cref="IMeshGenerator"/> IMeshGenerator to fill up a MeshFilter.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ProceduralMesh : MonoBehaviour
    {
        [SerializeField, Range(1,75)]
        private int _resolution = 1;

        private Mesh _mesh;
        
        /// <summary>
        /// Delegate to choose whcih mesh generator needs to be performed. 
        /// The MeshType enum is casted to an int to get the index of the Jobs array and choose the correct Mesh Generator to run.
        /// <see cref="_meshType"/>
        /// </summary>
        private static MeshJobScheduleDelegate[] Jobs =
        {
            MeshJob<SquareGrid, SingleMeshStream>.ScheduleParallel,
            MeshJob<SharedSquardGridMesh, SingleMeshStream>.ScheduleParallel,
            MeshJob<SharedTriangleGrid, SingleMeshStream>.ScheduleParallel
        };

        /// <summary>
        ///  The mesh generator job to run. 
        ///  These need to match the order of the values inside of the <see cref="Jobs"/> varaible declaration.
        /// </summary>
        public enum MeshType
        {
            SquareGrid, SharedSquareGrid, SharedTriangleGrid
        };

        [SerializeField] private MeshType _meshType;

        private void Awake()
        {
            _mesh = new Mesh
            {
                name = "Procedural Mesh"
            };
            // GenerateMesh();

            GetComponent<MeshFilter>().mesh = _mesh;
        }


        private void Update()
        {
            GenerateMesh();
            enabled = false;
        }

        private void GenerateMesh() 
        {
            // Allocate writable mesh data to allow injecting the result of an IMeshJob into the mesh.
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            // Schedule a MeshJob to run parallel and than tell it to only move forward after it is completed.
            //MeshJob<PlaneMesh, MultiMeshStream>.ScheduleParallel (
            //    _mesh, meshData, _resolution, default
            //).Complete();

            Jobs[(int)_meshType](_mesh, meshData, +_resolution, default).Complete();

            // Apply the mesh stream from the MeshJob into the mesh data array and dispose of the no longer needed data to clean up memory.
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _mesh);
        }

        private void OnValidate() => enabled = true;
    }
}
