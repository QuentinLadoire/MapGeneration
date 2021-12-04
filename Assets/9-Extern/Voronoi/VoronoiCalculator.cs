using System.Collections.Generic;
using Miscellaneous;

namespace DelaunayVoronoi
{
	public class VoronoiCalculator
	{
		private class CellNode
		{
			public int si = -1;
			public List<int> heis = new List<int>();
		}

		private CellNode[] cellNodes = null;
		private List<VoronoiDiagram.HalfEdge> halfEdgeList = new List<VoronoiDiagram.HalfEdge>();

		private DelaunayTriangulation triangulation = null;
		private DelaunayTriangulation.Triangle[] Triangles => triangulation.triangles;
		private DelaunayTriangulation.HalfEdge[] HalfEdges => triangulation.halfEdges;

		private void ComputeBarycenterHalfEdgesForTriangle(int ti)
		{
			var triangle = Triangles[ti];
			var halfEdge0 = HalfEdges[triangle.hei0];
			var halfEdge1 = HalfEdges[triangle.hei1];
			var halfEdge2 = HalfEdges[triangle.hei2];

			//First triangle HalfEdge
			if (halfEdge0.ohei != -1)
			{
				var otherTriangle0 = Triangles[HalfEdges[halfEdge0.ohei].ti];
				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.bci,
					pi1 = otherTriangle0.bci,

					ci = halfEdge0.pi1
				});

				cellNodes[halfEdge0.pi1].heis.Add(halfEdgeList.Count - 1);
			}

			//Second triangle HalfEdge
			if (halfEdge1.ohei != -1)
			{
				var otherTriangle1 = Triangles[HalfEdges[halfEdge1.ohei].ti];
				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.bci,
					pi1 = otherTriangle1.bci,

					ci = halfEdge1.pi1
				});

				cellNodes[halfEdge1.pi1].heis.Add(halfEdgeList.Count - 1);
			}

			//Third triangle HalfEdge
			if (halfEdge2.ohei != -1)
			{
				var otherTriangle2 = Triangles[HalfEdges[halfEdge2.ohei].ti];
				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.bci,
					pi1 = otherTriangle2.bci,

					ci = halfEdge2.pi1
				});

				cellNodes[halfEdge2.pi1].heis.Add(halfEdgeList.Count - 1);
			}
		}
		private void ComputeCircumcenterHalfEdgesForTriangle(int ti)
		{
			var triangle = Triangles[ti];
			var halfEdge0 = HalfEdges[triangle.hei0];
			var halfEdge1 = HalfEdges[triangle.hei1];
			var halfEdge2 = HalfEdges[triangle.hei2];

			//First triangle HalfEdge
			if (halfEdge0.ohei != -1)
			{
				var otherTriangle0 = Triangles[HalfEdges[halfEdge0.ohei].ti];
				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.cci,
					pi1 = otherTriangle0.cci,

					ci = halfEdge0.pi1
				});

				cellNodes[halfEdge0.pi1].heis.Add(halfEdgeList.Count - 1);
			}

			//Second triangle HalfEdge
			if (halfEdge1.ohei != -1)
			{
				var otherTriangle1 = Triangles[HalfEdges[halfEdge1.ohei].ti];
				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.cci,
					pi1 = otherTriangle1.cci,

					ci = halfEdge1.pi1
				});

				cellNodes[halfEdge1.pi1].heis.Add(halfEdgeList.Count - 1);
			}

			//Third triangle HalfEdge
			if (halfEdge2.ohei != -1)
			{
				var otherTriangle2 = Triangles[HalfEdges[halfEdge2.ohei].ti];
				halfEdgeList.Add(new VoronoiDiagram.HalfEdge
				{
					pi0 = triangle.cci,
					pi1 = otherTriangle2.cci,

					ci = halfEdge2.pi1
				});

				cellNodes[halfEdge2.pi1].heis.Add(halfEdgeList.Count - 1);
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
		private void ComputeHalfEdges(bool fromBarycenter)
		{
			for (int i = 0; i < Triangles.Length; i++)
			{
				if (fromBarycenter)
					ComputeBarycenterHalfEdgesForTriangle(i);
				else
					ComputeCircumcenterHalfEdgesForTriangle(i);
			}
		}
		private void GenerateResult(out VoronoiDiagram diagram, bool fromBarycenter)
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

			if (fromBarycenter)
				diagram = new VoronoiDiagram(triangulation.points, triangulation.barycenters, cells, halfEdgeList.ToArray());
			else
				diagram = new VoronoiDiagram(triangulation.points, triangulation.circumcenters, cells, halfEdgeList.ToArray());
		}
		private void Clear()
		{
			cellNodes = null;
			halfEdgeList.Clear();

			triangulation = null;
		}

		public void CalculateDiagram(DelaunayTriangulation triangulation, out VoronoiDiagram diagram, bool fromBarycenter = false)
		{
			this.triangulation = triangulation;

			InitializeCellNodes();

			ComputeHalfEdges(fromBarycenter);

			GenerateResult(out diagram, fromBarycenter);

			Clear();
		}
	}
}
