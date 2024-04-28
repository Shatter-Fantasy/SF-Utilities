using UnityEngine;

namespace SF.Utilities
{
    public static class BoundExtensions
    {
        #region bounds
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
    }
}
