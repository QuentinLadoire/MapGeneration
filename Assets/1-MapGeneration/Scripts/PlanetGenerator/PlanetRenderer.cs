using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry;
using Geometry.DataStructure;

[System.Serializable]
public class RenderPlanet
{
	public enum RenderMode
	{
		Plates,
		Terrain
	}

	public Material material = null;
	public RenderMode renderMode = RenderMode.Plates;

	public Color oceanColor = Color.white;
	public Color groundColor = Color.white;

	private Mesh mesh = null;
	private Planet planet = null;
	private Color[] colors = null;
	private MeshData meshData = null;

	private void InitializeMesh()
	{
		if (mesh == null)
			mesh = new Mesh();
	}
	private void InitializeColors()
	{
		colors = new Color[planet.tectonicPlates.Length];
		for (int i = 0; i < planet.tectonicPlates.Length; i++)
			colors[i] = Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
	}
	private void InitializeMeshData()
	{
		if (meshData == null)
			meshData = new MeshData();
		else
			meshData.Clear();
	}

	public void SetPlanet(Planet planet)
	{
		this.planet = planet;

		InitializeMesh();
		InitializeColors();
		InitializeMeshData();
	}

	private void ComputeCellsColor()
	{
		switch (renderMode)
		{
			case RenderMode.Plates:
				ComputePlatesColor();
				break;

			case RenderMode.Terrain:
				ComputeTerrainColor();
				break;
		}
	}
	private void ComputePlatesColor()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];
			var plate = cell.Plate;

			meshData.AddColor(colors[cell.plateIndex]);                                  //Cell Center Vertex Color
			cell.Face.ForEachHalfEdge(() => meshData.AddColor(colors[cell.plateIndex])); //Cell Corner Vertex Color
		}
	}
	private void ComputeTerrainColor()
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
	private void ComputeCellsGeometry()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];

			var face = cell.Face;
			var faceEdgeCount = face.HalfEdgeCount;
			var verticesCount = meshData.VerticesCount;

			meshData.AddVertex(cell.position); //Add Center Cell Vertex

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

	public void RecalculateColors()
	{
		if (meshData == null) return;

		meshData.ClearColors();

		ComputeCellsColor();
	}
	public void RecalculateGeometry()
	{
		if (meshData == null) return;

		meshData.Clear();

		ComputeCellsGeometry();
	}

	public void ApplyMeshColor()
	{
		if (mesh == null) return;

		mesh.SetColors(meshData.Colors);
	}
	public void ApplyMeshGeometry()
	{
		if (mesh == null) return;

		mesh.SetVertices(meshData.Vertices);
		mesh.SetTriangles(meshData.Triangles, 0);

		mesh.RecalculateNormals();
	}

	public void DrawPlanet(Matrix4x4 planetMatrice)
	{
		if (planet == null || mesh == null || material == null) return;

		Graphics.DrawMesh(mesh, planetMatrice, material, 0);
	}
}

[System.Serializable]
public class RenderBoundaries
{
	public enum RenderMode
	{
		Boundary,
		BoundaryType
	}

	public Mesh mesh = null;
	public Material material = null;
	public Material transformMaterial = null;
	public Material convergentMaterial = null;
	public Material divergentMaterial = null;
	public RenderMode renderMode = RenderMode.Boundary;

	private Planet planet = null;

	public void SetPlanet(Planet planet)
	{
		this.planet = planet;
	}

	private Matrix4x4 GetBoundaryMatrice(Boundary boundary)
	{
		var p0 = boundary.Edge.FirstHalfEdge.Vertex;
		var p1 = boundary.Edge.SecondHalfEdge.Vertex;

		var forward = p1 - p0;
		var upwards = -Vector3.Cross(boundary.parallelVector, boundary.perpendicularVector);

		var translation = boundary.MidPoint + upwards * 0.0001f;
		var rotation = Quaternion.LookRotation(forward, upwards);
		var scale = new Vector3(0.005f, 0.005f, forward.magnitude);

		return Matrix4x4.TRS(translation, rotation, scale);
	}

	private void DrawBoundaries(List<Matrix4x4> matrices, int offset, int count, Material material)
	{
		Graphics.DrawMeshInstanced(mesh, 0, material, matrices.GetRange(offset, count));
	}
	private void DrawBoundariesFor(List<Matrix4x4> matrices, Material material)
	{
		var drawCount = matrices.Count / 1023;
		for (int i = 0; i < drawCount; i++)
			DrawBoundaries(matrices, 1023 * i, 1023, material);

		var countLeft = matrices.Count - 1023 * drawCount;
		DrawBoundaries(matrices, 1023 * drawCount, countLeft, material);
	}
	private void DrawBoundariesDefault(Matrix4x4 planetMatrice)
	{
		var matrices = new List<Matrix4x4>();
		for (int i = 0; i < planet.boundaries.Count; i++)
			matrices.Add(planetMatrice * GetBoundaryMatrice(planet.boundaries[i]));

		DrawBoundariesFor(matrices, material);
	}
	private void DrawBoundariesType(Matrix4x4 planetMatrice)
	{
		var transformMatrices = new List<Matrix4x4>();
		var convergentMatrices = new List<Matrix4x4>();
		var divergentMatrices = new List<Matrix4x4>();

		for (int i = 0; i < planet.boundaries.Count; i++)
		{
			var boundary = planet.boundaries[i];
			switch (boundary.type)
			{
				case BoundaryType.Transform:
					transformMatrices.Add(planetMatrice * GetBoundaryMatrice(boundary));
					break;

				case BoundaryType.Divergent:
					divergentMatrices.Add(planetMatrice * GetBoundaryMatrice(boundary));
					break;

				case BoundaryType.Convergent:
					convergentMatrices.Add(planetMatrice * GetBoundaryMatrice(boundary));
					break;
			}
		}

		DrawBoundariesFor(transformMatrices, transformMaterial);
		DrawBoundariesFor(convergentMatrices, convergentMaterial);
		DrawBoundariesFor(divergentMatrices, divergentMaterial);
	}

