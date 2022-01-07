using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry;
using Geometry.DataStructure;

[ExecuteAlways]
public class PlanetRenderer : MonoBehaviour
{
	public enum RenderMode
	{
		Plates,
		PlatesType
	}

	[Header("Planet Setting")]
	[SerializeField] private Material material = null;
	[SerializeField] private Color oceanColor = Color.white;
	[SerializeField] private Color groundColor = Color.white;

	[Header("Debug Settings")]
	[SerializeField] private RenderHalfEdgeSetting renderPolygonDataSetting = RenderHalfEdgeSetting.Default;

	[Header("Render Options")]
	[SerializeField] private bool renderPlanet = false;
	[SerializeField] private bool renderPolygonData = false;
	[SerializeField] private RenderMode renderMode = RenderMode.Plates;

	[SerializeField] private Mesh mesh = null;
	private Planet planet = null;
	private MeshData meshData = null;

	private void ComputeCellsGeometry()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];
			var cellCenter = planet.cellCenters[i];

			var face = cell.Face;
			var faceEdgeCount = face.edgeCount;
			var verticesCount = meshData.VerticesCount;

			meshData.AddVertex(cellCenter); //Add Center Cell Vertex

			face.ForEachHalfEdge((halfEdge, index) =>
			{
				var firstIndex = verticesCount;										//Center Cell Index
				var secondIndex = verticesCount + index + 1;						//Current HalfEdge Vertex Index
				var thirdIndex = verticesCount + ((index + 1) % faceEdgeCount) + 1; //Next HalfEdge Vertex Index, clamp to [0, FaceEdgeCount[

				meshData.AddTriangle(firstIndex, secondIndex, thirdIndex);
				meshData.AddVertex(halfEdge.Vertex);
			});
		}
	}
	public void RecalculateGeometry()
	{
		if (meshData == null)
			meshData = new MeshData();

		meshData.Clear();

		ComputeCellsGeometry();
	}

	private void ComputePlateCellsColor()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];
			var plate = cell.Plate;

			meshData.AddColor(plate.color);									 //Cell Center Vertex Color
			cell.Face.ForEachHalfEdge(() => meshData.AddColor(plate.color)); //Cell Corner Vertex Color
		}
	}
	private void ComputePlateTypeCellsColor()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];
			var plate = cell.Plate;

			var color = plate.isOceanic ? oceanColor : groundColor;

			meshData.AddColor(color);                                  //Cell Center Vertex Color
			cell.Face.ForEachHalfEdge(() => meshData.AddColor(color)); //Cell Corner Vertex Color
		}
	}
	private void ComputeCellsColor()
	{
		switch (renderMode)
		{
			case RenderMode.Plates:
				ComputePlateCellsColor();
				break;

			case RenderMode.PlatesType:
				ComputePlateTypeCellsColor();
				break;
		}
	}
	public void RecalculateColors()
	{
		meshData.ClearColors();

		ComputeCellsColor();
	}

	public void RecalculateMeshData()
	{
		if (meshData == null)
			meshData = new MeshData();

		meshData.Clear();

		RecalculateGeometry();
		RecalculateColors();
	}

	public void ApplyMeshGeometry()
	{
		if (mesh == null)
			mesh = new Mesh();

		mesh.SetVertices(meshData.Vertices);
		mesh.SetTriangles(meshData.Triangles, 0);

		mesh.RecalculateNormals();
	}
	public void ApplyMeshColor()
	{
		mesh.SetColors(meshData.Colors);
	}
	public void RecalculateMesh()
	{
		if (mesh == null)
			mesh = new Mesh();

		ApplyMeshGeometry();
		ApplyMeshColor();
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
		if (planet == null || planet.polygonHalfEdgeData == null) return;

		if (renderPolygonData && renderPolygonDataSetting.mesh != null && renderPolygonDataSetting.material != null)
			DataStructureUtility.DrawHalfEdgeData(planet.polygonHalfEdgeData, renderPolygonDataSetting, transform.localToWorldMatrix);
	}

	private void Update()
	{
		DrawPlanet();

		DrawPolygonData();
	}

	private void OnValidate()
	{
		if (planet == null) return;

		RecalculateColors();
		ApplyMeshColor();
	}
}
