using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Geometry;
using Geometry.DataStructure;

[ExecuteAlways]
public class PlanetGenerator : MonoBehaviour
{
	[Header("Planet Settings")]
	[SerializeField] private int seed = 0;
	[SerializeField] private float planetRadius = 1.0f;
	[SerializeField] private int planetRefiningStep = 3;
	[SerializeField] private int tectonicPlateCount = 5;
	[SerializeField] private float angularVelocityMax = 1.0f;
	[SerializeField][Range(0.0f, 1.0f)] private float oceanicRate = 0.7f;

	private Planet planet = null;

	public void InitializePlanet()
	{
		DataStructureBuilder.CreateDualMeshData(MeshGenerator.CreateIcoSphere(planetRadius, planetRefiningStep), out DualHalfEdgeData dualMeshData);

		planet = new Planet(dualMeshData.polygonData);
		planet.radius = planetRadius;

		Random.InitState(seed);
	}
	public void InitializePlates()
	{
		var plateLenght = tectonicPlateCount;

		planet.tectonicPlates = new TectonicPlate[plateLenght];
		for (int i = 0; i < plateLenght; i++)
		{
			var cellIndex = Random.Range(0, planet.cells.Length);

			var angularAxis = Random.onUnitSphere;
			var angularMagnitude = Mathf.Clamp(Random.value, 0.1f, 1.0f) * angularVelocityMax;

			var isOceanic = Random.value < oceanicRate;

			planet.tectonicPlates[i] = new TectonicPlate
			{
				parentPlanet = planet,

				isOceanic = isOceanic,
				angularAxis = angularAxis,
				angularMagnitude = angularMagnitude
			};

			planet.cells[cellIndex].plateIndex = i;
			planet.tectonicPlates[i].AddCell(cellIndex);
		}
	}

	public void ProcessPlatesWithVoronoi()
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
	public void DeterminePlateBorder()
	{
		var platesBorders = new List<int>[planet.tectonicPlates.Length];
		for (int i = 0; i < platesBorders.Length; i++)
			platesBorders[i] = new List<int>();

		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];
			cell.Face.ForEachHalfEdge((halfEdge, index) =>
			{
				var otherCell = planet.cells[halfEdge.Opposite.faceIndex];

				if (cell.plateIndex != otherCell.plateIndex)
				{
					cell.Plate.AddBorderVertex(halfEdge.vertexIndex);
					cell.Plate.AddBorderVertex(halfEdge.Next.vertexIndex);
				}
			});
		}
	}

	public void Generate()
	{
		InitializePlanet();
		InitializePlates();
		ProcessPlatesWithVoronoi();
		DeterminePlateBorder();

		GetComponent<PlanetRenderer>().SetPlanet(planet);
	}
}
