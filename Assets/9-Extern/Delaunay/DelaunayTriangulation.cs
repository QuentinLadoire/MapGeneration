using System.Collections.Generic;
using Miscellaneous;

namespace DelaunayVoronoi
{
	public class DelaunayTriangulation
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
	}
}
