using System.Collections.Generic;
using Miscellaneous;

namespace DelaunayVoronoi
{
	[System.Serializable]
	public struct DiagramSetting
	{
		public bool createHull;
		public bool fromBarycenter;

		public DiagramSetting(bool fromBarycenter = false, bool createHull = false)
		{
			this.createHull = createHull;
			this.fromBarycenter = fromBarycenter;
		}
	}

	public class VoronoiCalculator
	{
		private class CellNode
		{
			public int si = -1;
			public List<int> heis = new List<int>();
		}

		private bool createHull = false;
		private bool fromBarycenter = false;
		private DelaunayTriangulation triangulation = null;

		private int siteCount = 0;
		private List<Point2D> pointList = new List<Point2D>();

		private CellNode[] cellNodes = null;
		private List<int> triangleIndiceList = new List<int>();
		private List<int> halfEdgeIndexToProcessList = new List<int>();
		private List<VoronoiDiagram.HalfEdge> halfEdgeList = new List<VoronoiDiagram.HalfEdge>();

		private void AddPoint(float x, float y)
		{
			pointList.Add(new Point2D
			{
				x = x,
				y = y
			});
		}
		private void AddHalfEdge(int pi0, int pi1, int ci)
		{
			halfEdgeList.Add(new VoronoiDiagram.HalfEdge
			{
				pi0 = pi0,
				pi1 = pi1,

				ci = ci
			});

			cellNodes[ci].heis.Add(halfEdgeList.Count - 1);

			halfEdgeIndexToProcessList.Add(halfEdgeList.Count - 1);
		}
		private void AddHalfEdge(int pi0, int pi1, int ci, int ohei)
		{
			halfEdgeList.Add(new VoronoiDiagram.HalfEdge
			{
				pi0 = pi0,
				pi1 = pi1,

				ci = ci,

				ohei = ohei
			});
			cellNodes[ci].heis.Add(halfEdgeList.Count - 1);
		}
		private void AddTriangle(int index1, int index2, int index3)
		{
			triangleIndiceList.Add(index1);
			triangleIndiceList.Add(index2);
			triangleIndiceList.Add(index3);
		}

		private void ComputeDiagramHalfEdgeForTriangleHalfEdge(int hei)
		{
			var points = triangulation.points;
			var triangles = triangulation.triangles;
			var halfEdges = triangulation.halfEdges;

			var halfEdge = halfEdges[hei];
			var triangle = triangles[halfEdge.ti];

			if (halfEdge.ohei != -1)
			{
				var otherTriangle = triangles[halfEdges[halfEdge.ohei].ti];

				AddHalfEdge(triangle.bci, otherTriangle.bci, halfEdge.pi1);

				AddTriangle(siteCount + triangle.bci, siteCount + otherTriangle.bci, halfEdge.pi1);
			}
			else if (createHull)
			{
				var p0 = points[halfEdge.pi0];
				var p1 = points[halfEdge.pi1];

				AddPoint((p0.x + p1.x) * 0.5f, (p0.y + p1.y) * 0.5f);

				AddHalfEdge(triangle.bci, pointList.Count - 1 - siteCount, halfEdge.pi1, halfEdgeList.Count + 1);

				AddTriangle(siteCount + triangle.bci, pointList.Count - 1, halfEdge.pi1);

				AddHalfEdge(pointList.Count - 1 - siteCount, triangle.bci, halfEdge.pi0, halfEdgeList.Count - 1);

				AddTriangle(pointList.Count - 1, siteCount + triangle.bci, halfEdge.pi0);
			}
		}
		private void ComputeDiagramHalfEdgesForTriangle(int ti)
		{
			var triangle = triangulation.triangles[ti];

			//First triangle HalfEdge
			ComputeDiagramHalfEdgeForTriangleHalfEdge(triangle.hei0);
			//Second triangle HalfEdge
			ComputeDiagramHalfEdgeForTriangleHalfEdge(triangle.hei1);
			//Third triangle HalfEdge
			ComputeDiagramHalfEdgeForTriangleHalfEdge(triangle.hei2);
		}

		private void ProcessHalfEdgeIndexList()
		{
			while (halfEdgeIndexToProcessList.Count > 0)
			{
				var current = halfEdgeIndexToProcessList[0];

				for (int i = 1; i < halfEdgeIndexToProcessList.Count; i++)
				{
					var tmp = halfEdgeIndexToProcessList[i];
					if (halfEdgeList[current].pi0 == halfEdgeList[tmp].pi1 &&
						halfEdgeList[current].pi1 == halfEdgeList[tmp].pi0)
					{
						halfEdgeList[current].ohei = tmp;
						halfEdgeList[tmp].ohei = current;

						halfEdgeIndexToProcessList.RemoveAt(i);

						break;
					}
				}

				halfEdgeIndexToProcessList.RemoveAt(0);
			}
		}

		private void InitializeCellNodes()
		{
			cellNodes = new CellNode[triangulation.points.Length];
			for (int i = 0; i < cellNodes.Length; i++)
			{
				cellNodes[i] = new CellNode
				{
					si = i
				};
			}
		}
		private void InitializePointList()
		{
			siteCount = triangulation.points.Length;
			pointList.AddRange(triangulation.points);

			if (fromBarycenter)
				pointList.AddRange(triangulation.barycenters);
			else
				pointList.AddRange(triangulation.circumcenters);
		}

		private void ComputeDiagramHalfEdges()
		{
			var triangles = triangulation.triangles;
			for (int i = 0; i < triangles.Length; i++)
			{
				ComputeDiagramHalfEdgesForTriangle(i);
			}

			ProcessHalfEdgeIndexList();
		}
		private void GenerateResult(out VoronoiDiagram diagram)
		{
			var cells = new VoronoiDiagram.Cell[cellNodes.Length];
			for (int i = 0; i < cellNodes.Length; i++)
			{
				cells[i] = new VoronoiDiagram.Cell
				{
					si = cellNodes[i].si,

					hei = cellNodes[i].heis.ToArray()
				};
			}

			diagram = new VoronoiDiagram(pointList.ToArray(), siteCount, triangleIndiceList.ToArray(), cells, halfEdgeList.ToArray());
		}

		private void Clear()
		{
			createHull = false;
			fromBarycenter = false;
			triangulation = null;

			siteCount = 0;
			pointList.Clear();

			cellNodes = null;
			triangleIndiceList.Clear();
			halfEdgeIndexToProcessList.Clear();
			halfEdgeList.Clear();
		}

		public void CalculateDiagram(DelaunayTriangulation triangulation, out VoronoiDiagram diagram, bool fromBarycenter = false, bool createHull = false)
		{
			this.createHull = createHull;
			this.triangulation = triangulation;
			this.fromBarycenter = fromBarycenter;

			InitializeCellNodes();

			InitializePointList();

			ComputeDiagramHalfEdges();

			GenerateResult(out diagram);

			Clear();
		}
		public void CalculateDiagram(DelaunayTriangulation triangulation, out VoronoiDiagram diagram, DiagramSetting setting)
		{
			CalculateDiagram(triangulation, out diagram, setting.fromBarycenter, setting.createHull);
		}
	}
}
