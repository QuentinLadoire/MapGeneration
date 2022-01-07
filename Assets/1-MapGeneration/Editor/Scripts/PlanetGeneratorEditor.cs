using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorEditor : Editor
{
	private bool stepByStep = false;
	private bool auto = false;
	private float stepByStepDelay = 0.2f;

	private float currentDelay = 0.0f;
	private float previousRealTime = 0.0f;

	private PlanetGenerator planetGenerator = null;

	private void BFSStepByStep()
	{
		var deltaTime = Time.realtimeSinceStartup - previousRealTime;

		if (planetGenerator.HasFinish)
		{
			currentDelay = stepByStepDelay;
			
			EditorApplication.update -= BFSStepByStep;
		}
		else if (auto)
		{
			if (currentDelay >= stepByStepDelay)
			{
				planetGenerator.BFSProcessPlatesStepByStep();
				currentDelay = 0.0f;
			}
			else
				currentDelay += deltaTime;
		}

		previousRealTime = Time.realtimeSinceStartup;
	}
	private void VoronoiStepByStep()
	{
		var deltaTime = Time.realtimeSinceStartup - previousRealTime;

		if (planetGenerator.HasFinish)
		{
			currentDelay = stepByStepDelay;

			EditorApplication.update -= VoronoiStepByStep;
		}
		else if (auto)
		{
			if (currentDelay >= stepByStepDelay)
			{
				planetGenerator.VoronoiProcessPlatesStepByStep();
				currentDelay = 0.0f;
			}
			else
				currentDelay += deltaTime;
		}

		previousRealTime = Time.realtimeSinceStartup;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		planetGenerator = target as PlanetGenerator;

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
		stepByStep = EditorGUILayout.Toggle("Step By Step", stepByStep);
		auto = EditorGUILayout.Toggle("Auto Generation", auto);
		stepByStepDelay = EditorGUILayout.FloatField("Step By Step Delay", stepByStepDelay);

		EditorGUILayout.Space();

		if (GUILayout.Button("Generate From BFS"))
		{
			if (!planetGenerator.HasFinish)
			{
				currentDelay = stepByStepDelay;
				EditorApplication.update -= BFSStepByStep;
			}

			planetGenerator.InitializePlanet();

			planetGenerator.InitializePlates();

			planetGenerator.InitializeBFS();

			if (stepByStep)
				EditorApplication.update += BFSStepByStep;
			else
				planetGenerator.BFSProcessPlates();
		}

		if (!auto && GUILayout.Button("BFS Step By Step"))
		{
			planetGenerator.BFSProcessPlatesStepByStep();
		}

		if (GUILayout.Button("Generate From Voronoi"))
		{
			if (!planetGenerator.HasFinish)
			{
				currentDelay = stepByStepDelay;
				EditorApplication.update -= VoronoiStepByStep;
			}

			planetGenerator.InitializePlanet();

			planetGenerator.InitializePlates();

			planetGenerator.InitializeVoronoi();

			if (stepByStep)
				EditorApplication.update += VoronoiStepByStep;
			else
				planetGenerator.VoronoiProcessPlates();
		}

		if (!auto && GUILayout.Button("Voronoi Step By Step"))
		{
			planetGenerator.VoronoiProcessPlatesStepByStep();
		}
	}
}
