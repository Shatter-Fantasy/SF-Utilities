#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Rendering;

namespace SF.Utilities
{
    public static class GLUtilities
    {
        public static Color GLColor;
        public static Material DrawHandleMaterial;

        private const int k_GridGizmoVertexCount = 32000;
        private const float k_GridGizmoDistanceFalloff = 50f;
        
        /// <summary>
        /// This is used to store the Viewport before doing a viewport clip during clipping operations. This allows for restoring the pre clipped viewport rect.
        /// </summary>
        private static Rect _storedPreClippedViewPort = new();
        public static void InitHandleMaterial(CompareFunction zTest = CompareFunction.Always)
        {
            if(!DrawHandleMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                DrawHandleMaterial = new Material(shader);
                DrawHandleMaterial.hideFlags = HideFlags.HideAndDontSave;
                DrawHandleMaterial.SetFloat("_HandleZTest", (float)zTest);
                DrawHandleMaterial.SetInt("_ZWrite", 0);
            }
        }
        public static void ApplyHandleMaterial(CompareFunction zTest = CompareFunction.Always)
        {
            InitHandleMaterial(zTest);
            DrawHandleMaterial.SetPass(0);
        }
        public static void ClipViewPort(Rect viewPortRect, Vector2 clipOffset)
        {
            _storedPreClippedViewPort = viewPortRect;

            // TODO: Check different types of view matrices to see if we need to minus instead of add the position.
            viewPortRect.position -= clipOffset;
            viewPortRect.size -= clipOffset;
            GL.Viewport(new Rect(viewPortRect));
        }
        /// <summary>
        /// Restores the Viewport rect to the value before it was clipped during any clipping operation.
        /// </summary>
        public static void RestorePreClippedViewRect()
        {
            GL.Viewport(_storedPreClippedViewPort);
        }
        public static void StartDrawing(Matrix4x4 matrix, int drawMode = GL.LINES)
        {
            GL.PushMatrix();
            GL.MultMatrix(matrix);
            GL.Begin(drawMode);
            GL.Color(GLColor);
        }
        public static void StartDrawing(Matrix4x4 matrix, Color color, int drawMode = GL.LINES)
        {
            GL.PushMatrix();
            GL.MultMatrix(matrix);
            GL.Begin(drawMode);
            GL.Color(color);
        }

        public static void StartDrawingOrtho(int drawMode = GL.LINES)
        {
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(drawMode);
            GL.Color(GLColor);
        }


        public static void EndDrawing()
        {
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawLine(Vector3 p1, Vector3 p2, float width = 1)
        {
            // Draw the normal line than draw extra if the thickness needs increased.
            GL.Vertex(p1);
            GL.Vertex(p2);

            // TODO implement multi width line drawing.
        }
        public static void DrawBatchedLine(Vector3 p1, Vector3 p2)
        {
            GL.Vertex3(p1.x, p1.y, p1.z);
            GL.Vertex3(p2.x, p2.y, p2.z);
        }
        public static void DrawBatchedHorizontalLine(float x1, float x2, float y)
        {
            GL.Vertex3(x1, y, 0f);
            GL.Vertex3(x2, y, 0f);
            GL.Vertex3(x2, y + 1, 0f);
            GL.Vertex3(x1, y + 1, 0f);
        }
        public static void DrawBatchedVerticalLine(float y1, float y2, float x)
        {
            GL.Vertex3(x, y1, 0f);
            GL.Vertex3(x, y2, 0f);
            GL.Vertex3(x + 1, y2, 0f);
            GL.Vertex3(x + 1, y1, 0f);
        }

        public static void DrawBox(Rect position, int width = 1)
        {
            var points0 = new Vector3(position.xMin, position.yMin, 0f);
            var points1 = new Vector3(position.xMax, position.yMin, 0f);
            var points2 = new Vector3(position.xMax, position.yMax, 0f);
            var points3 = new Vector3(position.xMin, position.yMax, 0f);

            DrawLine(points0, points1, width);
            DrawLine(points1, points2, width);
            DrawLine(points2, points3, width);
            DrawLine(points3, points0, width);
        }

        public static void DrawTraingle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
            GL.Vertex(p3);
        }


