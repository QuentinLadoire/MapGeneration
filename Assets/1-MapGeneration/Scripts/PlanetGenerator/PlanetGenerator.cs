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
	[SerializeField] private int tectonicPlateCount = 5;
	[SerializeField] [Range(0.0f, 1.0f)] private float oceanicRate = 0.7f;

	private Planet planet = null;

	public void InitializePlanet()
	{
		DataStructureBuilder.CreateDualMeshData(MeshGenerator.CreateIcoSphere(planetRadius, planetRefiningStep), out DualHalfEdgeData dualMeshData);

		planet = new Planet(dualMeshData.polygonData);
	}
	public void InitializePlates()
	{
		var plateLenght = tectonicPlateCount;

		planet.tectonicPlates = new TectonicPlate[plateLenght];
		for (int i = 0; i < plateLenght; i++)
		{
			var isOceanic = Random.value < oceanicRate;

			planet.tectonicPlates[i] = new TectonicPlate(isOceanic, planet);
		}
	}

	public void InitializeVoronoi()
	{
		var plateLenght = tectonicPlateCount;

		for (int i = 0; i < plateLenght; i++)
		{
			var cellIndex = Random.Range(0, planet.cells.Length);

			planet.cells[cellIndex].plateIndex = i;
			planet.tectonicPlates[i].AddCell(cellIndex);
		}
	}
	public void VoronoiProcessPlates()
	{
		var plateLenght = tectonicPlateCount;
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];
			if (!cell.IsAssign)
			{
				var plates = planet.tectonicPlates;
				var sqrMagnitude = float.MaxValue;
				var nearestPlateIndex = -1;
				for (int j = 0; j < plateLenght; j++)
				{
					var plateCenter = plates[j].GetCenterCell().center;

					var toTest = (cell.center - plateCenter).sqrMagnitude;
					if (toTest < sqrMagnitude)
					{
						nearestPlateIndex = j;
						sqrMagnitude = toTest;
					}
				}

				cell.plateIndex = nearestPlateIndex;
				plates[nearestPlateIndex].AddCell(i);
			}
		}
	}

	public void Generate()
	{
		InitializePlanet();
		InitializePlates();
		InitializeVoronoi();
		VoronoiProcessPlates();

		GetComponent<PlanetRenderer>().SetPlanet(planet);
	}
}
