using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry.DataStructure;

public enum BoundaryType
{
	None,
	Transform,
	Convergent,
	Divergent
}

public class Boundary
{
	public Planet parentPlanet = null;

	public int edgeIndex = -1;

	public float stress = 0.0f;

	public BoundaryType type = BoundaryType.None;

	public Vector3 parallelVector = Vector3.zero;
	public Vector3 perpendicularVector = Vector3.zero;

	public float StressInPercent => stress / parentPlanet.boundaryStressMax;

	public Edge Edge => parentPlanet.polygonHalfEdgeData.edges[edgeIndex];

	public Vector3 LeftVertex => Edge.FirstHalfEdge.Vertex;
	public Vector3 RightVertex => Edge.FirstHalfEdge.Next.Vertex;

	public Vector3 MidPoint => (LeftVertex + RightVertex) * 0.5f;

	public Cell RightCell => parentPlanet.cells[Edge.FirstHalfEdge.faceIndex];
	public Cell LeftCell => parentPlanet.cells[Edge.SecondHalfEdge.faceIndex];
}

public class Cell
{
	public Planet parentPlanet = null;

	public int faceIndex = -1;
	public int plateIndex = -1;

	public float elevation = 0.0f;

	public Vector3 normal = Vector3.zero;
	public Vector3 position = Vector3.zero;

	public float linearMagnitude = 0.0f;
	public Vector3 linearDirection = Vector3.zero;
	public Vector3 LinearVelocity => linearDirection * linearMagnitude;

	public bool IsAssign => plateIndex != -1;
	public TectonicPlate Plate => parentPlanet.tectonicPlates[plateIndex];

	public Face Face => parentPlanet.polygonHalfEdgeData.faces[faceIndex];
}

public class TectonicPlate
{
	private List<int> cellIndexes = new List<int>();
	private List<int> boundaryIndexes = new List<int>();
	private List<int> borderVertexIndexes = new List<int>();

	public Planet parentPlanet = null;

	public bool isOceanic = false;

	public float surface = 0.0f;

	public Vector3 center = Vector3.zero;

	public float angularMagnitude = 0.0f;
	public Vector3 angularAxis = Vector3.zero;
	public Vector3 AngularVelocity => angularAxis * angularMagnitude;

	public Cell CellOrigin => parentPlanet.cells[cellIndexes[0]];

	public int CellCount => cellIndexes.Count;
	public int BoundaryCount => boundaryIndexes.Count;
	public int BorderVerticesCount => borderVertexIndexes.Count;

	public void AddCell(int cellIndex)
	{
		var cell = parentPlanet.cells[cellIndex];

		var linearVelocity = Vector3.Cross(AngularVelocity, cell.position);
		cell.linearMagnitude = linearVelocity.magnitude;
		cell.linearDirection = linearVelocity.normalized;

		surface += parentPlanet.cellSurface;

		cellIndexes.Add(cellIndex);
	}
	public void AddBoundary(int boundaryIndex)
	{
		boundaryIndexes.Add(boundaryIndex);
	}
	public void AddBorderVertex(int borderIndex)
	{
		borderVertexIndexes.Add(borderIndex);
	}

	public Cell GetCellAt(int index)
	{
		return parentPlanet.cells[cellIndexes[index]];
	}
	public Boundary GetBoundayAt(int index)
	{
		return parentPlanet.boundaries[boundaryIndexes[index]];
	}
	public Vector3 GetBorderVertexAt(int index)
	{
		return parentPlanet.polygonHalfEdgeData.vertices[borderVertexIndexes[index]];
	}

	public void Clear()
	{
		ClearCells();
		ClearBoundaries();
		ClearBorderVertices();
	}
	public void ClearCells()
	{
		cellIndexes.Clear();
	}
	public void ClearBoundaries()
	{
		boundaryIndexes.Clear();
	}
	public void ClearBorderVertices()
	{
		borderVertexIndexes.Clear();
	}
}

public class Planet
{
	public float radius = 1.0f;
	public float surface = 0.0f;
	public float cellSurface = 0.0f;
	public float boundaryStressMax = 0.0f;
	public float angularVelocityMax = 0.0f;

	public Cell[] cells = null;
	public TectonicPlate[] tectonicPlates = null;
	public HalfEdgeData polygonHalfEdgeData = null;
	public List<Boundary> boundaries = new List<Boundary>();

	public Planet(HalfEdgeData polygonHalfEdgeData)
	{
		this.polygonHalfEdgeData = polygonHalfEdgeData;

		InitializeCells();
		CalculateCellSurface();
		CalculatePlanetSurface();
	}

	private void InitializeCells()
	{
		cells = new Cell[this.polygonHalfEdgeData.faces.Length];
		for (int i = 0; i < this.polygonHalfEdgeData.faces.Length; i++)
		{
			var face = this.polygonHalfEdgeData.faces[i];
			var polygon = new Vector3[face.halfEdgeIndexes.Length];
			face.ForEachHalfEdge((halfEdge, index) => polygon[index] = halfEdge.Vertex);
			var cellCenter = Geometry.GeometryUtility.CalculateBarycenter(polygon);

			var forward = face.First.Vertex - cellCenter;
			var normal = Vector3.Cross(forward, (face.Last.Vertex - cellCenter)).normalized * -1.0f;

			cells[i] = new Cell
			{
				faceIndex = i,
				position = cellCenter,
				normal = normal,

				parentPlanet = this
			};
		}
	}
	private void CalculateCellSurface()
	{
		var cell = cells[0];
		var edgeCount = cells[0].Face.HalfEdgeCount;

		var p0 = cell.position;
		var p1 = cell.Face.GetHalfEdgeAt(0).Vertex;
		var p2 = cell.Face.GetHalfEdgeAt(1).Vertex;
		var triangleArea = Geometry.GeometryUtility.TriangleArea(p0, p1, p2);

		cellSurface = triangleArea * edgeCount;
	}
	private void CalculatePlanetSurface()
	{
		surface = cells.Length * cellSurface;
	}
}
