using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry;
using Geometry.DataStructure;

[ExecuteAlways]
public class PlanetGenerator : MonoBehaviour
{
	[Header("Planet Settings")]
	[SerializeField] private float planetRadius = 1.0f;
	[SerializeField] private int planetRefiningStep = 3;
	[SerializeField] private int planetTectonicPlateCount = 5;

	private Planet planet = null;

	public void Generate()
	{
		DataStructureBuilder.CreateDualMeshData(MeshGenerator.CreateIcoSphere(planetRadius, planetRefiningStep), out DualHalfEdgeData dualMeshData);

		planet = new Planet(dualMeshData.polygonData);
		planet.ComputeTectonicPlates(planetTectonicPlateCount);

		GetComponent<PlanetRenderer>().SetPlanet(planet);
	}
}
