using UnityEngine;

namespace SF.Utilities
{
    public static class BoundExtensions
    {
        #region Bounds
        public static Vector2 TopRight(this Bounds bounds) => new(bounds.max.x, bounds.max.y);
        public static Vector2 TopCenter(this Bounds bounds) => new(bounds.center.x, bounds.max.y);
        public static Vector2 TopLeft(this Bounds bounds) => new(bounds.min.x, bounds.max.y);

        public static Vector2 BottomRight(this Bounds bounds) => new(bounds.max.x, bounds.min.y);
        public static Vector2 BottomCenter(this Bounds bounds) => new(bounds.center.x, bounds.min.y);
        public static Vector2 BottomLeft(this Bounds bounds) => new(bounds.min.x, bounds.min.y);

        public static Vector2 MiddleRight(this Bounds bounds) => new(bounds.max.x, bounds.center.y);
        public static Vector2 MiddleCenter(this Bounds bounds) => new(bounds.center.x, bounds.center.y);
        public static Vector2 MiddleLeft(this Bounds bounds) => new(bounds.min.x, bounds.center.y);
        #endregion

        #region BoundInt
        public static Vector2 TopRight(this BoundsInt bounds) => new(bounds.max.x, bounds.max.y);
        public static Vector2 TopCenter(this BoundsInt bounds) => new(bounds.center.x, bounds.max.y);
        public static Vector2 TopLeft(this BoundsInt bounds) => new(bounds.min.x, bounds.max.y);

        public static Vector2 BottomRight(this BoundsInt bounds) => new(bounds.max.x, bounds.min.y);
        public static Vector2 BottomCenter(this BoundsInt bounds) => new(bounds.center.x, bounds.min.y);
        public static Vector2 BottomLeft(this BoundsInt bounds) => new(bounds.min.x, bounds.min.y);

        public static Vector2 MiddleRight(this BoundsInt bounds) => new(bounds.max.x, bounds.center.y);
        public static Vector2 MiddleCenter(this BoundsInt bounds) => new(bounds.center.x, bounds.center.y);
        public static Vector2 MiddleLeft(this BoundsInt bounds) => new(bounds.min.x, bounds.center.y);
        #endregion

        #region Bounds Conversion
        public static Bounds ToBounds(this BoundsInt boundsInt)
        {
            Bounds bounds = new(boundsInt.size, boundsInt.center);
            return bounds;
        }

        public static BoundsInt ToBoundsInt(this Bounds bounds)
        {
            BoundsInt boundsInt = new(bounds.GetBorderMinInt(),bounds.GetBorderSizeInt());
            return boundsInt;
        }

        public static Vector3Int GetBorderMinInt(this Bounds bounds)
        {

            Vector3Int vector3Int = new((int)bounds.min.x,(int)bounds.min.y,(int)bounds.min.z);
            return vector3Int;
        }

        public static void Deconstruct(this Vector3Int vector3Int, out int x, out int y, out int z)
        {
            x = vector3Int.x;
            y = vector3Int.y;
            z = vector3Int.z;
        }

        public static Vector3Int GetBorderSizeInt(this Bounds bounds)
        {
            Vector3Int vector3Int = new((int)bounds.size.x, (int)bounds.size.y, (int)bounds.size.z);
            return vector3Int;
        }
        #endregion
    }
}
