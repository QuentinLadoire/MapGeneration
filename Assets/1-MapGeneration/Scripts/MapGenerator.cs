using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Miscellaneous;
using Miscellaneous.Unity;
using PoissonDisk;
using DelaunayVoronoi;

[ExecuteAlways]
public class MapGenerator : MonoBehaviour
{
	[Header("PoissonDisk Settings")]
	[SerializeField] private int seed = 0;
	[SerializeField] private float radius = 1.0f;
	[SerializeField] private bool createHull = false;
	[SerializeField] private int sampleLimitBeforeRejection = 30;
	[SerializeField] private Vector2 areaSize = new Vector2(30.0f, 30.0f);

	[Header("Diagram Setting")]
	[SerializeField] private bool fromBarycenter = false;

	[Header("Render Settings")]
	[SerializeField] private bool renderDelaunayMesh = false;
	[SerializeField] private bool renderDelaunayPoints = false;
	[SerializeField] private bool renderDelaunayHalfEdge = false;
	[SerializeField] private bool renderDelaunayBarycenter = false;
	[SerializeField] private bool renderDelaunayCircumcenter = false;

	[SerializeField] private bool renderVoronoiSite = false;
	[SerializeField] private bool renderVoronoiPoint = false;
	[SerializeField] private bool renderVoronoiHalfEdge = false;

	[Header("MapGenerator Setting")]
	[SerializeField] private bool autoGenerate = false;

	private VoronoiDiagram diagram = null;
	private DelaunayTriangulation triangulation = null;
	private VoronoiCalculator voronoiCalculator = new VoronoiCalculator();
	private DelaunayCalculator delaunayCalculator = new DelaunayCalculator();
	private PoissonDiskSampling poissonDiskSampling = new PoissonDiskSampling();

	private MeshFilter meshFilter = null;
	private MeshRenderer meshRenderer = null;

	public DelaunayTriangulation Triangulation => triangulation;

	private void GenerateData()
	{
		poissonDiskSampling.Seed = seed;
		poissonDiskSampling.Radius = radius;
		poissonDiskSampling.AreaWidth = areaSize.x;
		poissonDiskSampling.AreaHeight = areaSize.y;
		poissonDiskSampling.CreateHull = createHull;
		poissonDiskSampling.SampleLimitBeforeRejection = sampleLimitBeforeRejection;

		Point2D[] points;
		poissonDiskSampling.ComputePoints(out points);

		delaunayCalculator.CalculateTriangulation(points, out triangulation, true);
		triangulation.CalculateDataStruct();

		voronoiCalculator.CalculateDiagram(triangulation, out diagram, fromBarycenter);
	}
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

	private void DrawDelaunayPoints()
	{
		for (int i = 0; i < triangulation.points.Length; i++)
		{
			var point = triangulation.points[i];

			Gizmos.color = Color.red;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, transform.localScale.y, 0.0f));
			Gizmos.DrawSphere(point.ToVector3(), 0.1f);
		}
	}
	private void DrawDelaunayHalfEdges()
	{
		for (int i = 0; i < triangulation.halfEdges.Length; i++)
		{
			var halfEdge = triangulation.halfEdges[i];

			Gizmos.color = Color.gray;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, transform.localScale.y, 0.0f));
			Gizmos.DrawLine(triangulation.points[halfEdge.pi0].ToVector3(), triangulation.points[halfEdge.pi1].ToVector3());
		}
	}
	private void DrawDelaunayBarycenters()
	{
		for (int i = 0; i < triangulation.barycenters.Length; i++)
		{
			var barycenter = triangulation.barycenters[i];

			Gizmos.color = Color.blue;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, transform.localScale.y, 0.0f));
			Gizmos.DrawSphere(barycenter.ToVector3(), 0.1f);
		}
	}
	private void DrawDelaunayCircumcenters()
	{
		for (int i = 0; i < triangulation.circumcenters.Length; i++)
		{
			var circumcenter = triangulation.circumcenters[i];

			Gizmos.color = Color.green;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, transform.localScale.y, 0.0f));
			Gizmos.DrawSphere(circumcenter.ToVector3(), 0.1f);
		}
	}

	private void DrawVoronoiSites()
	{
		for (int i = 0; i < diagram.sites.Length; i++)
		{
			var site = diagram.sites[i];

			Gizmos.color = Color.red;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, transform.localScale.y, 0.0f));
			Gizmos.DrawSphere(site.ToVector3(), 0.1f);
		}
	}
	private void DrawVoronoiPoints()
	{
		for (int i = 0; i < diagram.points.Length; i++)
		{
			var point = diagram.points[i];

			Gizmos.color = Color.blue;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, transform.localScale.y, 0.0f));
			Gizmos.DrawSphere(point.ToVector3(), 0.1f);
		}
	}
	private void DrawVoronoiHalfEdges()
	{
		for (int i = 0; i < diagram.halfEdges.Length; i++)
		{
			var halfEdge = diagram.halfEdges[i];

			Gizmos.color = Color.black;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, transform.localScale.y, 0.0f));
			Gizmos.DrawLine(diagram.points[halfEdge.pi0].ToVector3(), diagram.points[halfEdge.pi1].ToVector3());
		}
	}

	public void Generate()
	{
		GenerateData();
		CreateMesh();

		meshRenderer.enabled = renderDelaunayMesh;
	}
	public string LogPointCount()
	{
		return string.Format("Point Count : {0}", triangulation.points.Length);
	}
	public string LogTriangleCount()
	{
		return string.Format("Triangle Count : {0}", triangulation.triangles.Length);
	}
	public string LogHalfEdgeCount()
	{
		return string.Format("HalfEdge Count : {0}", triangulation.halfEdges.Length);
	}

	private void Awake()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
	}
	private void Start()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
	}

	private void OnValidate()
	{
		if (autoGenerate)
			Generate();
	}
	private void OnDrawGizmos()
	{
		if (triangulation != null && diagram != null)
		{
			if (renderDelaunayHalfEdge)
				DrawDelaunayHalfEdges();

			if (renderVoronoiHalfEdge)
				DrawVoronoiHalfEdges();

			if (renderDelaunayPoints)
				DrawDelaunayPoints();

			if (renderDelaunayBarycenter)
				DrawDelaunayBarycenters();

			if (renderDelaunayCircumcenter)
				DrawDelaunayCircumcenters();

			if (renderVoronoiSite)
				DrawVoronoiSites();

			if (renderVoronoiPoint)
				DrawVoronoiPoints();
		}
	}
}
