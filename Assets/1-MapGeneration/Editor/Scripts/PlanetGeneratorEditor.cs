using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var planetGenerator = target as PlanetGenerator;

		EditorGUILayout.Space();

		if (GUILayout.Button("Generate"))
		{
			planetGenerator.Generate();
		}
	}
}
