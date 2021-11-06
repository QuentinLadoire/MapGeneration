using System.Collections.Generic;

using PoissonDisk;

namespace Delaunay
{
	public class HalfEdge
	{
		public int pi0 = -1;
		public int pi1 = -1;

		public int ti = -1;

		public int ohei = -1;
	}

	public class Triangle
	{
		public int hei0 = -1;
		public int hei1 = -1;
		public int hei2 = -1;

		public int cci = -1;
		public int bci = -1;
	}

	public class DelaunayTriangulation
	{
		public Point2D[] points = null;
		public int[] indices = null;

		public Triangle[] triangles = null;
		public HalfEdge[] halfEdges = null;

		public Point2D[] barycenters = null;
		public Point2D[] circumcenters = null;

		public DelaunayTriangulation(Point2D[] points, int[] indices)
		{
			this.points = points;
			this.indices = indices;
		}

		public void CalculateDataStruct()
		{
			var triangleCount = indices.Length / 3;
			triangles = new Triangle[triangleCount];

			var halfEdgeCount = indices.Length;
			halfEdges = new HalfEdge[halfEdgeCount];

			barycenters = new Point2D[triangleCount];
			circumcenters = new Point2D[triangleCount];

			var processlist = new List<int>();

			var index = 0;
			for (int i = 0; i < indices.Length; i += 3)
			{
				processlist.Add(i);
				processlist.Add(i + 1);
				processlist.Add(i + 2);

				halfEdges[i] = new HalfEdge
				{
					pi0 = indices[i],
					pi1 = indices[i + 1],
					
					ti = index
				};
				halfEdges[i + 1] = new HalfEdge
				{
					pi0 = indices[i + 1],
					pi1 = indices[i + 2],

					ti = index
				};
				halfEdges[i + 2] = new HalfEdge
				{
					pi0 = indices[i + 2],
					pi1 = indices[i],

					ti = index
				};

				barycenters[index] = Utility.CalculateBarycenter(points[indices[i]], points[indices[i + 1]], points[indices[i + 2]]);
				circumcenters[index] = Utility.CalculateCircumcenter(points[indices[i]], points[indices[i + 1]], points[indices[i + 2]]);

				triangles[index] = new Triangle
				{
					hei0 = i,
					hei1 = i + 1,
					hei2 = i + 2,

					bci = index,
					cci = index
				};

				index++;
			}

			while (processlist.Count > 0)
			{
				var current = processlist[0];

				for (int i = 0; i < processlist.Count; i++)
				{
					var tmp = processlist[i];
					if (halfEdges[current].pi0 == halfEdges[tmp].pi1 &&
						halfEdges[current].pi1 == halfEdges[tmp].pi0)
					{
						halfEdges[current].ohei = tmp;
						halfEdges[tmp].ohei = current;

						processlist.RemoveAt(i);

						break;
					}
				}

				processlist.RemoveAt(0);
			}
		}
	}
}
