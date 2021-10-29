using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PoissonDisk;
using PoissonDisk.Unity;
using Delaunay;

public enum DrawMode
{
	Delaunay,
	Voronoi
}

public class DelaunayTest : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter = null;
	[SerializeField] private MeshRenderer meshRenderer = null;

	[SerializeField] private float radius = 5.0f;
	[SerializeField] private int seed = 0;

	[SerializeField] private DrawMode drawMode = DrawMode.Delaunay;

	private Point2D[] points = null;
	private DelaunayTriangulation triangulation = null;
	private DelaunayCalculator delaunayCalculator = new DelaunayCalculator();
	private PoissonDiskSampling poissonDiskSampling = new PoissonDiskSampling();

	private void DrawDelaunay()
	{
		var vertices = new Vector3[triangulation.Vertices.Count];
		for (int i = 0; i < triangulation.Vertices.Count; i++)
			vertices[i] = triangulation.Vertices[i].ToVector2();

		var triangles = new int[triangulation.Triangles.Count];
		for (int i = 0; i < triangulation.Triangles.Count; i += 3)
		{
			triangles[i] = triangulation.Triangles[i];
			triangles[i + 1] = triangulation.Triangles[i + 2];
			triangles[i + 2] = triangulation.Triangles[i + 1];
		}

		var uv = new Vector2[triangulation.Vertices.Count];
		for (int i = 0; i < triangulation.Vertices.Count; i++)
		{
			uv[i] = new Vector2
			{
				x = triangulation.Vertices[i].x / poissonDiskSampling.AreaWidth,
				y = triangulation.Vertices[i].y / poissonDiskSampling.AreaHeight
			};
		}

		var mesh = new Mesh
		{
			vertices = vertices,
			triangles = triangles,
			uv = uv
		};

		meshFilter.sharedMesh = mesh;
		meshRenderer.sharedMaterial.mainTexture = PoissonDiskUtility.GenerateTexture(points, poissonDiskSampling.AreaWidth, poissonDiskSampling.AreaHeight, 256);
	}

	private void Generate()
	{
		poissonDiskSampling.Radius = radius;
		poissonDiskSampling.Seed = seed;
		poissonDiskSampling.CreateHull = false;
		poissonDiskSampling.ComputePoints(ref points);

		delaunayCalculator.CalculateTriangulation(points, ref triangulation);

		DrawDelaunay();
	}

	private void OnValidate()
	{
		Generate();
	}
}
