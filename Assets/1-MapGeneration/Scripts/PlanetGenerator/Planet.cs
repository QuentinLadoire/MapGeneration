using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry;
using Geometry.DataStructure;

using GeometryUtility = Geometry.GeometryUtility;

public class TectonicPlate
{
	public List<int> cellList = new List<int>();
}

public class Planet
{
	private Mesh mesh = null;
	private bool[] freeCells = null;
	private MeshData meshData = null;
	private TectonicPlate[] tectonicPlates = null;
	private HalfEdgeData polygonHalfEdgeData = null;

	public Mesh Mesh => mesh;
	public HalfEdgeData PolygonHalfEdgeData => polygonHalfEdgeData;

	private void ComputeFace(Face face)
	{
		var polygon = new Vector3[face.edgeCount];

		var faceEdgeCount = face.edgeCount;
		var verticesCount = meshData.VerticesCount;

		var halfEdge = face.First;
		for (int i = 0; i < faceEdgeCount; i++)
		{
			polygon[i] = halfEdge.Vertex;

			var firstIndex = verticesCount + i;
			var secondIndex = (i != faceEdgeCount - 1) ? (verticesCount + i + 1) : verticesCount;
			var thirdIndex = verticesCount + faceEdgeCount;

			meshData.AddTriangle(firstIndex, secondIndex, thirdIndex);
			meshData.AddVertex(halfEdge.Vertex);

			halfEdge = halfEdge.Next;
		}

		meshData.AddVertex(GeometryUtility.CalculateBarycenter(polygon));
	}
	private bool ComputeTectonicPlate(int plateIndex, Queue<int> processingCellIndexes)
	{
		if (processingCellIndexes.Count == 0)
			return false;

		var cellIndex = processingCellIndexes.Dequeue();
		if (freeCells[cellIndex])
		{
			freeCells[cellIndex] = false;

			tectonicPlates[plateIndex].cellList.Add(cellIndex);

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

	public void ComputeTectonicPlates(int count = 5)
	{
		freeCells.Fill(true);

		var faceCount = polygonHalfEdgeData.faces.Length;

		var processingCellIndexQueue = new Queue<int>[count];
		for (int i = 0; i < count; i++)
		{
			processingCellIndexQueue[i] = new Queue<int>();
			processingCellIndexQueue[i].Enqueue(Random.Range(0, faceCount));
		}

		tectonicPlates = new TectonicPlate[count];
		for (int i = 0; i < count; i++)
			tectonicPlates[i] = new TectonicPlate();

		while (faceCount > 0)
		{
			for (int i = 0; i < count; i++)
			{
				if (ComputeTectonicPlate(i, processingCellIndexQueue[i]))
					faceCount--;
			}
		}
	}
	public void ComputeGeometry()
	{
		if (meshData == null)
			meshData = new MeshData();

		if (tectonicPlates != null)
		{
			for (int i = 0; i < tectonicPlates.Length; i++)
			{
				var cellList = tectonicPlates[i].cellList;
				for (int j = 0; j < cellList.Count; j++)
				{
					var faceIndex = cellList[j];
					var face = polygonHalfEdgeData.faces[faceIndex];
					ComputeFace(face);
				}
			}
		}
		else
		{
			var faces = polygonHalfEdgeData.faces;
			for (int i = 0; i < faces.Length; i++)
				ComputeFace(faces[i]);
		}
	}
	public void ComputeColor()
	{
		for (int i = 0; i < tectonicPlates.Length; i++)
		{
			var plateColor = Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f);
			var cellList = tectonicPlates[i].cellList;

			for (int j = 0; j < cellList.Count; j++)
			{
				var edgeCount = polygonHalfEdgeData.faces[cellList[j]].edgeCount;
				for (int k = 0; k < edgeCount + 1; k++)
					meshData.AddColor(plateColor);
			}
		}
	}

	public void ComputeMesh()
	{
		if (mesh == null)
			mesh = new Mesh();

		mesh.SetVertices(meshData.Vertices);
		mesh.SetTriangles(meshData.Triangles, 0);
		mesh.SetColors(meshData.Colors);

		mesh.RecalculateNormals();
	}

	public Planet(HalfEdgeData polygonHalfEdgeData)
	{
		this.polygonHalfEdgeData = polygonHalfEdgeData;
		this.freeCells = new bool[this.polygonHalfEdgeData.faces.Length];
	}
}
