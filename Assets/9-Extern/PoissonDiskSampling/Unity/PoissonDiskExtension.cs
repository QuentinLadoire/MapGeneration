using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoissonDisk.Unity
{
    public static class PoissonDiskExtension
    {
        public static Vector2 ToVector2(this Point2D point)
		{
            return new Vector2(point.x, point.y);
		}
        public static Vector2[] ToVector2Array(this Point2D[] points)
		{
            var array = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
                array[i] = points[i].ToVector2();

            return array;
		}
    }
}
