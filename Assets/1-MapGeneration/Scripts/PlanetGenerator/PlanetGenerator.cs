using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;
using Geometry.DataStructure;

[ExecuteAlways]
public class PlanetGenerator : MonoBehaviour
{
	[Header("Planet Settings")]
	[SerializeField] private Material planetMaterial = null;
	[SerializeField] private float planetRadius = 1.0f;
	[SerializeField] private int planetRefiningStep = 3;

	[Header("Debug Settings")]
	[SerializeField] private Mesh cubeMesh = null;
	[SerializeField] private Material triangleDebugMat = null;
	[SerializeField] private Material polygonDebugMat = null;
	[SerializeField] private float tickness = 0.005f;

	[Header("Render Options")]
	[SerializeField] private bool renderPlanet = false;
	[SerializeField] private bool renderTriangleData = false;
	[SerializeField] private bool renderPolygonData = false;

	private Mesh mesh = null;
	private MeshData meshData = null;
	private DualMeshData dualMeshData = null;

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
		meshData = MeshGenerator.CreateIcoSphere(planetRadius, planetRefiningStep);
		DataStructureBuilder.CreateDualMeshData(meshData, out dualMeshData);

		mesh = new Mesh
		{
			vertices = meshData.vertices,
			triangles = meshData.triangles
		};
		mesh.RecalculateNormals();
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
