using UnityEngine;
using UnityEngine.Rendering;

namespace SF.Utilities
{
    public static class GLUtilities
    {
        public static Color GLColor;
        public static Material DrawHandleMaterial;

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



    }
}
