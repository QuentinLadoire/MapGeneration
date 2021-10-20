using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK
{
    public static class DelaunayExtension
    {
        public static void FlipTriangles(this DelaunayTriangulation triangulation)
		{
            triangulation.Triangles.Reverse();
		}
    }
}
