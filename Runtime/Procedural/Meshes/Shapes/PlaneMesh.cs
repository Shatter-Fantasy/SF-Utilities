using UnityEngine;

using static Unity.Mathematics.math;

namespace SF.Utilities.Procedural.Meshes
{
    public struct PlaneMesh : IMeshGenerator
    {
        // Note static Unity.Mathematics.math allows the use of right(), up() and float declarations without using new.
        public int VertexCount => 4 * Resolution * Resolution;

        // Two triangles are needed for a quad.
        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution;

        /// <summary>
        /// The amount of columns and rows of the shape to be generated into the mesh.
        /// Columns and rows will have the same amount.
        /// </summary>
        public int Resolution { get; set; }

        // Vector3 (1f, 0 , 1f ) sets the center of the bounds to the middle of the generated plane.
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0 ,1f));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="z"> This acts like an index for a plane shape mesh.</param>
        /// <param name="streams"></param>
        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            /*  We have to manually move over the memory pointer spot to show
                which traingle shape we are adding the vertexes over. */

            int vertexIndex = 4 * Resolution * z, triangleIndex = 2 * Resolution * z;

            for(int x = 0; x < Resolution; x++, vertexIndex += 4, triangleIndex += 2)
            {

                // Setting the coordniates in seperate values allows the burst compiler to do some optmizations for us automatically.
                // Center the plane mesh to have it's 0,0 be in the origin
                // Resolution - 0.5f

                // x value of each float2 represents the minimum of the x or z value a vertex can have.
                // Y value of each float2 represents the maximum of the x or z value a vertex can have.

                var xCoordinates = float2(x, x + 1f) / Resolution - 0.5f;
                var zCoordinates = float2(z, z + 1f) / Resolution - 0.5f;

                var vertex = new Vertex();
                vertex.Normal.y = 1f;
                vertex.Tangent.xw = float2(1f, -1f);

                vertex.Position.x = xCoordinates.x;
                vertex.Position.z = zCoordinates.x;

                // Set the first Vertex with an index of zero.
                // Side note normally you would need the NativeDisableParallelForRestrictionAttribute here to write to any index,
                // but we declared a NativeDisableContainerSafetyRestrictionAttribute on the NativeArray containers so no need to.
                streams.SetVertex(vertexIndex + 0, vertex);

                // Bottom right Vertex of quad
                // Only need to set xCoordinates.y because the last vertex set is already on the bottom part of the plane.
                vertex.Position.x = xCoordinates.y;
                vertex.TexCoord0 = float2(1f, 0f);
                streams.SetVertex(vertexIndex + 1, vertex);

                // Top left Vertex of quad
                vertex.Position.x = xCoordinates.x;
                vertex.Position.z = zCoordinates.y;

                vertex.TexCoord0 = float2(0f, 1f);
                streams.SetVertex(vertexIndex + 2, vertex);

                // Top right Vertex of quad
                // Previous z value was already set to the top so no need to update it's value.
                vertex.Position.x = xCoordinates.y;
                // Just like HLSL shaders the Mathematics library supports swizzling and spreading one value into two.
                vertex.TexCoord0 = 1f;
                streams.SetVertex(vertexIndex + 3, vertex);

                // Set the indices of the traingles to match the vertices.
                // Botom left triangle uses the 0, 2, 1 vertexes
                // Top right triangle uses the 1,2, 3 vertexes.
                streams.SetTriangle(triangleIndex + 0, vertexIndex + int3(0, 2, 1));
                streams.SetTriangle(triangleIndex + 1, vertexIndex + int3(1, 2, 3));
            }
        }
    }
}