        public static void DrawGridMarquee(GridLayout gridLayout, BoundsInt area, Color color)
        {
            if(gridLayout == null)
                return;
            switch(gridLayout.cellLayout)
            {
                case GridLayout.CellLayout.Hexagon:
                    DrawSelectedHexGridArea(gridLayout, area, color);
                    break;
                case GridLayout.CellLayout.Isometric:
                case GridLayout.CellLayout.IsometricZAsY:
                case GridLayout.CellLayout.Rectangle:
                    var cellStride = gridLayout.cellSize + gridLayout.cellGap;
                    var cellGap = Vector3.one;
                    if(!Mathf.Approximately(cellStride.x, 0f))
                    {
                        cellGap.x = gridLayout.cellSize.x / cellStride.x;
                    }
                    if(!Mathf.Approximately(cellStride.y, 0f))
                    {
                        cellGap.y = gridLayout.cellSize.y / cellStride.y;
                    }

                    Vector3[] cellLocals =
                    {
                        gridLayout.CellToLocal(new Vector3Int(area.xMin, area.yMin, area.zMin)),
                        gridLayout.CellToLocalInterpolated(new Vector3(area.xMax - 1 + cellGap.x, area.yMin, area.zMin)),
                        gridLayout.CellToLocalInterpolated(new Vector3(area.xMax - 1 + cellGap.x, area.yMax - 1  + cellGap.y, area.zMin)),
                        gridLayout.CellToLocalInterpolated(new Vector3(area.xMin, area.yMax - 1 + cellGap.y, area.zMin))
                    };

                    ApplyHandleMaterial(CompareFunction.Always);
                    GL.PushMatrix();
                    GL.MultMatrix(gridLayout.transform.localToWorldMatrix);
                    GL.Begin(GL.LINES);
                    GL.Color(color);
                    int i = 0;

                    for(int j = cellLocals.Length - 1; i < cellLocals.Length; j = i++)
                        DrawBatchedLine(cellLocals[j], cellLocals[i]);

                    GL.End();
                    GL.PopMatrix();
                    break;
            }
        }
        public static void DrawSelectedHexGridArea(GridLayout gridLayout, BoundsInt area, Color color)
        {
            int requiredVertices = 4 * (area.size.x + area.size.y) - 2;
            if(requiredVertices < 0)
                return;
            Vector3[] cellLocals = new Vector3[requiredVertices];
            int horizontalCount = area.size.x * 2;
            int verticalCount = area.size.y * 2 - 1;
            int bottom = 0;
            int top = horizontalCount + verticalCount + horizontalCount - 1;
            int left = requiredVertices - 1;
            int right = horizontalCount;
            Vector3[] cellOffset =
            {
                // Replace this with the new GetSwizzleCellOffSets function
                Grid.Swizzle(gridLayout.cellSwizzle, new Vector3(0, gridLayout.cellSize.y / 2, 0)),
                Grid.Swizzle(gridLayout.cellSwizzle, new Vector3(gridLayout.cellSize.x / 2, gridLayout.cellSize.y / 4, 0)),
                Grid.Swizzle(gridLayout.cellSwizzle, new Vector3(gridLayout.cellSize.x / 2, -gridLayout.cellSize.y / 4, 0)),
                Grid.Swizzle(gridLayout.cellSwizzle, new Vector3(0, -gridLayout.cellSize.y / 2, 0)),
                Grid.Swizzle(gridLayout.cellSwizzle, new Vector3(-gridLayout.cellSize.x / 2, -gridLayout.cellSize.y / 4, 0)),
                Grid.Swizzle(gridLayout.cellSwizzle, new Vector3(-gridLayout.cellSize.x / 2, gridLayout.cellSize.y / 4, 0))
            };


            // Fill Top and Bottom Vertices
            for(int x = area.min.x; x < area.max.x; x++)
            {
                // TODO: Replace this for loop with the FillTopBotVertices.

                cellLocals[bottom++] = gridLayout.CellToLocal(new Vector3Int(x, area.min.y, area.zMin)) + cellOffset[4];
                cellLocals[bottom++] = gridLayout.CellToLocal(new Vector3Int(x, area.min.y, area.zMin)) + cellOffset[3];
                cellLocals[top--] = gridLayout.CellToLocal(new Vector3Int(x, area.max.y - 1, area.zMin)) + cellOffset[0];
                cellLocals[top--] = gridLayout.CellToLocal(new Vector3Int(x, area.max.y - 1, area.zMin)) + cellOffset[1];
            }
            // Fill first Left and Right Vertices
            cellLocals[left--] = gridLayout.CellToLocal(new Vector3Int(area.min.x, area.min.y, area.zMin)) + cellOffset[5];
            cellLocals[top--] = gridLayout.CellToLocal(new Vector3Int(area.max.x - 1, area.max.y - 1, area.zMin)) + cellOffset[2];
            // Fill Left and Right Vertices
            for(int y = area.min.y + 1; y < area.max.y; y++)
            {
                cellLocals[left--] = gridLayout.CellToLocal(new Vector3Int(area.min.x, y, area.zMin)) + cellOffset[4];
                cellLocals[left--] = gridLayout.CellToLocal(new Vector3Int(area.min.x, y, area.zMin)) + cellOffset[5];
            }
            for(int y = area.min.y; y < (area.max.y - 1); y++)
            {
                cellLocals[right++] = gridLayout.CellToLocal(new Vector3Int(area.max.x - 1, y, area.zMin)) + cellOffset[2];
                cellLocals[right++] = gridLayout.CellToLocal(new Vector3Int(area.max.x - 1, y, area.zMin)) + cellOffset[1];
            }

            ApplyHandleMaterial(CompareFunction.Always);
            GL.PushMatrix();
            GL.MultMatrix(gridLayout.transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            GL.Color(color);
            int i = 0;
            for(int j = cellLocals.Length - 1; i < cellLocals.Length; j = i++)
            {
                DrawBatchedLine(cellLocals[j], cellLocals[i]);
            }
            GL.End();
            GL.PopMatrix();
        }

#if UNITY_EDITOR

        //TODO: All the functions here need converted over to the SF Material Handle System.

        //Comes from Unity's Internal GridEditorUtility Class inside of the Unity.2D.Tilemap.Editor assembly.
        public static void DrawGridGizmo(GridLayout gridLayout, Transform transform, Color color, ref Mesh gridMesh, ref Material gridMaterial)
        {
            // TODO: Hook this up with DrawGrid
            if(Event.current.type != EventType.Repaint)
                return;

            if(gridMesh == null)
                gridMesh = GenerateCachedGridMesh(gridLayout, color);

            if(gridMaterial == null)
            {
                gridMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/GridGap.mat");
            }

            if(gridLayout.cellLayout == GridLayout.CellLayout.Hexagon)
            {
                gridMaterial.SetVector("_Gap", new Vector4(1f, 1f / 3f, 1f, 1f));
                gridMaterial.SetVector("_Stride", new Vector4(1f, 1f, 1f, 1f));
            }
            else
            {
                gridMaterial.SetVector("_Gap", gridLayout.cellSize);
                gridMaterial.SetVector("_Stride", gridLayout.cellGap + gridLayout.cellSize);
            }

            gridMaterial.SetPass(0);
            ApplyHandleMaterial(CompareFunction.Always);
            GL.PushMatrix();
            if(gridMesh.GetTopology(0) == MeshTopology.Lines)
                GL.Begin(GL.LINES);
            else
                GL.Begin(GL.QUADS);

            Graphics.DrawMeshNow(gridMesh, transform.localToWorldMatrix);
            GL.End();
            GL.PopMatrix();
        }

        private static Mesh GenerateCachedGridMesh(GridLayout gridLayout, Color color)
        {
            switch(gridLayout.cellLayout)
            {
                case GridLayout.CellLayout.Hexagon:
                    return GenerateCachedHexagonalGridMesh(gridLayout, color);
                case GridLayout.CellLayout.Isometric:
                case GridLayout.CellLayout.IsometricZAsY:
                case GridLayout.CellLayout.Rectangle:
                    int min = k_GridGizmoVertexCount / -32;
                    int max = min * -1;
                    int numCells = max - min;
                    RectInt bounds = new RectInt(min, min, numCells, numCells);

                    return GenerateCachedGridMesh(gridLayout, color, 0f, bounds, MeshTopology.Lines);
            }
            return null;
        }
        public static Mesh GenerateCachedGridMesh(GridLayout gridLayout, Color color, float screenPixelSize, RectInt bounds, MeshTopology topology)
        {
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;

            int vertex = 0;

            int totalVertices = topology == MeshTopology.Quads ?
                8 * (bounds.size.x + bounds.size.y) :
                4 * (bounds.size.x + bounds.size.y);

            Vector3 horizontalPixelOffset = new Vector3(screenPixelSize, 0f, 0f);
            Vector3 verticalPixelOffset = new Vector3(0f, screenPixelSize, 0f);

            Vector3[] vertices = new Vector3[totalVertices];
            Vector2[] uvs2 = new Vector2[totalVertices];

            Vector3 cellStride = gridLayout.cellSize + gridLayout.cellGap;
            Vector3Int minPosition = new Vector3Int(0, bounds.min.y, 0);
            Vector3Int maxPosition = new Vector3Int(0, bounds.max.y, 0);

            Vector3 cellGap = Vector3.zero;
            if(!Mathf.Approximately(cellStride.x, 0f))
            {
                cellGap.x = gridLayout.cellSize.x / cellStride.x;
            }

            for(int x = bounds.min.x; x < bounds.max.x; x++)
            {
                minPosition.x = x;
                maxPosition.x = x;

                vertices[vertex + 0] = gridLayout.CellToLocal(minPosition);
                vertices[vertex + 1] = gridLayout.CellToLocal(maxPosition);
                uvs2[vertex + 0] = Vector2.zero;
                uvs2[vertex + 1] = new Vector2(0f, cellStride.y * bounds.size.y);
                if(topology == MeshTopology.Quads)
                {
                    vertices[vertex + 2] = gridLayout.CellToLocal(maxPosition) + horizontalPixelOffset;
                    vertices[vertex + 3] = gridLayout.CellToLocal(minPosition) + horizontalPixelOffset;
                    uvs2[vertex + 2] = new Vector2(0f, cellStride.y * bounds.size.y);
                    uvs2[vertex + 3] = Vector2.zero;
                }
                vertex += topology == MeshTopology.Quads ? 4 : 2;

                vertices[vertex + 0] = gridLayout.CellToLocalInterpolated(minPosition + cellGap);
                vertices[vertex + 1] = gridLayout.CellToLocalInterpolated(maxPosition + cellGap);
                uvs2[vertex + 0] = Vector2.zero;
                uvs2[vertex + 1] = new Vector2(0f, cellStride.y * bounds.size.y);
                if(topology == MeshTopology.Quads)
                {
                    vertices[vertex + 2] = gridLayout.CellToLocalInterpolated(maxPosition + cellGap) + horizontalPixelOffset;
                    vertices[vertex + 3] = gridLayout.CellToLocalInterpolated(minPosition + cellGap) + horizontalPixelOffset;
                    uvs2[vertex + 2] = new Vector2(0f, cellStride.y * bounds.size.y);
                    uvs2[vertex + 3] = Vector2.zero;
                }
                vertex += topology == MeshTopology.Quads ? 4 : 2;
            }

            minPosition = new Vector3Int(bounds.min.x, 0, 0);
            maxPosition = new Vector3Int(bounds.max.x, 0, 0);
            cellGap = Vector3.zero;
            if(!Mathf.Approximately(cellStride.y, 0f))
            {
                cellGap.y = gridLayout.cellSize.y / cellStride.y;
            }

            for(int y = bounds.min.y; y < bounds.max.y; y++)
            {
                minPosition.y = y;
                maxPosition.y = y;

                vertices[vertex + 0] = gridLayout.CellToLocal(minPosition);
                vertices[vertex + 1] = gridLayout.CellToLocal(maxPosition);
                uvs2[vertex + 0] = Vector2.zero;
                uvs2[vertex + 1] = new Vector2(cellStride.x * bounds.size.x, 0f);
                if(topology == MeshTopology.Quads)
                {
                    vertices[vertex + 2] = gridLayout.CellToLocal(maxPosition) + verticalPixelOffset;
                    vertices[vertex + 3] = gridLayout.CellToLocal(minPosition) + verticalPixelOffset;
                    uvs2[vertex + 2] = new Vector2(cellStride.x * bounds.size.x, 0f);
                    uvs2[vertex + 3] = Vector2.zero;
                }
                vertex += topology == MeshTopology.Quads ? 4 : 2;

                vertices[vertex + 0] = gridLayout.CellToLocalInterpolated(minPosition + cellGap);
                vertices[vertex + 1] = gridLayout.CellToLocalInterpolated(maxPosition + cellGap);
                uvs2[vertex + 0] = Vector2.zero;
                uvs2[vertex + 1] = new Vector2(cellStride.x * bounds.size.x, 0f);
                if(topology == MeshTopology.Quads)
                {
                    vertices[vertex + 2] = gridLayout.CellToLocalInterpolated(maxPosition + cellGap) + verticalPixelOffset;
                    vertices[vertex + 3] = gridLayout.CellToLocalInterpolated(minPosition + cellGap) + verticalPixelOffset;
                    uvs2[vertex + 2] = new Vector2(cellStride.x * bounds.size.x, 0f);
                    uvs2[vertex + 3] = Vector2.zero;
                }
                vertex += topology == MeshTopology.Quads ? 4 : 2;
            }

            var uv0 = new Vector2(k_GridGizmoDistanceFalloff, 0f);
            var uvs = new Vector2[vertex];
            var indices = new int[vertex];
            var colors = new Color[vertex];
            var normals = new Vector3[totalVertices];     // Normal channel stores the position of the other end point of the line.
            var uvs3 = new Vector2[totalVertices];        // UV3 channel stores the UV2 value of the other end point of the line.

            for(int i = 0; i < vertex; i++)
            {
                uvs[i] = uv0;
                indices[i] = i;
                colors[i] = color;
                var alternate = i + ((i % 2) == 0 ? 1 : -1);
                normals[i] = vertices[alternate];
                uvs3[i] = uvs2[alternate];
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.uv2 = uvs2;
            mesh.uv3 = uvs3;
            mesh.colors = colors;
            mesh.normals = normals;
            mesh.SetIndices(indices, topology, 0);

            return mesh;
        }
        private static Mesh GenerateCachedHexagonalGridMesh(GridLayout gridLayout, Color color)
        {
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            int vertex = 0;
            int max = k_GridGizmoVertexCount / (2 * (6 * 2));
            max = (max / 4) * 4;
            int min = -max;
            float numVerticalCells = 6 * (max / 4);
            int totalVertices = max * 2 * 6 * 2;
            var cellStrideY = gridLayout.cellGap.y + gridLayout.cellSize.y;
            var cellOffsetY = gridLayout.cellSize.y / 2;
            var hexOffset = (1.0f / 3.0f);
            var drawTotal = numVerticalCells * 2.0f * hexOffset;
            var drawDiagTotal = 2 * drawTotal;
            Vector3[] vertices = new Vector3[totalVertices];
            Vector2[] uvs2 = new Vector2[totalVertices];
            // Draw Vertical Lines
            for(int x = min; x < max; x++)
            {
                vertices[vertex] = gridLayout.CellToLocal(new Vector3Int(x, min, 0));
                vertices[vertex + 1] = gridLayout.CellToLocal(new Vector3Int(x, max, 0));
                uvs2[vertex] = new Vector2(0f, 2 * hexOffset);
                uvs2[vertex + 1] = new Vector2(0f, 2 * hexOffset + drawTotal);
                vertex += 2;
                // Alternate Row Offset
                vertices[vertex] = gridLayout.CellToLocal(new Vector3Int(x, min - 1, 0));
                vertices[vertex + 1] = gridLayout.CellToLocal(new Vector3Int(x, max - 1, 0));
                uvs2[vertex] = new Vector2(0f, 2 * hexOffset);
                uvs2[vertex + 1] = new Vector2(0f, 2 * hexOffset + drawTotal);
                vertex += 2;
            }
            // Draw Diagonals
            for(int y = min; y < max; y++)
            {
                float drawDiagOffset = ((y + 1) % 3) * hexOffset;
                var cellOffSet = Grid.Swizzle(gridLayout.cellSwizzle, new Vector3(0f, y * cellStrideY + cellOffsetY, 0.0f));
                // Slope Up
                vertices[vertex] = gridLayout.CellToLocal(new Vector3Int(Mathf.RoundToInt(1.5f * min), min, 0)) + cellOffSet;
                vertices[vertex + 1] = gridLayout.CellToLocal(new Vector3Int(Mathf.RoundToInt(1.5f * max), max, 0)) + cellOffSet;
                uvs2[vertex] = new Vector2(0f, drawDiagOffset);
                uvs2[vertex + 1] = new Vector2(0f, drawDiagOffset + drawDiagTotal);
                vertex += 2;
                // Slope Down
                vertices[vertex] = gridLayout.CellToLocal(new Vector3Int(Mathf.RoundToInt(1.5f * max), min, 0)) + cellOffSet;
                vertices[vertex + 1] = gridLayout.CellToLocal(new Vector3Int(Mathf.RoundToInt(1.5f * min), max, 0)) + cellOffSet;
                uvs2[vertex] = new Vector2(0f, drawDiagOffset);
                uvs2[vertex + 1] = new Vector2(0f, drawDiagOffset + drawDiagTotal);
                vertex += 2;
                // Alternate Row Offset
                vertices[vertex] = gridLayout.CellToLocal(new Vector3Int(Mathf.RoundToInt(1.5f * min) + 1, min, 0)) + cellOffSet;
                vertices[vertex + 1] = gridLayout.CellToLocal(new Vector3Int(Mathf.RoundToInt(1.5f * max) + 1, max, 0)) + cellOffSet;
                uvs2[vertex] = new Vector2(0f, drawDiagOffset);
                uvs2[vertex + 1] = new Vector2(0f, drawDiagOffset + drawDiagTotal);
                vertex += 2;
                vertices[vertex] = gridLayout.CellToLocal(new Vector3Int(Mathf.RoundToInt(1.5f * max) + 1, min, 0)) + cellOffSet;
                vertices[vertex + 1] = gridLayout.CellToLocal(new Vector3Int(Mathf.RoundToInt(1.5f * min) + 1, max, 0)) + cellOffSet;
                uvs2[vertex] = new Vector2(0f, drawDiagOffset);
                uvs2[vertex + 1] = new Vector2(0f, drawDiagOffset + drawDiagTotal);
                vertex += 2;
            }
            var uv0 = new Vector2(k_GridGizmoDistanceFalloff, 0f);
            var indices = new int[totalVertices];
            var uvs = new Vector2[totalVertices];
            var colors = new Color[totalVertices];
            var normals = new Vector3[totalVertices];     // Normal channel stores the position of the other end point of the line.
            var uvs3 = new Vector2[totalVertices];        // UV3 channel stores the UV2 value of the other end point of the line.

            for(int i = 0; i < totalVertices; i++)
            {
                uvs[i] = uv0;
                indices[i] = i;
                colors[i] = color;
                var alternate = i + ((i % 2) == 0 ? 1 : -1);
                normals[i] = vertices[alternate];
                uvs3[i] = uvs2[alternate];
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.uv2 = uvs2;
            mesh.uv3 = uvs3;
            mesh.colors = colors;
            mesh.normals = normals;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            return mesh;
        }
#endif

    }
}
