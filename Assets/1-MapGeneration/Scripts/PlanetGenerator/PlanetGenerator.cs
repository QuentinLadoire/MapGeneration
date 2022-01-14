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

	private int GetNearestPlateIndex(Cell cell)
	{
		var plates = planet.tectonicPlates;

		var nearestPlateIndex = -1;
		var sqrMagnitude = float.MaxValue;
		for (int i = 0; i < planet.tectonicPlates.Length; i++)
		{
			var plateCenter = plates[i].CellOrigin.position;
			var toTest = (cell.position - plateCenter).sqrMagnitude;
			if (toTest < sqrMagnitude)
			{
				nearestPlateIndex = i;
				sqrMagnitude = toTest;
			}
		}

		return nearestPlateIndex;
	}
	private void AddCellToPlate(int cellIndex, int plateIndex)
	{
		var cell = planet.cells[cellIndex];
		var plate = planet.tectonicPlates[plateIndex];

		cell.plateIndex = plateIndex;
		plate.AddCell(cellIndex);


	}

	public void InitializePlanet()
	{
		DataStructureBuilder.CreateDualMeshData(MeshGenerator.CreateIcoSphere(planetRadius, planetRefiningStep), out DualHalfEdgeData dualMeshData);

		planet = new Planet(dualMeshData.polygonData);
		planet.radius = planetRadius;
		planet.angularVelocityMax = angularVelocityMax;

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
	public void AssignCellToPlates()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var cell = planet.cells[i];
			if (!cell.IsAssign)
			{
				var nearestPlateIndex = GetNearestPlateIndex(cell);

				AddCellToPlate(i, nearestPlateIndex);
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
		AssignCellToPlates();
		DeterminePlateBorder();

		GetComponent<PlanetRenderer>().SetPlanet(planet);
	}
}
