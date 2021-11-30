using UnityEngine;
using Miscellaneous;

namespace Miscellaneous.Unity
{
    public static class Point2DExtension
    {
        public static Vector2 ToVector2(this Point2D point)
		{
            return new Vector2(point.x, point.y);
		}
        public static Vector3 ToVector3(this Point2D point)
		{
            return new Vector3(point.x, point.y, 0.0f);
        }

        public static Vector2[] ToVector2Array(this Point2D[] points)
		{
            var array = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
                array[i] = points[i].ToVector2();

            return array;
		}
        public static Vector3[] ToVector3Array(this Point2D[] points)
        {
            var array = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
                array[i] = points[i].ToVector3();

            return array;
        }
    }
}
