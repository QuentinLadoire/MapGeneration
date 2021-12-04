using Miscellaneous;

namespace DelaunayVoronoi
{
    public class VoronoiDiagram
    {
        public class HalfEdge
        {
            public int pi0 = -1;
            public int pi1 = -1;

            public int ci = -1;

            public int ohei = -1;
        }

        public class Cell
        {
            public int si = -1;
            public int[] hei = null;
        }

        public Cell[] cells = null;
        public Point2D[] sites = null;
        public Point2D[] points = null;
        public HalfEdge[] halfEdges = null;

        public VoronoiDiagram(Point2D[] sites, Point2D[] points, Cell[] cells, HalfEdge[] halfEdges)
		{
            this.sites = sites;
            this.points = points;
            this.cells = cells;
            this.halfEdges = halfEdges;
		}
    }
}
