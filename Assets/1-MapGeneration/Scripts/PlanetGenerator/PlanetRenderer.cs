using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry;
using Geometry.DataStructure;

using GeometryUtility = Geometry.GeometryUtility;

[ExecuteAlways]
public class PlanetRenderer : MonoBehaviour
{
	[Header("Planet Setting")]
	[SerializeField] private Material material = null;

	[Header("Debug Settings")]
	[SerializeField] private RenderHalfEdgeSetting renderPolygonDataSetting = RenderHalfEdgeSetting.Default;

	[Header("Render Options")]
	[SerializeField] private bool renderPlanet = false;
	[SerializeField] private bool renderPolygonData = false;
	[SerializeField] private bool renderTectonicPlate = false;
	[SerializeField] private bool renderTectonicPlateBorder = false;

	private Mesh mesh = null;
	private Planet planet = null;
	private MeshData meshData = new MeshData();

	private void ComputeFaceGeometry(Face face)
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
		else
		{
			meshData.ClearVertices();
			meshData.ClearTriangles();
		}

		var cells = planet.cells;
		for (int i = 0; i < cells.Length; i++)
		{
			var face = cells[i].Face;
			ComputeFaceGeometry(face);
		}
	}

	private void ComputeFaceColor(Face face, MeshData meshData, Color color)
	{
		var edgeCount = face.edgeCount;
		for (int i = 0; i < edgeCount + 1; i++) //+1 for the barycenter vertex
			meshData.AddColor(color);
	}
	public void ComputeTectonicPlatesColor()
	{
		meshData.ClearColors();

		var cells = planet.cells;
		for (int i = 0; i < cells.Length; i++)
		{
			var plateColor = cells[i].Plate.color;
			var face = cells[i].Face;
			ComputeFaceColor(face, meshData, plateColor);
		}
	}

	private void RecalculateMeshData()
	{
		if (meshData != null)
			meshData.Clear();

		ComputeGeometry();

		if (renderTectonicPlate)
			ComputeTectonicPlatesColor();
	}
	private void RecalculateMesh()
	{
		if (mesh == null)
			mesh = new Mesh();

		mesh.SetVertices(meshData.Vertices);
		mesh.SetTriangles(meshData.Triangles, 0);
		mesh.SetColors(meshData.Colors);

		mesh.RecalculateNormals();
	}

	public void SetPlanet(Planet planet)
	{
		this.planet = planet;

		RecalculateMeshData();
		RecalculateMesh();
	}

	private void DrawPlanet()
	{
		if (renderPlanet && planet != null && material != null && mesh != null)
			Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, 0);
	}
	private void DrawPolygonData()
	{
		if (planet.polygonHalfEdgeData == null) return;

		if (renderPolygonData && renderPolygonDataSetting.mesh != null && renderPolygonDataSetting.material != null)
			DataStructureUtility.DrawHalfEdgeData(planet.polygonHalfEdgeData, renderPolygonDataSetting, transform.localToWorldMatrix);
	}
	private void DrawDebug()
	{
		if (planet == null) return;

		DrawPolygonData();
	}

	private void Update()
	{
		DrawPlanet();

		DrawDebug();
	}
}
