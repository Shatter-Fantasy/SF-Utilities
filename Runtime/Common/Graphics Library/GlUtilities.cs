using UnityEngine;

namespace SF.Utilities
{
    public static class GLUtilities
    {
        public static Color GLColor;

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
    }
}
