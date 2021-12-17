using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DiagramGenerator))]
public class DiagramGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var target = this.target as DiagramGenerator;
		EditorGUILayout.Space();

		if (GUILayout.Button("Generate"))
		{
			target.Generate();
		}

		EditorGUILayout.Space();

		GUILayout.Label("Delaunay Logs", EditorStyles.boldLabel);
		if (target.Triangulation != null)
		{
			GUILayout.Label(target.LogDelaunayPointCount());
			GUILayout.Label(target.LogDelaunayTriangleCount());
			GUILayout.Label(target.LogDelaunayHalfEdgeCount());
		}

		EditorGUILayout.Space();

		GUILayout.Label("Voronoi Logs", EditorStyles.boldLabel);
		if (target.Diagram != null)
		{
			GUILayout.Label(target.LogVoronoiSiteCount());
			GUILayout.Label(target.LogVoronoiPointCount());
			GUILayout.Label(target.LogVoronoiHalfEdgeCount());
		}
	}
}
