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

	private int voronoiStepIndex = -1;
	private int remainingFreeCellCount = 0;
	private Queue<int>[] processingCellIndexQueue = null;

	public bool HasFinish => remainingFreeCellCount == 0 || voronoiStepIndex == planet.cells.Length;

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

	private void BreathFirstFloodFillStep()
	{
		for (int i = 0; i < planet.tectonicPlates.Length; i++)
		{
			var plateIndex = i;
			var plate = planet.tectonicPlates[plateIndex];

			var queue = processingCellIndexQueue[plateIndex];

			if (queue.Count == 0)
				continue;

			var hasAddCell = false;
			do
			{
				var cellIndex = queue.Dequeue();
				var cell = planet.cells[cellIndex];

				if (!cell.IsAssign)
				{
					cell.plateIndex = i;
					plate.AddCell(cellIndex);

					hasAddCell = true;

					remainingFreeCellCount--;


					var halfEdge = cell.Face.First;
					for (int j = 0; j < cell.Face.edgeCount; j++)
					{
						var adjacentIndex = halfEdge.Opposite.faceIndex;
						var adjacentCell = planet.cells[adjacentIndex];

						if (!adjacentCell.IsAssign)
							queue.Enqueue(adjacentIndex);

						halfEdge = halfEdge.Next;
					}
				}
			}
			while (!hasAddCell && queue.Count != 0);
		}
	}
	public void InitializeBFS()
	{
		var plateLenght = tectonicPlateCount;

		processingCellIndexQueue = new Queue<int>[plateLenght];
		for (int i = 0; i < plateLenght; i++)
		{
			processingCellIndexQueue[i] = new Queue<int>();
			processingCellIndexQueue[i].Enqueue(Random.Range(0, planet.cells.Length));
		}

		remainingFreeCellCount = planet.cells.Length;
	}
	public void BFSProcessPlates()
	{
		while (remainingFreeCellCount > 0)
		{
			BreathFirstFloodFillStep();
		}
	}
	public void BFSProcessPlatesStepByStep()
	{
		if (remainingFreeCellCount > 0)
		{
			BreathFirstFloodFillStep();
		}
	}

	private void VoronoiFloodFillStep(int i)
	{
		var plateLenght = tectonicPlateCount;

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
	public void InitializeVoronoi()
	{
		var plateLenght = tectonicPlateCount;

		for (int i = 0; i < plateLenght; i++)
		{
			var cellIndex = Random.Range(0, planet.cells.Length);

			planet.cells[cellIndex].plateIndex = i;
			planet.tectonicPlates[i].AddCell(cellIndex);
		}

		voronoiStepIndex = 0;
	}
	public void VoronoiProcessPlates()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			VoronoiFloodFillStep(i);
		}
	}
	public void VoronoiProcessPlatesStepByStep()
	{
		if (voronoiStepIndex < planet.cells.Length)
		{
			VoronoiFloodFillStep(voronoiStepIndex);
			voronoiStepIndex++;
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
