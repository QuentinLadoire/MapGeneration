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

        private int siteCount = 0;
        private int pointCount = 0;

        public int[] indices = null;
        public Point2D[] allPoints = null;

        public Cell[] cells = null;
        public HalfEdge[] halfEdges = null;

        public int SiteCount => siteCount;
        public int PointCount => pointCount;

        public Point2D GetSiteAt(int index)
		{
            return allPoints[index];
		}
        public Point2D GetPointAt(int index)
		{
            return allPoints[siteCount + index];
		}

        public int GetRealPointIndex(int index)
		{
            return siteCount + index;
        }

        public VoronoiDiagram(Point2D[] allPoints, int siteCount, int[] indices, Cell[] cells, HalfEdge[] halfEdges)
		{
            this.allPoints = allPoints;
            this.siteCount = siteCount;
            this.pointCount = allPoints.Length - siteCount;

            this.indices = indices;

            this.cells = cells;
            this.halfEdges = halfEdges;
		}
    }
}
