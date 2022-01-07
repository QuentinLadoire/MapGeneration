using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorEditor : Editor
{
	private PlanetGenerator planetGenerator = null;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		planetGenerator = target as PlanetGenerator;

		EditorGUILayout.Space();

		if (GUILayout.Button("Generate"))
		{
			planetGenerator.Generate();
		}
	}
}
