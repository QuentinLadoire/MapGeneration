using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry.DataStructure;

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

	public Planet planet = null;

	private void DrawPlanet()
	{
		if (renderPlanet && planet != null && planet.Mesh != null && material != null)
			Graphics.DrawMesh(planet.Mesh, transform.localToWorldMatrix, material, 0);
	}
	private void DrawDebug()
	{
		if (planet == null || planet.PolygonHalfEdgeData == null)
			return;

		if (renderPolygonData && renderPolygonDataSetting.mesh != null && renderPolygonDataSetting.material != null)
			DataStructureUtility.DrawHalfEdgeData(planet.PolygonHalfEdgeData, renderPolygonDataSetting, transform.localToWorldMatrix);
	}

	private void Update()
	{
		DrawPlanet();

		DrawDebug();
	}
}
