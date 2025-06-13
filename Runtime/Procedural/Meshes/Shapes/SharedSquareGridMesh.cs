using UnityEngine;

using static Unity.Mathematics.math;

namespace SF.Utilities.Procedural.Meshes
{
    public struct SharedSquardGridMesh : IMeshGenerator
    {
        /* Note static Unity.Mathematics.math allows the use of right(), up() and float declarations without using new.
            Sharing the vertices where possible let's us drop the vertex count by a lot.
            Example in Square Grid is 4 * Resolution * Resolution
            or when resolution is 5
            4 * 5 * 5 = 100
            Here  sharing vertices for triangles we get for a resolution of 5
            (5 + 1) * (5 + 1) = 36 */
        public int VertexCount => (Resolution + 1) * (Resolution + 1);

        /* Two triangles are needed for a quad.
            Indices stay the same amount. Some will point to the same Vertex though.
            This is how we share Vertex data to lower out vertices count for performance improvements. */
        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution + 1;

        /// <summary>
        /// The amount of columns and rows of the shape to be generated into the mesh.
        /// Columns and rows will have the same amount.
        /// </summary>
        public int Resolution { get; set; }

        public Bounds Bounds => new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));


        /// <summary>
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="z"> This acts like an index for a plane shape mesh.</param>
        /// <param name="streams"></param>
        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            // Set the first index of the first row
            int vertexIndex = (Resolution + 1) * z;

            /*  For the curent vertex row and the one below it we set the first traingle index
                of a row to the row index minus one, multiplied with double the resolution.

                The below shows the index offset for each traingle pair that makes up a square.
                This results looking like this for the bottom left traingle in a square/quad.
                The results are flipped dia
                R = Resolution

                   
                  
                  ( -1 ) --- (0)                    
                   |----------|                         
                   |   \      |                         
                   |    \     |                         
                   |     \    |                         
                   |      \   |                        
                   |----------|                         
               (- R - 2) --- ( -R - 1 )              
            */
            int triangleIndex = 2 * Resolution * (z - 1);

            var vertex = new Vertex();
            vertex.Normal.y = 1f;
            vertex.Tangent.xw = float2(1f, -1f);

            // Set the first left most vertex of the row.
            // The 0.5 will help with centering the mesh origin later on.
            vertex.Position.x = -0.5f;
            vertex.Position.z = (float)z / Resolution - 0.5f;
            vertex.TexCoord0.y = (float)z / Resolution; // (0, 1) Stretch out the textcoordinate over the whole mesh.
            streams.SetVertex(vertexIndex, vertex);

            vertexIndex += 1;
            
            for(int x = 1; x <= Resolution; x++, vertexIndex++, triangleIndex +=2)
            {

                vertex.Position.x = (float)x / Resolution - 0.5f;
                vertex.TexCoord0.x = (float)x / Resolution; // Don't do a -0.5f offset for TexCoord0
                streams.SetVertex(vertexIndex,vertex);

                // Don't generate quads for every vertex row. There will be now quads below the bottom row.
                if(z > 0)
                {

                    /// See the visual diagram in the comment right below setting the initial vertex index 
                    /// in the <see cref="Execute{S}(int, S)"/> to see the bottom left traingle of a square
                    streams.SetTriangle(
                    triangleIndex + 0, vertexIndex + int3(-Resolution - 2, -1, -Resolution - 1)
                    );

                    /// See the visual diagram in the comment right below setting the initial vertex index 
                    /// in the <see cref="Execute{S}(int, S)"/> to see the top right traingle of a square
                    streams.SetTriangle(
                       triangleIndex + 1, vertexIndex + int3(-Resolution - 1, -1, 0)
                    );
                }
            }
        }
    }
}

