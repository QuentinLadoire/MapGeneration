using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseGenerator))]
public class NoiseGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var target = this.target as NoiseGenerator;
		EditorGUILayout.Space();

		if (GUILayout.Button("Generate"))
		{
			target.Generate();
		}
	}
}
