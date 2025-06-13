using UnityEngine;

using static Unity.Mathematics.math;

namespace SF.Utilities.Procedural.Meshes
{
    public struct SquareGrid : IMeshGenerator
    {
        // Note static Unity.Mathematics.math allows the use of right(), up() and float declarations without using new.
        public int VertexCount => 4 * Resolution * Resolution;

        // Two triangles are needed for a quad.
        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => 1 * Resolution * Resolution;

        /// <summary>
        /// The amount of columns and rows of the shape to be generated into the mesh.
        /// Columns and rows will have the same amount.
        /// </summary>
        public int Resolution { get; set; }

        public Bounds Bounds => new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));


        public void Execute<S>(int index, S streams) where S : struct, IMeshStreams
        {
            /*  We have to manually move over the memory pointer spot to show
                which traingle shape we are adding the vertexes over. */

            int vertexIndex = 4 * index, triangleIndex = 2 * index;

            // Declare y first because it is used to calculate the x.
            int y = index / Resolution;
            int x = index - Resolution * y;

            var coordinates = float4(x, x + 1, y, y + 1);

            var vertex = new Vertex();
            vertex.Normal.z = -1f;
            vertex.Tangent.xw = float2(1f, -1f);


            // Set the first Vertex with an index of zero.

            // We use coordinate z because we are making a quad.
            vertex.Position.xy = coordinates.xz;
            // Side note normally you would need the NativeDisableParallelForRestrictionAttribute here to write to any index,
            // but we declared a NativeDisableContainerSafetyRestrictionAttribute on the NativeArray containers so no need to.
            streams.SetVertex(vertexIndex + 0, vertex);

            // Bottom right Vertex of quad
            vertex.Position.xy = coordinates.yz;
            vertex.TexCoord0 = float2(1f, 0f);
            streams.SetVertex(vertexIndex + 1, vertex);

            // Top left Vertex of quad
            vertex.Position.xy = coordinates.xw;
            vertex.TexCoord0 = float2(0f, 1f);
            streams.SetVertex(vertexIndex + 2, vertex);
    
            // Top right Vertex of quad
            vertex.Position.xy = coordinates.yw;
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
