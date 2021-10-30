using PoissonDisk;

namespace Delaunay
{
	public class DelaunayTriangulation
	{
		public Point2D[] points = null;
		public int[] triangles = null;

		public DelaunayTriangulation(Point2D[] points, int[] triangles)
		{
			this.points = points;
			this.triangles = triangles;
		}
	}
}
