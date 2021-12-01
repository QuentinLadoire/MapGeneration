using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Miscellaneous;
using Miscellaneous.Unity;
using PoissonDisk;
using Delaunay;

[ExecuteAlways]
public class MapGenerator : MonoBehaviour
{
	[Header("PoissonDisk Settings")]
	[SerializeField] private int seed = 0;
	[SerializeField] private float radius = 1.0f;
	[SerializeField] private int sampleLimitBeforeRejection = 30;
	[SerializeField] private Vector2 areaSize = new Vector2(30.0f, 30.0f);

	[Header("Render Settings")]
	[SerializeField] private bool renderMesh = false;
	[SerializeField] private bool renderPoints = false;
	[SerializeField] private bool renderHalfEdge = false;
	[SerializeField] private bool renderBarycenter = false;
	[SerializeField] private bool renderCircumcenter = false;

	private Point2D[] points = null;
	private DelaunayTriangulation triangulation = null;
	private DelaunayCalculator delaunayCalculator = new DelaunayCalculator();
	private PoissonDiskSampling poissonDiskSampling = new PoissonDiskSampling();

	private MeshFilter meshFilter = null;
	private MeshRenderer meshRenderer = null;

	private void CreateMesh()
	{
		var vertices = new Vector3[triangulation.points.Length];
		for (int i = 0; i < triangulation.points.Length; i++)
			vertices[i] = triangulation.points[i].ToVector3();

		var triangles = triangulation.indices;

		var uv = new Vector2[triangulation.points.Length];
		for (int i = 0; i < triangulation.points.Length; i++)
		{
			uv[i] = new Vector2
			{
				x = triangulation.points[i].x / areaSize.x,
				y = triangulation.points[i].y / areaSize.y
			};
		}

		var mesh = new Mesh
		{
			vertices = vertices,
			triangles = triangles,
			uv = uv
		};

		meshFilter.mesh = mesh;
	}

	private void DrawPoints()
	{
		for (int i = 0; i < triangulation.points.Length; i++)
		{
			var point = triangulation.points[i];

			Gizmos.color = Color.red;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1.0f, 1.0f, 0.0f));
			Gizmos.DrawSphere(point.ToVector3(), 0.1f);
		}
	}
	private void DrawHalfEdges()
	{
		for (int i = 0; i < triangulation.halfEdges.Length; i++)
		{
			var halfEdge = triangulation.halfEdges[i];

			Gizmos.color = Color.black;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1.0f, 1.0f, 0.0f));
			Gizmos.DrawLine(triangulation.points[halfEdge.pi0].ToVector3(), triangulation.points[halfEdge.pi1].ToVector3());
		}
	}
	private void DrawBarycenters()
	{
		for (int i = 0; i < triangulation.barycenters.Length; i++)
		{
			var barycenter = triangulation.barycenters[i];

			Gizmos.color = Color.blue;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1.0f, 1.0f, 0.0f));
			Gizmos.DrawSphere(barycenter.ToVector3(), 0.1f);
		}
	}
	private void DrawCircumcenters()
	{
		for (int i = 0; i < triangulation.circumcenters.Length; i++)
		{
			var circumcenter = triangulation.circumcenters[i];

			Gizmos.color = Color.green;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1.0f, 1.0f, 0.0f));
			Gizmos.DrawSphere(circumcenter.ToVector3(), 0.1f);
		}
	}

	public void GenerateData()
	{
		poissonDiskSampling.Seed = seed;
		poissonDiskSampling.Radius = radius;
		poissonDiskSampling.AreaWidth = areaSize.x;
		poissonDiskSampling.AreaHeight = areaSize.y;
		poissonDiskSampling.SampleLimitBeforeRejection = sampleLimitBeforeRejection;

		poissonDiskSampling.ComputePoints(ref points);
		delaunayCalculator.CalculateTriangulation(points, out triangulation, true);
		triangulation.CalculateDataStruct();

		Debug.Log(string.Format("Triangulation - Triangles count : {0} - HalfEdges count : {1}", triangulation.triangles.Length, triangulation.halfEdges.Length));
	}
	public void Generate()
	{
		GenerateData();
		CreateMesh();

		meshRenderer.enabled = renderMesh;
	}

	private void Start()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
	}

	private void OnValidate()
	{
		Generate();
	}
	private void OnDrawGizmos()
	{
		if (renderHalfEdge)
			DrawHalfEdges();

		if (renderPoints)
			DrawPoints();

		if (renderBarycenter)
			DrawBarycenters();

		if (renderCircumcenter)
			DrawCircumcenters();
	}
}
