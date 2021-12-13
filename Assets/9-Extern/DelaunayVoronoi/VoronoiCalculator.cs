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

		private CellNode[] cellNodes = null;
		private List<Point2D> pointList = new List<Point2D>();
		private List<VoronoiDiagram.HalfEdge> halfEdgeList = new List<VoronoiDiagram.HalfEdge>();

		private DelaunayTriangulation triangulation = null;
		private Point2D[] Points => triangulation.points;
		private DelaunayTriangulation.Triangle[] Triangles => triangulation.triangles;
		private DelaunayTriangulation.HalfEdge[] HalfEdges => triangulation.halfEdges;

		private void ComputeBarycenterTriangleHalfEdge(int hei)
		{
			var halfEdge = HalfEdges[hei];
			var triangle = Triangles[halfEdge.ti];

			if (halfEdge.ohei != -1)
			{
				var otherTriangle = Triangles[HalfEdges[halfEdge.ohei].ti];
				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.bci,
					pi1 = otherTriangle.bci,

					ci = halfEdge.pi1
				});

				cellNodes[halfEdge.pi1].heis.Add(halfEdgeList.Count - 1);
			}
			else if (createHull)
			{
				var p0 = Points[halfEdge.pi0];
				var p1 = Points[halfEdge.pi1];

				pointList.Add(new Point2D
				{
					x = (p0.x + p1.x) * 0.5f,
					y = (p0.y + p1.y) * 0.5f
				});

				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.bci,
					pi1 = pointList.Count - 1,

					ci = halfEdge.pi1
				});
				cellNodes[halfEdge.pi1].heis.Add(halfEdgeList.Count - 1);

				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = pointList.Count - 1,
					pi1 = triangle.bci,

					ci = halfEdge.pi0
				});
				cellNodes[halfEdge.pi0].heis.Add(halfEdgeList.Count - 1);
			}
		}
		private void ComputeCircumcenterTriangleHalfEdge(int hei)
		{
			var halfEdge = HalfEdges[hei];
			var triangle = Triangles[halfEdge.ti];

			if (halfEdge.ohei != -1)
			{
				var otherTriangle = Triangles[HalfEdges[halfEdge.ohei].ti];
				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.cci,
					pi1 = otherTriangle.cci,

					ci = halfEdge.pi1
				});

				cellNodes[halfEdge.pi1].heis.Add(halfEdgeList.Count - 1);
			}
			else if (createHull)
			{
				var p0 = Points[halfEdge.pi0];
				var p1 = Points[halfEdge.pi1];

				pointList.Add(new Point2D
				{
					x = (p0.x + p1.x) * 0.5f,
					y = (p0.y + p1.y) * 0.5f
				});

				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.cci,
					pi1 = pointList.Count - 1,

					ci = halfEdge.pi1
				});
				cellNodes[halfEdge.pi1].heis.Add(halfEdgeList.Count - 1);

				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = pointList.Count - 1,
					pi1 = triangle.cci,

					ci = halfEdge.pi0
				});
				cellNodes[halfEdge.pi0].heis.Add(halfEdgeList.Count - 1);
			}
		}

		private void ComputeBarycenterHalfEdgesForTriangle(int ti)
		{
			var triangle = Triangles[ti];

			//First triangle HalfEdge
			ComputeBarycenterTriangleHalfEdge(triangle.hei0);
			//Second triangle HalfEdge
			ComputeBarycenterTriangleHalfEdge(triangle.hei1);
			//Third triangle HalfEdge
			ComputeBarycenterTriangleHalfEdge(triangle.hei2);
		}
		private void ComputeCircumcenterHalfEdgesForTriangle(int ti)
		{
			var triangle = Triangles[ti];

			//First triangle HalfEdge
			ComputeCircumcenterTriangleHalfEdge(triangle.hei0);
			//Second triangle HalfEdge
			ComputeCircumcenterTriangleHalfEdge(triangle.hei1);
			//Third triangle HalfEdge
			ComputeCircumcenterTriangleHalfEdge(triangle.hei2);
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
			if (fromBarycenter)
				pointList.AddRange(triangulation.barycenters);
			else
				pointList.AddRange(triangulation.circumcenters);
		}

		private void ComputeHalfEdges()
		{
			for (int i = 0; i < Triangles.Length; i++)
			{
				if (fromBarycenter)
					ComputeBarycenterHalfEdgesForTriangle(i);
				else
					ComputeCircumcenterHalfEdgesForTriangle(i);
			}
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

			diagram = new VoronoiDiagram(triangulation.points, pointList.ToArray(), cells, halfEdgeList.ToArray());
		}

		private void Clear()
		{
			createHull = false;
			fromBarycenter = false;

			cellNodes = null;
			pointList.Clear();
			halfEdgeList.Clear();

			triangulation = null;
		}

		public void CalculateDiagram(DelaunayTriangulation triangulation, out VoronoiDiagram diagram, bool fromBarycenter = false, bool createHull = false)
		{
			this.createHull = createHull;
			this.triangulation = triangulation;
			this.fromBarycenter = fromBarycenter;

			InitializeCellNodes();

			InitializePointList();

			ComputeHalfEdges();

			GenerateResult(out diagram);

			Clear();
		}
		public void CalculateDiagram(DelaunayTriangulation triangulation, out VoronoiDiagram diagram, DiagramSetting setting)
		{
			CalculateDiagram(triangulation, out diagram, setting.fromBarycenter, setting.createHull);
		}
	}
}
