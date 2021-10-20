using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GK;
using PoissonDisk;
using PoissonDisk.Unity;

public class DelaunayTest : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter = null;
	[SerializeField] private MeshRenderer meshRenderer = null;

	[SerializeField] private float radius = 5.0f;
	[SerializeField] private int seed = 0;

    private DelaunayCalculator delaunayCalculator = new DelaunayCalculator();
	private PoissonDiskSampling poissonDiskSampling = new PoissonDiskSampling();

	private void Generate()
	{
		Point2D[] points = null;
		poissonDiskSampling.Radius = radius;
		poissonDiskSampling.Seed = seed;
		poissonDiskSampling.CreateHull = true;
		poissonDiskSampling.ComputePoints(ref points);

		DelaunayTriangulation triangulation = null;
		delaunayCalculator.CalculateTriangulation(points.ToVector2Array(), ref triangulation);

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
			vertices = triangulation.Vertices.ToVector3Array(),
			triangles = triangulation.Triangles.ToArray(),
			uv = uv
		};

		meshFilter.sharedMesh = mesh;
		meshRenderer.sharedMaterial.mainTexture = PoissonDiskUtility.GenerateTexture(points, poissonDiskSampling.AreaWidth, poissonDiskSampling.AreaHeight, 256);
	}

	private void OnValidate()
	{
		Generate();
	}
}
