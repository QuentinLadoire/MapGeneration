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
	[SerializeField] private int planetRefiningStep = 3;
	[SerializeField] private int tectonicPlateCount = 5;

	[SerializeField] private int powElevation = 10;
	[SerializeField] private float planetRadius = 1.0f;
	[SerializeField] private float elevationMax = 1.0f;
	[SerializeField] private float angularVelocityMax = 1.0f;
	[SerializeField][Range(0.0f, 1.0f)] private float oceanicRate = 0.7f;

	private Planet planet = null;

	private float GetConvergentElevation(float distance, int pow)
	{
		var inverse = 1 - distance;
		var elevation = (1 / inverse) * Mathf.Pow(inverse, pow);

		return elevation;
	}
	private float GetDivergentElevation(float distance, int pow)
	{
		var inverse = 1 - distance;
		var elevation = (1 / inverse) * Mathf.Pow(inverse, pow);

		return -elevation;
	}

	private bool IsOrogenicBelts(Boundary boundary)
	{
		var isConvergent = boundary.type == BoundaryType.Convergent;
		var isOrogenic = boundary.LeftCell.Plate.isOceanic == boundary.RightCell.Plate.isOceanic;

		return isConvergent && isOrogenic;
	}
	private bool IsSubductionZones(Boundary boundary)
	{
		var isConvergent = boundary.type == BoundaryType.Convergent;
		var isSubduction = boundary.LeftCell.Plate.isOceanic != boundary.RightCell.Plate.isOceanic;

		return isConvergent && isSubduction;
	}

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
	private void AddNewBoundary(int edgeIndex)
	{
		var boundary = new Boundary();
		boundary.parentPlanet = planet;
		boundary.edgeIndex = edgeIndex;

		boundary.parallelVector = (boundary.RightVertex - boundary.LeftVertex).normalized;
		boundary.perpendicularVector = Vector3.Cross(boundary.parallelVector, boundary.LeftVertex).normalized;

		var stress = boundary.RightCell.LinearVelocity - boundary.LeftCell.LinearVelocity;
		var parallelDot = Vector3.Dot(stress, boundary.parallelVector);
		var perpendicularDot = Vector3.Dot(stress, boundary.perpendicularVector);

		if (Mathf.Abs(parallelDot) > Mathf.Abs(perpendicularDot))
		{
			boundary.type = BoundaryType.Transform;
		}
		else if (perpendicularDot > 0.0f)
		{
			boundary.type = BoundaryType.Convergent;
		}
		else
		{
			boundary.type = BoundaryType.Divergent;
		}

		boundary.stress = stress.magnitude;

		if (planet.boundaryStressMax < boundary.stress)
			planet.boundaryStressMax = boundary.stress;

		planet.boundaries.Add(boundary);
	}
	private void AddCellToPlate(int cellIndex, int plateIndex)
	{
		var cell = planet.cells[cellIndex];
		var plate = planet.tectonicPlates[plateIndex];

		cell.plateIndex = plateIndex;
		plate.AddCell(cellIndex);

		cell.Face.ForEachHalfEdge((halfEdge, index) =>
		{
			var adjacentCell = planet.cells[halfEdge.Opposite.faceIndex];
			if (adjacentCell.IsAssign && adjacentCell.plateIndex != plateIndex)
			{
				AddNewBoundary(halfEdge.edgeIndex);

				cell.Plate.AddBoundary(planet.boundaries.Count - 1);
				adjacentCell.Plate.AddBoundary(planet.boundaries.Count - 1);
			}
		});
	}

	private void InitializePlanet()
	{
		DataStructureBuilder.CreateDualMeshData(MeshGenerator.CreateIcoSphere(planetRadius, planetRefiningStep), out DualHalfEdgeData dualMeshData);

		planet = new Planet(dualMeshData.polygonData);
		planet.radius = planetRadius;
		planet.angularVelocityMax = angularVelocityMax;

		Random.InitState(seed);
	}
	private void InitializePlates()
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

			AddCellToPlate(cellIndex, i);
		}
	}
	private void AssignCellToPlates()
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
	private void DeterminePlatesCenter()
	{
		for (int i = 0; i < planet.tectonicPlates.Length; i++)
		{
			var plate = planet.tectonicPlates[i];
			for (int j = 0; j < plate.CellCount; j++)
			{
				var cell = plate.GetCellAt(j);
				plate.center += cell.position;
			}
			plate.center /= plate.CellCount;

			plate.center = Geometry.GeometryUtility.ProjectOnSphere(plate.center, planetRadius);
		}
	}
	private void AssignCellsElevation()
	{
		for (int i = 0; i < planet.cells.Length; i++)
		{
			var count = 0;
			var elevationAverage = 0.0f;

			var cell = planet.cells[i];
			for (int j = 0; j < cell.Plate.BoundaryCount; j++)
			{
				var boundary = cell.Plate.GetBoundayAt(j);
				if (boundary.type == BoundaryType.Transform)
					continue;

				var cellToCenterDistance = (cell.position - cell.Plate.center).sqrMagnitude;
				var cellToBoundaryDistance = (cell.position - boundary.MidPoint).sqrMagnitude;
				var boundaryToCenterDistance = (boundary.MidPoint - cell.Plate.center).sqrMagnitude;

				if (cellToBoundaryDistance > boundaryToCenterDistance)
					continue;

				var ratio = cellToBoundaryDistance / (cellToCenterDistance + cellToBoundaryDistance);
				if (IsSubductionZones(boundary) && !cell.Plate.isOceanic)
				{
					elevationAverage += boundary.StressInPercent * GetConvergentElevation(ratio, powElevation);
					count++;
				}
				else if (IsOrogenicBelts(boundary))
				{
					elevationAverage += boundary.StressInPercent * GetConvergentElevation(ratio, powElevation);
					count++;
				}
				else if (boundary.type == BoundaryType.Divergent)
				{
					elevationAverage += boundary.StressInPercent * GetDivergentElevation(ratio, powElevation);
					count++;
				}
			}

			elevationAverage /= count;

			cell.elevation = elevationAverage;
		}
	}
	private void DeterminePlateBorder()
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
		DeterminePlatesCenter();
		AssignCellsElevation();
		DeterminePlateBorder();

		GetComponent<PlanetRenderer>().SetPlanet(planet);
	}
}
