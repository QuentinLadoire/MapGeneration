using UnityEngine;

using Miscellaneous;
using Miscellaneous.Unity;
using PoissonDisk;
using DelaunayVoronoi;

[System.Serializable]
public struct RenderDelaunaySetting
{
	public bool renderMesh;
	public bool renderPoints;
	public bool renderHalfEdges;
	public bool renderBarycenters;
	public bool renderCircumcenters;

	public RenderDelaunaySetting(bool renderMesh, bool renderPoints, bool renderHalfEdges, bool renderBarycenters, bool renderCircumcenters)
	{
		this.renderMesh = renderMesh;
		this.renderPoints = renderPoints;
		this.renderHalfEdges = renderHalfEdges;
		this.renderBarycenters = renderBarycenters;
		this.renderCircumcenters = renderCircumcenters;
	}
}

[System.Serializable]
public struct RenderVoronoiSetting
{
	public bool renderSites;
	public bool renderPoints;
	public bool renderHalfEdges;

	public RenderVoronoiSetting(bool renderSites, bool renderPoints, bool renderHalfEdges)
	{
		this.renderSites = renderSites;
		this.renderPoints = renderPoints;
		this.renderHalfEdges = renderHalfEdges;
	}
}

[ExecuteAlways]
public class MapGenerator : MonoBehaviour
{
	[SerializeField] private PoissonDiskSetting poissonDiskSetting = PoissonDiskSetting.Default;

	[Header("Voronoi Setting")]
	[SerializeField] private DiagramSetting diagramSetting = new DiagramSetting();

	[Header("Render Settings")]
	[SerializeField] private RenderDelaunaySetting renderDelaunaySetting = new RenderDelaunaySetting();
	[SerializeField] private RenderVoronoiSetting renderVoronoiSetting = new RenderVoronoiSetting();

	[Header("MapGenerator Setting")]
	[SerializeField] private bool autoGenerate = false;

	private VoronoiDiagram diagram = null;
	private DelaunayTriangulation triangulation = null;
	private VoronoiCalculator voronoiCalculator = new VoronoiCalculator();
	private DelaunayCalculator delaunayCalculator = new DelaunayCalculator();
	private PoissonDiskSampling poissonDiskSampling = new PoissonDiskSampling();

	private MeshFilter meshFilter = null;
	private MeshRenderer meshRenderer = null;

	public VoronoiDiagram Diagram => diagram;
	public DelaunayTriangulation Triangulation => triangulation;

	private void GenerateData()
	{
		Point2D[] points;
		poissonDiskSampling.ComputePoints(poissonDiskSetting, out points);

		delaunayCalculator.CalculateTriangulation(points, out triangulation, true);

		voronoiCalculator.CalculateDiagram(triangulation, out diagram, diagramSetting);
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
				x = triangulation.points[i].x / poissonDiskSetting.areaWidth,
				y = triangulation.points[i].y / poissonDiskSetting.areaHeight
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

		meshRenderer.enabled = renderDelaunaySetting.renderMesh;
	}

	public string LogDelaunayPointCount()
	{
		return string.Format("Point Count : {0}", triangulation.points.Length);
	}
	public string LogDelaunayTriangleCount()
	{
		return string.Format("Triangle Count : {0}", triangulation.triangles.Length);
	}
	public string LogDelaunayHalfEdgeCount()
	{
		return string.Format("HalfEdge Count : {0}", triangulation.halfEdges.Length);
	}

	public string LogVoronoiSiteCount()
	{
		return string.Format("Site Count : {0}", diagram.sites.Length);
	}
	public string LogVoronoiPointCount()
	{
		return string.Format("Point Count : {0}", diagram.points.Length);
	}
	public string LogVoronoiHalfEdgeCount()
	{
		return string.Format("HalfEdge Count : {0}", diagram.halfEdges.Length);
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
			if (renderDelaunaySetting.renderHalfEdges)
				DrawDelaunayHalfEdges();

			if (renderVoronoiSetting.renderHalfEdges)
				DrawVoronoiHalfEdges();

			if (renderDelaunaySetting.renderPoints)
				DrawDelaunayPoints();

			if (renderDelaunaySetting.renderBarycenters)
				DrawDelaunayBarycenters();

			if (renderDelaunaySetting.renderCircumcenters)
				DrawDelaunayCircumcenters();

			if (renderVoronoiSetting.renderSites)
				DrawVoronoiSites();

			if (renderVoronoiSetting.renderPoints)
				DrawVoronoiPoints();
		}
	}
}
