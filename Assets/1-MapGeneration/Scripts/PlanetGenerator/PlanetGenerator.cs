using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;
using Geometry.DataStructure;

using GeometryUtility = Geometry.GeometryUtility;

public class TectonicPlate
{
	public int[] cells = null;
}

public class Planet
{
	private Mesh mesh = null;
	private bool[] freeCells = null;
	private MeshData meshData = null;
	private TectonicPlate[] tectonicPlates = null;
	private HalfEdgeData polygonHalfEdgeData = null;

	public Mesh Mesh => mesh;

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
	public void ComputeGeometry()
	{
		if (meshData == null)
			meshData = new MeshData();

		var faces = polygonHalfEdgeData.faces;
		for (int i = 0; i < faces.Length; i++)
			ComputeFace(faces[i]);
	}

	private bool ComputeTectonicPlate(int plateIndex, int[] processingCellIndexes)
	{
		return false;
	}
	public void ComputeTectonicPlates(int count = 5)
	{
		freeCells.Fill(true);

		var faceCount = polygonHalfEdgeData.faces.Length;

		var processingCellIndexes = new int[count];
		for (int i = 0; i < count; i++)
			processingCellIndexes[i] = Random.Range(0, faceCount);

		tectonicPlates = new TectonicPlate[count];
		while (faceCount > 0)
		{
			for (int i = 0; i < count; i++)
			{
				if (ComputeTectonicPlate(i, processingCellIndexes))
					faceCount--;
			}
		}
	}

	public void ComputeMesh()
	{
		if (mesh == null)
			mesh = new Mesh();

		mesh.SetVertices(meshData.Vertices);
		mesh.SetTriangles(meshData.Triangles, 0);

		mesh.RecalculateNormals();
	}

	public Planet(HalfEdgeData polygonHalfEdgeData)
	{
		this.polygonHalfEdgeData = polygonHalfEdgeData;
		this.freeCells = new bool[this.polygonHalfEdgeData.faces.Length];
	}
}

[ExecuteAlways]
public class PlanetGenerator : MonoBehaviour
{
	[Header("Planet Settings")]
	[SerializeField] private Material planetMaterial = null;
	[SerializeField] private float planetRadius = 1.0f;
	[SerializeField] private int planetRefiningStep = 3;
	[SerializeField] private int planetTectonicPlateCount = 5;

	[Header("Debug Settings")]
	[SerializeField] private Mesh cubeMesh = null;
	[SerializeField] private Material triangleDebugMat = null;
	[SerializeField] private Material polygonDebugMat = null;
	[SerializeField] private float tickness = 0.005f;

	[Header("Render Options")]
	[SerializeField] private bool renderPlanet = false;
	[SerializeField] private bool renderTriangleData = false;
	[SerializeField] private bool renderPolygonData = false;

	[SerializeField] private Mesh mesh = null;
	private Planet planet = null;
	private DualHalfEdgeData dualMeshData = null;

	private void DrawPlanet()
	{
		if (mesh != null && planetMaterial != null && renderPlanet)
			Graphics.DrawMesh(mesh, transform.localToWorldMatrix, planetMaterial, 0);
	}

	private void DrawEdges(Edge[] edges, int offset, int count, Material material, float tickness = 0.005f)
	{
		var edgeMatrices = new List<Matrix4x4>(count);
		for (int j = 0; j < count; j++)
		{
			var p0 = edges[offset + j].FirstHalfEdge.Vertex;
			var p1 = edges[offset + j].FirstHalfEdge.Next.Vertex;

			var forward = p1 - p0;

			var translation = (p0 + p1) * 0.5f;
			var rotation = Quaternion.LookRotation(forward);

			edgeMatrices.Add(transform.localToWorldMatrix * Matrix4x4.TRS(translation, rotation, new Vector3(tickness, tickness, forward.magnitude)));
		}

		Graphics.DrawMeshInstanced(cubeMesh, 0, material, edgeMatrices);
	}
	private void DrawPolygonData()
	{
		var edges = dualMeshData.polygonData.edges;
		
		int drawCount = edges.Length / 1023;
		for (int i = 0; i < drawCount; i++)
			DrawEdges(edges, 1023 * i, 1023, polygonDebugMat, tickness);

		var edgeCountLeft = edges.Length - 1023 * drawCount;
		DrawEdges(edges, 1023 * drawCount, edgeCountLeft, polygonDebugMat, tickness);
	}
	private void DrawTriangleData()
	{
		var edges = dualMeshData.triangleData.edges;

		int drawCount = edges.Length / 1023;
		for (int i = 0; i < drawCount; i++)
			DrawEdges(edges, 1023 * i, 1023, triangleDebugMat, tickness);

		var edgeCountLeft = edges.Length - 1023 * drawCount;
		DrawEdges(edges, 1023 * drawCount, edgeCountLeft, triangleDebugMat, tickness);
	}
	private void DrawDebug()
	{
		if (dualMeshData == null)
			return;

		if (renderTriangleData && triangleDebugMat != null)
			DrawTriangleData();

		if (renderPolygonData && polygonDebugMat != null)
			DrawPolygonData();
	}

	private void GenerateShape()
	{
		DataStructureBuilder.CreateDualMeshData(MeshGenerator.CreateIcoSphere(planetRadius, planetRefiningStep), out dualMeshData);

		planet = new Planet(dualMeshData.polygonData);
		planet.ComputeGeometry();
		planet.ComputeMesh();

		mesh = planet.Mesh;
	}
	public void Generate()
	{
		GenerateShape();
	}

	private void Update()
	{
		DrawPlanet();

		DrawDebug();
	}
}
