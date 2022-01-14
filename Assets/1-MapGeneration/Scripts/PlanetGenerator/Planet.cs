using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry.DataStructure;

public class Cell
{
	public int faceIndex = -1;
	public int plateIndex = -1;

	public bool isBorder = false;
	public Vector3 center = Vector3.zero;
	public Vector3 normal = Vector3.zero;

	public float linearMagnitude = 0.0f;
	public Vector3 linearDirection = Vector3.zero;

	public Planet parentPlanet = null;

	public Vector3 LinearVelocity => linearDirection * linearMagnitude;

	public bool IsAssign => plateIndex != -1;
	public TectonicPlate Plate => parentPlanet.tectonicPlates[plateIndex];
	public Face Face => parentPlanet.polygonHalfEdgeData.faces[faceIndex];
}

public class TectonicPlate
{
	private List<int> cellIndexes = new List<int>();
	private List<int> borderCellIndexes = new List<int>();
	private List<int> borderVertexIndexes = new List<int>();

	public Planet parentPlanet = null;

	public bool isOceanic = false;

	public float angularMagnitude = 0.0f;
	public Vector3 angularAxis = Vector3.zero;

	public Vector3 AngularVelocity => angularAxis* angularMagnitude;

	public int CellCount => cellIndexes.Count;
	public int BorderCellCount => borderCellIndexes.Count;
	public int BorderVerticesCount => borderVertexIndexes.Count;

	public void AddCell(int cellIndex)
	{
		var cell = parentPlanet.cells[cellIndex];

		var linearVelocity = Vector3.Cross(AngularVelocity, cell.center);
		cell.linearMagnitude = linearVelocity.magnitude;
		cell.linearDirection = linearVelocity.normalized;

		cellIndexes.Add(cellIndex);
	}
	public void AddBorderCell(int cellIndex)
	{
		borderCellIndexes.Add(cellIndex);
	}
	public void AddBorderVertex(int borderIndex)
	{
		borderVertexIndexes.Add(borderIndex);
	}

	public int GetCenterCellIndex()
	{
		return cellIndexes[0];
	}
	public int GetCellIndex(int index)
	{
		return cellIndexes[index];
	}
	public int GetBorderCellIndex(int index)
	{
		return cellIndexes[borderCellIndexes[index]];
	}
	public int GetBorderVertexIndex(int index)
	{
		return borderVertexIndexes[index];
	}

	public Cell GetCenterCell()
	{
		return parentPlanet.cells[cellIndexes[0]];
	}
	public Cell GetCellAt(int index)
	{
		return parentPlanet.cells[cellIndexes[index]];
	}
	public Cell GetBorderCellAt(int index)
	{
		return parentPlanet.cells[cellIndexes[borderCellIndexes[index]]];
	}
	public Vector3 GetBorderVertex(int index)
	{
		return parentPlanet.polygonHalfEdgeData.vertices[borderVertexIndexes[index]];
	}

	public void ClearCells()
	{
		cellIndexes.Clear();
	}
	public void ClearBorderCells()
	{
		borderCellIndexes.Clear();
	}
	public void ClearBorderVertices()
	{
		borderVertexIndexes.Clear();
	}
	public void Clear()
	{
		ClearCells();
		ClearBorderCells();
		ClearBorderVertices();
	}
}

public class Planet
{
	public float radius = 1.0f;

	public Cell[] cells = null;
	public TectonicPlate[] tectonicPlates = null;
	public HalfEdgeData polygonHalfEdgeData = null;

	public Planet(HalfEdgeData polygonHalfEdgeData)
	{
		this.polygonHalfEdgeData = polygonHalfEdgeData;

		cells = new Cell[this.polygonHalfEdgeData.faces.Length];
		for (int i = 0; i < this.polygonHalfEdgeData.faces.Length; i++)
		{
			var face = this.polygonHalfEdgeData.faces[i];
			var polygon = new Vector3[face.edgeCount];
			face.ForEachHalfEdge((halfEdge, index) => polygon[index] = halfEdge.Vertex);
			var cellCenter = Geometry.GeometryUtility.CalculateBarycenter(polygon);

			var forward = face.First.Vertex - cellCenter;
			var normal = Vector3.Cross(forward, (face.Last.Vertex - cellCenter)).normalized * -1.0f;

			cells[i] = new Cell
			{
				faceIndex = i,
				center = cellCenter,
				normal = normal,

				parentPlanet = this
			};
		}
	}
}
