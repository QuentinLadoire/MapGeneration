using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry;
using Geometry.DataStructure;

[System.Serializable]
public struct RenderPlateMovementSetting
{
	public Mesh mesh;
	public Material material;

	public static RenderPlateMovementSetting Default = new RenderPlateMovementSetting
	{
		mesh = null,
		material = null
	};
}

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
	[SerializeField] private RenderPlateMovementSetting renderPlateMovementSetting = RenderPlateMovementSetting.Default;

	[Header("Render Options")]
	[SerializeField] private bool renderPlanet = false;
	[SerializeField] private bool renderPolygonData = false;
	[SerializeField] private bool renderPlateBorder = false;
	[SerializeField] private bool renderPlateMovement = false;
	[SerializeField] private RenderMode renderMode = RenderMode.Plates;

	[SerializeField] private Mesh mesh = null;
	private Planet planet = null;
	private MeshData meshData = null;

	private Color[] colors = null;

	private void ComputeCellsGeometry()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];

			var face = cell.Face;
			var faceEdgeCount = face.edgeCount;
			var verticesCount = meshData.VerticesCount;

			meshData.AddVertex(cell.center); //Add Center Cell Vertex

			face.ForEachHalfEdge((halfEdge, index) =>
			{
				var firstIndex = verticesCount;                                     //Center Cell Index
				var secondIndex = verticesCount + index + 1;                        //Current HalfEdge Vertex Index
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

			meshData.AddColor(colors[cell.plateIndex]);                                  //Cell Center Vertex Color
			cell.Face.ForEachHalfEdge(() => meshData.AddColor(colors[cell.plateIndex])); //Cell Corner Vertex Color
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

		colors = new Color[planet.tectonicPlates.Length];
		for (int i = 0; i < planet.tectonicPlates.Length; i++)
			colors[i] = Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f);

		RecalculateMeshData();
		RecalculateMesh();
	}

	private void DrawMovementForCells(int offset, int count, Mesh mesh, Material material, Matrix4x4 matrix)
	{
		var matrices = new List<Matrix4x4>(count);
		for (int j = 0; j < count; j++)
		{
			var cell = planet.cells[offset + j];
			var cellRadius = (cell.Face.First.Vertex - cell.center).magnitude;

			var translation = cell.center + cell.normal * 0.0001f;
			var rotation = Quaternion.LookRotation(cell.linearDirection, cell.normal);
			var scale = new Vector3(0.0025f * planet.radius, 1.0f, cell.linearMagnitude / 5.0f * cellRadius);

			matrices.Add(matrix * Matrix4x4.TRS(translation, rotation, scale));
		}

		Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
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
	private void DrawPlateMovement()
	{
		if (planet == null || !renderPlateMovement) return;
		if (renderPlateMovementSetting.mesh == null || renderPlateMovementSetting.material == null) return;

		var drawCount = planet.cells.Length / 1023;
		for (int i = 0; i < drawCount; i++)
			DrawMovementForCells(1023 * i, 1023, renderPlateMovementSetting.mesh, renderPlateMovementSetting.material, transform.localToWorldMatrix);

		var countLeft = planet.cells.Length - 1023 * drawCount;
		DrawMovementForCells(1023 * drawCount, countLeft, renderPlateMovementSetting.mesh, renderPlateMovementSetting.material, transform.localToWorldMatrix);
	}
	private void DrawPlateBorder()
	{
		if (planet == null || !renderPlateBorder) return;

		for (int i = 0; i < planet.tectonicPlates.Length; i++)
		{
			var plate = planet.tectonicPlates[i];

			var matrices = new List<Matrix4x4>();
			for (int j = 0; j < plate.BorderVerticesCount; j += 2)
			{
				var first = plate.GetBorderVertex(j);
				var second = plate.GetBorderVertex(j + 1);

				var forward = second - first;

				var translation = (first + second) * 0.5f;
				var rotation = Quaternion.LookRotation(forward);

				matrices.Add(transform.localToWorldMatrix * Matrix4x4.TRS(translation, rotation, new Vector3(renderPolygonDataSetting.tickness, renderPolygonDataSetting.tickness, forward.magnitude)));
			}

			Graphics.DrawMeshInstanced(renderPolygonDataSetting.mesh, 0, renderPolygonDataSetting.material, matrices);
		}
	}

	private void Update()
	{
		DrawPlanet();

		DrawPolygonData();

		DrawPlateMovement();

		DrawPlateBorder();
	}

	private void OnValidate()
	{
		if (planet == null) return;

		RecalculateColors();
		ApplyMeshColor();
	}
}
