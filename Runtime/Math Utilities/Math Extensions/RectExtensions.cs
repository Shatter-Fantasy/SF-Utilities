using UnityEngine;

namespace SF.Utilities
{
    public static class RectExtensions 
    {
        /// <summary>
        /// Makes sure if the rect has a negative width or height to readjust it's values to make them positive without moving the rect's occupied space.
        /// This will move the starting points of the rect in some cases but the occupied space will be the same in the end.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rect AbsoluteRect(this Rect rect)
        {
            // Make sure width is not negative
            if(rect.height < 0)
            {
                rect.y -= Mathf.Abs(rect.height);
                rect.height = Mathf.Abs(rect.height);
            }
            // Make sure height is not negative.
            if(rect.width < 0)
            {
                rect.x -= Mathf.Abs(rect.width);
                rect.width = Mathf.Abs(rect.width);
            }

            return rect;
        }

        /// <summary>
        /// Gets the area of the Rect from it's width and height.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static float GetArea(this Rect rect)
        {
            // Make sure first there is no negative values ot mess with the area calculations.
            rect = rect.AbsoluteRect();
            return rect.width * rect.height;
        }
    }
}
