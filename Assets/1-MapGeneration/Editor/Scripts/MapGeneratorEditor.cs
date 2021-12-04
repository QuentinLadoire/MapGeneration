using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var target = this.target as MapGenerator;

		EditorGUILayout.Space();
		GUILayout.Label("Logs", EditorStyles.boldLabel);
		GUILayout.Label(target.LogPointCount());
		GUILayout.Label(target.LogTriangleCount());
		GUILayout.Label(target.LogHalfEdgeCount());

		if (GUILayout.Button("Generate"))
		{
			target.Generate();
		}
	}
}
