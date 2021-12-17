using UnityEngine;

using Miscellaneous;
using Miscellaneous.Unity;
using PoissonDisk;
using DelaunayVoronoi;

[System.Serializable]
public struct RenderDelaunaySetting
{
	public bool renderPoints;
	public bool renderHalfEdges;
	public bool renderBarycenters;
	public bool renderCircumcenters;

	public RenderDelaunaySetting(bool renderPoints, bool renderHalfEdges, bool renderBarycenters, bool renderCircumcenters)
	{
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

public class DiagramGenerator : MonoBehaviour
{
	[SerializeField] private PoissonDiskSetting poissonDiskSetting = PoissonDiskSetting.Default;
	[SerializeField] private TriangulationSetting triangulationSetting = TriangulationSetting.Default;
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

	public VoronoiDiagram Diagram => diagram;
	public DelaunayTriangulation Triangulation => triangulation;

	private void GenerateData()
	{
		Point2D[] points;
		poissonDiskSampling.ComputePoints(poissonDiskSetting, out points);

		delaunayCalculator.CalculateTriangulation(points, out triangulation, triangulationSetting);

		voronoiCalculator.CalculateDiagram(triangulation, out diagram, diagramSetting);
	}

	private void DrawDelaunayPoints()
	{
		for (int i = 0; i < triangulation.points.Length; i++)
		{
			var point = triangulation.points[i];

			Gizmos.color = Color.red;
			Gizmos.DrawSphere(point.ToVector3(), 0.1f);
		}
	}
	private void DrawDelaunayHalfEdges()
	{
		for (int i = 0; i < triangulation.halfEdges.Length; i++)
		{
			var halfEdge = triangulation.halfEdges[i];

			Gizmos.color = Color.gray;
			Gizmos.DrawLine(triangulation.points[halfEdge.pi0].ToVector3(), triangulation.points[halfEdge.pi1].ToVector3());
		}
	}
	private void DrawDelaunayBarycenters()
	{
		for (int i = 0; i < triangulation.barycenters.Length; i++)
		{
			var barycenter = triangulation.barycenters[i];

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(barycenter.ToVector3(), 0.1f);
		}
	}
	private void DrawDelaunayCircumcenters()
	{
		for (int i = 0; i < triangulation.circumcenters.Length; i++)
		{
			var circumcenter = triangulation.circumcenters[i];

			Gizmos.color = Color.green;
			Gizmos.DrawSphere(circumcenter.ToVector3(), 0.1f);
		}
	}

	private void DrawVoronoiSites()
	{
		for (int i = 0; i < diagram.sites.Length; i++)
		{
			var site = diagram.sites[i];

			Gizmos.color = Color.red;
			Gizmos.DrawSphere(site.ToVector3(), 0.1f);
		}
	}
	private void DrawVoronoiPoints()
	{
		for (int i = 0; i < diagram.points.Length; i++)
		{
			var point = diagram.points[i];

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(point.ToVector3(), 0.1f);
		}
	}
	private void DrawVoronoiHalfEdges()
	{
		for (int i = 0; i < diagram.halfEdges.Length; i++)
		{
			var halfEdge = diagram.halfEdges[i];

			Gizmos.color = Color.black;
			Gizmos.DrawLine(diagram.points[halfEdge.pi0].ToVector3(), diagram.points[halfEdge.pi1].ToVector3());
		}
	}

	public void Generate()
	{
		GenerateData();
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

	private void OnValidate()
	{
		if (autoGenerate)
			Generate();
	}
	private void OnDrawGizmos()
	{
		if (triangulation != null && diagram != null)
		{
			var translation = new Vector3(-0.5f, -0.5f, 0.0f);
			var rotation = Quaternion.identity;
			var scale = new Vector3(1.0f / poissonDiskSetting.areaWidth, 1.0f / poissonDiskSetting.areaHeight, 0.0f);

			Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(translation, rotation, scale);

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