	public void DrawBoundaries(Matrix4x4 planetMatrice)
	{
		if (planet == null || mesh == null || material == null) return;

		switch (renderMode)
		{
			case RenderMode.Boundary:
				DrawBoundariesDefault(planetMatrice);
				break;

			case RenderMode.BoundaryType:
				DrawBoundariesType(planetMatrice);
				break;
		}
	}
}

[System.Serializable]
public class RenderPolygonData
{
	public Mesh mesh = null;
	public Material material = null;

	private Planet planet = null;

	public void SetPlanet(Planet planet)
	{
		this.planet = planet;
	}

	public void DrawPolygonData(Matrix4x4 planetMatrice)
	{
		if (planet == null || planet.polygonHalfEdgeData == null || mesh == null || material == null) return;

		DataStructureUtility.DrawHalfEdgeData(planet.polygonHalfEdgeData, mesh, material, planetMatrice);
	}
}

[System.Serializable]
public class RenderPlateMovement
{
	public Mesh mesh = null;
	public Material material = null;

	private Planet planet = null;

	public void SetPlanet(Planet planet)
	{
		this.planet = planet;
	}

	private Matrix4x4 GetPlateMovementMatriceAtCellPosition(Cell cell)
	{
		var cellRadius = (cell.Face.First.Vertex - cell.position).magnitude;

		var translation = cell.position + cell.normal * 0.0001f;
		var rotation = Quaternion.LookRotation(cell.linearDirection, cell.normal);
		var scale = new Vector3(0.0025f * planet.radius, 1.0f, cell.linearMagnitude / planet.angularVelocityMax * cellRadius);

		return Matrix4x4.TRS(translation, rotation, scale);
	}
	private void DrawPlateMovementFor(int offset, int count, Matrix4x4 planetMatrice)
	{
		var matrices = new Matrix4x4[count];
		for (int i = 0; i < count; i++)
			matrices[i] = planetMatrice * GetPlateMovementMatriceAtCellPosition(planet.cells[offset + i]);

		Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
	}
	public void DrawPlateMovement(Matrix4x4 planetMatrice)
	{
		if (planet == null || mesh == null || material == null) return;

		var drawCount = planet.cells.Length / 1023;
		for (int i = 0; i < drawCount; i++)
			DrawPlateMovementFor(1023 * i, 1023, planetMatrice);

		var countLeft = planet.cells.Length - 1023 * drawCount;
		DrawPlateMovementFor(1023 * drawCount, countLeft, planetMatrice);
	}
}

[ExecuteAlways]
public class PlanetRenderer : MonoBehaviour
{
	[SerializeField] private RenderPlanet renderPlanet = new RenderPlanet();
	[SerializeField] private RenderBoundaries renderBoundaries = new RenderBoundaries();
	[SerializeField] private RenderPolygonData renderPolygonData = new RenderPolygonData();
	[SerializeField] private RenderPlateMovement renderPlateMovement = new RenderPlateMovement();

	[Header("Render Options")]
	[SerializeField] private bool wantToRenderPlanet = false;
	[SerializeField] private bool wantToRenderBoundaries = false;
	[SerializeField] private bool wantToRenderPolygonData = false;
	[SerializeField] private bool wantToRenderPlateMovement = false;

	private Planet planet = null;

	public void RecalculateMesh()
	{
		renderPlanet.ApplyMeshGeometry();
		renderPlanet.ApplyMeshColor();
	}
	public void RecalculateMeshData()
	{
		renderPlanet.RecalculateGeometry();
		renderPlanet.RecalculateColors();
	}

	public void SetPlanet(Planet planet)
	{
		this.planet = planet;

		renderPlanet.SetPlanet(planet);
		renderBoundaries.SetPlanet(planet);
		renderPolygonData.SetPlanet(planet);
		renderPlateMovement.SetPlanet(planet);

		RecalculateMeshData();
		RecalculateMesh();
	}

	private void Update()
	{
		if (wantToRenderPlanet)
			renderPlanet.DrawPlanet(transform.localToWorldMatrix);

		if (wantToRenderBoundaries)
			renderBoundaries.DrawBoundaries(transform.localToWorldMatrix);

		if (wantToRenderPolygonData)
			renderPolygonData.DrawPolygonData(transform.localToWorldMatrix);

		if (wantToRenderPlateMovement)
			renderPlateMovement.DrawPlateMovement(transform.localToWorldMatrix);
	}

	private void OnValidate()
	{
		if (planet == null) return;

		RecalculateMeshData();
		RecalculateMesh();
	}
}
