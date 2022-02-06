using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
	public static class GeometryUtility
	{
		private const float oneThird = 1.0f / 3.0f;

		public static Vector3 CalculateBarycenter(Vector3 v0, Vector3 v1, Vector3 v2)
		{
			return new Vector3
			{
				x = (v0.x + v1.x + v2.x) * oneThird,
				y = (v0.y + v1.y + v2.y) * oneThird,
				z = (v0.z + v1.z + v2.z) * oneThird
			};
		}
		public static Vector3 CalculateBarycenter(Vector3[] polygon)
		{
			var result = Vector3.zero;
			for (int i = 0; i < polygon.Length; i++)
				result += polygon[i];

			return result / polygon.Length;
		}

		public static Vector3 ProjectOnUnitSphere(Vector3 vector)
		{
			return vector.normalized;
		}
		public static Vector3 ProjectOnSphere(Vector3 vector, float radius)
		{
			return ProjectOnUnitSphere(vector) * radius;
		}

		public static float TriangleArea(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			var v0 = p1 - p0;
			var v1 = p2 - p0;

			return 0.5f * Vector3.Cross(v0, v1).magnitude;
		}
	}
}
