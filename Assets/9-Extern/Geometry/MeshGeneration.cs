using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
	public class MeshData
	{
		public int[] triangles = null;
		public Vector3[] vertices = null;

		public MeshData(Vector3[] vertices, int[] triangles)
		{
			this.vertices = vertices;
			this.triangles = triangles;
		}
	}

	public static class MeshGenerator
	{
		private const float goldenRatio = 1.618033989f;

		public static MeshData CreateIcosahedron(float radius = 1.0f)
		{
			var vertices = new Vector3[]
			{
				new Vector3(-1.0f,  goldenRatio, 0.0f).normalized * radius,
				new Vector3( 1.0f,  goldenRatio, 0.0f).normalized * radius,
				new Vector3(-1.0f, -goldenRatio, 0.0f).normalized * radius,
				new Vector3( 1.0f, -goldenRatio, 0.0f).normalized * radius,

				new Vector3(-goldenRatio, 0.0f,  1.0f).normalized * radius,
				new Vector3( goldenRatio, 0.0f,  1.0f).normalized * radius,
				new Vector3(-goldenRatio, 0.0f, -1.0f).normalized * radius,
				new Vector3( goldenRatio, 0.0f, -1.0f).normalized * radius,

				new Vector3(0.0f, -1.0f,  goldenRatio).normalized * radius,
				new Vector3(0.0f,  1.0f,  goldenRatio).normalized * radius,
				new Vector3(0.0f, -1.0f, -goldenRatio).normalized * radius,
				new Vector3(0.0f,  1.0f, -goldenRatio).normalized * radius
			};
			var triangles = new int[]
			{
				0, 1, 11,
				0, 11, 6,
				0, 6, 4,
				0, 4, 9,
				0, 9, 1,

				7, 11, 1,
				7, 1, 5,
				7, 5, 3,
				7, 3, 10,
				7, 10, 11,

				2, 4, 6,
				2, 6, 10,
				2, 10, 3,
				2, 3, 8,
				2, 8, 4,

				11, 10, 6,
				1, 9, 5,
				9, 8, 5,
				5, 8, 3,
				8, 9, 4
			};

			return new MeshData(vertices, triangles);
		}
		public static MeshData CreateIcoSphere(float radius = 1.0f, int refiningStep = 3)
		{
			var ico = CreateIcosahedron(radius);

			var triangles = ico.triangles;
			var vertices = new List<Vector3>(ico.vertices);

			for (int j = 0; j < refiningStep; j++)
			{
				var newTriangles = new List<int>();
				var verticesCache = new Dictionary<Vector3, int>();
				for (int i = 0; i < triangles.Length; i += 3)
				{
					var pi0 = triangles[i];
					var pi1 = triangles[i + 1];
					var pi2 = triangles[i + 2];

					var p0 = vertices[pi0];
					var p1 = vertices[pi1];
					var p2 = vertices[pi2];

					var mp0 = (p0 + p1) * 0.5f;
					var mp1 = (p1 + p2) * 0.5f;
					var mp2 = (p2 + p0) * 0.5f;

					if (!verticesCache.TryGetValue(mp0, out int mpi0))
					{
						mpi0 = vertices.Count;
						verticesCache.Add(mp0, mpi0);

						mp0.Normalize();
						mp0 *= radius;
						vertices.Add(mp0);
					}
					if (!verticesCache.TryGetValue(mp1, out int mpi1))
					{
						mpi1 = vertices.Count;
						verticesCache.Add(mp1, mpi1);

						mp1.Normalize();
						mp1 *= radius;
						vertices.Add(mp1);
					}
					if (!verticesCache.TryGetValue(mp2, out int mpi2))
					{
						mpi2 = vertices.Count;
						verticesCache.Add(mp2, mpi2);

						mp2.Normalize();
						mp2 *= radius;
						vertices.Add(mp2);
					}

					newTriangles.Add(mpi0);
					newTriangles.Add(mpi1);
					newTriangles.Add(mpi2);

					newTriangles.Add(mpi0);
					newTriangles.Add(pi1);
					newTriangles.Add(mpi1);

					newTriangles.Add(mpi1);
					newTriangles.Add(pi2);
					newTriangles.Add(mpi2);

					newTriangles.Add(mpi2);
					newTriangles.Add(pi0);
					newTriangles.Add(mpi0);
				}

				triangles = newTriangles.ToArray();

				newTriangles.Clear();
				verticesCache.Clear();
			}

			return new MeshData(vertices.ToArray(), triangles);
		}
	}
}
