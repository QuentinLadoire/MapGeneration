using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry.DataStructure;

public struct Cell
{
	public int faceIndex;
	public int plateIndex;

	public Planet parentPlanet;

	public Face Face => parentPlanet.polygonHalfEdgeData.faces[faceIndex];
	public TectonicPlate Plate => parentPlanet.tectonicPlates[plateIndex];

	public Cell(int faceIndex, int plateIndex, Planet parentPlanet)
	{
		this.faceIndex = faceIndex;
		this.plateIndex = plateIndex;

		this.parentPlanet = parentPlanet;
	}
}

public struct TectonicPlate
{
	public Color color;
	public List<int> cellIndexes;

	public Planet parentPlanet;

	public void AddCell(int cellIndex)
	{
		cellIndexes.Add(cellIndex);
	}

	public void ClearCells()
	{
		cellIndexes.Clear();
	}

	public TectonicPlate(Color color, Planet parentPlanet)
	{
		this.color = color;
		this.cellIndexes = new List<int>();

		this.parentPlanet = parentPlanet;
	}
}

public class Planet
{
	public Cell[] cells = null;
	public TectonicPlate[] tectonicPlates = null;
	public HalfEdgeData polygonHalfEdgeData = null;

	private bool ComputeTectonicPlate(int plateIndex, Queue<int> processingCellIndexes, bool[] freeCells)
	{
		if (processingCellIndexes.Count == 0)
			return false;

		var cellIndex = processingCellIndexes.Dequeue();
		if (freeCells[cellIndex])
		{
			freeCells[cellIndex] = false;

			cells[cellIndex].plateIndex = plateIndex;
			tectonicPlates[plateIndex].AddCell(cellIndex);

			var face = polygonHalfEdgeData.faces[cellIndex];
			var halfEdge = face.First;
			for (int i = 0; i < face.edgeCount; i++)
			{
				var adjacentFaceIndex = halfEdge.Opposite.faceIndex;
				processingCellIndexes.Enqueue(adjacentFaceIndex);

				halfEdge = halfEdge.Next;
			}

			return true;
		}

		return false;
	}
	public void ComputeTectonicPlates(int plateCount = 5)
	{
		tectonicPlates = new TectonicPlate[plateCount];
		for (int i = 0; i < plateCount; i++)
			tectonicPlates[i] = new TectonicPlate(Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f), this);

		var freeCells = new bool[cells.Length];
		freeCells.Fill(true);

		var processingCellIndexQueue = new Queue<int>[plateCount];
		for (int i = 0; i < plateCount; i++)
		{
			processingCellIndexQueue[i] = new Queue<int>();
			processingCellIndexQueue[i].Enqueue(Random.Range(0, cells.Length));
		}

		var freeCellRemainingCount = freeCells.Length;
		while (freeCellRemainingCount > 0)
		{
			for (int i = 0; i < plateCount; i++)
			{
				var queue = processingCellIndexQueue[i];
				if (ComputeTectonicPlate(i, queue, freeCells))
					freeCellRemainingCount--;
			}
		}
	}

	public Planet(HalfEdgeData polygonHalfEdgeData)
	{
		this.polygonHalfEdgeData = polygonHalfEdgeData;

		cells = new Cell[this.polygonHalfEdgeData.faces.Length];
		for (int i = 0; i < this.polygonHalfEdgeData.faces.Length; i++)
			cells[i] = new Cell(i, -1, this);
	}
}
