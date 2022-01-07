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

	public Planet parentPlanet = null;

	public bool IsAssign => plateIndex != -1;
	public TectonicPlate Plate => parentPlanet.tectonicPlates[plateIndex];
	public Face Face => parentPlanet.polygonHalfEdgeData.faces[faceIndex];
}

public class TectonicPlate
{
	private List<int> cellIndexes = new List<int>();
	private List<int> borderCellIndexes = new List<int>();

	public bool isOceanic = false;

	public Planet parentPlanet = null;

	public int CellCount => cellIndexes.Count;
	public int BorderCellCount => borderCellIndexes.Count;

	public void AddCell(int cellIndex)
	{
		cellIndexes.Add(cellIndex);
	}
	public void AddBorderCell(int cellIndex)
	{
		borderCellIndexes.Add(cellIndex);
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

	public void ClearCells()
	{
		cellIndexes.Clear();
	}
	public void ClearBorderCells()
	{
		borderCellIndexes.Clear();
	}
	public void Clear()
	{
		ClearCells();
		ClearBorderCells();
	}

	public TectonicPlate(bool isOceanic, Planet parentPlanet)
	{
		this.isOceanic = isOceanic;
		this.cellIndexes = new List<int>();
		this.borderCellIndexes = new List<int>();

		this.parentPlanet = parentPlanet;
	}
}

public class Planet
{
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

			cells[i] = new Cell
			{
				faceIndex = i,
				center = cellCenter,
				parentPlanet = this
			};
		}
	}
}
