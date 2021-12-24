using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry.DataStructure
{
	public static class DataStructureBuilder
	{
		private class HalfEdgeDataBuilderFromMeshData
		{
			private HalfEdgeData data = null;

			private void Initialize(Vector3[] vertices, int[] triangles)
			{
				data = new HalfEdgeData
				{
					vertices = vertices,
					faces = new Face[triangles.Length / 3],
					halfEdges = new HalfEdge[triangles.Length],
					dualData = null
				};
			}
			private void ComputeHalfEdgesAndFacesFrom(int[] triangles)
			{
				int faceIndex = 0;
				for (int i = 0; i < triangles.Length; i += 3)
				{
					var halfEdgeIndex0 = i;
					var halfEdgeIndex1 = i + 1;
					var halfEdgeIndex2 = i + 2;

					var vertexIndex0 = triangles[i];
					var vertexIndex1 = triangles[i + 1];
					var vertexIndex2 = triangles[i + 2];

					data.halfEdges[halfEdgeIndex0] = new HalfEdge(vertexIndex0, halfEdgeIndex1, halfEdgeIndex2, -1, faceIndex, data);
					data.halfEdges[halfEdgeIndex1] = new HalfEdge(vertexIndex1, halfEdgeIndex2, halfEdgeIndex0, -1, faceIndex, data);
					data.halfEdges[halfEdgeIndex2] = new HalfEdge(vertexIndex2, halfEdgeIndex0, halfEdgeIndex1, -1, faceIndex, data);

					data.faces[faceIndex] = new Face(halfEdgeIndex0, halfEdgeIndex2, data);

					faceIndex++;
				}

				var processList = new List<int>(triangles.Length);
				for (int i = 0; i < triangles.Length; i++)
					processList.Add(i);

				while (processList.Count > 0)
				{
					var current = processList[0];
					for (int i = 1; i < processList.Count; i++)
					{
						var toTest = processList[i];

						if (data.halfEdges[current].vertexIndex == data.halfEdges[toTest].Next.vertexIndex &&
							data.halfEdges[current].Next.vertexIndex == data.halfEdges[toTest].vertexIndex)
						{
							data.halfEdges[current].oppositeIndex = toTest;
							data.halfEdges[toTest].oppositeIndex = current;

							processList.RemoveAt(i);

							break;
						}
					}

					processList.RemoveAt(0);
				}
			}
			private void GenerateResult(out HalfEdgeData result)
			{
				result = data;
			}
			private void Clear()
			{
				data = null;
			}

			public bool CreateHalfEdgeData(MeshData data, out HalfEdgeData result)
			{
				if (data == null || data.triangles == null || data.triangles.Length < 3 || data.vertices == null)
				{
					result = null;
					return false;
				}

				Initialize(data.vertices, data.triangles);
				ComputeHalfEdgesAndFacesFrom(data.triangles);
				GenerateResult(out result);
				Clear();

				return true;
			}
		}
		private class HalfEdgeDataBuilderFromHalfEdgeData
		{
			private HalfEdgeData halfEdgeResult = null;

			private List<HalfEdge> halfEdges = new List<HalfEdge>();

			private void Initialize(HalfEdgeData data)
			{
				halfEdgeResult = new HalfEdgeData();

				halfEdgeResult.vertices = new Vector3[data.faces.Length];
				for (int i = 0; i < data.faces.Length; i++)
				{
					var face = data.faces[i];

					var p0 = face.First.Vertex;
					var p1 = face.First.Next.Vertex;
					var p2 = face.Last.Vertex;

					halfEdgeResult.vertices[i] = GeometryUtility.CalculateBarycenter(p0, p1, p2);
				}

				halfEdgeResult.faces = new Face[data.vertices.Length];
			}
			private void Compute(HalfEdgeData data)
			{
				var processList = new List<int>(data.halfEdges.Length);
				for (int i = 0; i < data.halfEdges.Length; i++)
					processList.Add(i);

				while (processList.Count > 0)
				{
					var processIndex = processList[0];
					var triangleHalfEdge = data.halfEdges[processIndex];

					int currentIndex = processIndex;
					int halfEdgeCount = 0;
					do
					{
						halfEdgeCount++;

						processList.Remove(currentIndex);

						currentIndex = triangleHalfEdge.Opposite.previousIndex;
						triangleHalfEdge = triangleHalfEdge.Opposite.Previous;
					}
					while (currentIndex != processIndex);

					var halfEdgesCount = halfEdges.Count;
					var faceIndex = triangleHalfEdge.Next.vertexIndex;
					halfEdgeResult.faces[faceIndex] = new Face(halfEdgesCount, halfEdgesCount + halfEdgeCount - 1, halfEdgeResult);

					for (int i = 0; i < halfEdgeCount; i++)
					{
						var vertexIndex = triangleHalfEdge.faceIndex;
						var nextIndex = (i != halfEdgeCount - 1) ? i + 1 : 0;
						var previousIndex = (i != 0) ? i - 1 : halfEdgeCount - 1;

						halfEdges.Add(new HalfEdge(vertexIndex, halfEdgesCount + nextIndex, halfEdgesCount + previousIndex, -1, faceIndex, this.halfEdgeResult));

						triangleHalfEdge = triangleHalfEdge.Opposite.Previous;
					}
				}
			}
			private void GenerateResult(out HalfEdgeData result)
			{
				halfEdgeResult.halfEdges = halfEdges.ToArray();
				
				result = halfEdgeResult;
			}
			private void Clear()
			{
				halfEdgeResult = null;
				halfEdges.Clear();
			}

			public bool CreateHalfEdgeData(HalfEdgeData data, out HalfEdgeData result)
			{
				if (data == null)
				{
					result = null;
					return false;
				}

				Initialize(data);
				Compute(data);
				GenerateResult(out result);
				Clear();

				return true;
			}
		}

		public static bool CreateHalfEdgeData(MeshData meshData, out HalfEdgeData result)
		{
			var halfEdgeDataBuilder = new HalfEdgeDataBuilderFromMeshData();
			return halfEdgeDataBuilder.CreateHalfEdgeData(meshData, out result);
		}
		public static bool CreateHalfEdgeData(HalfEdgeData data, out HalfEdgeData result)
		{
			var halfEdgeDataBuilder = new HalfEdgeDataBuilderFromHalfEdgeData();
			return halfEdgeDataBuilder.CreateHalfEdgeData(data, out result);
		}
		public static bool CreateDualMeshData(MeshData meshData, out DualMeshData dualMesh)
		{
			if (!CreateHalfEdgeData(meshData, out HalfEdgeData triangleMeshData) || !CreateHalfEdgeData(triangleMeshData, out HalfEdgeData polygonMeshData))
			{
				dualMesh = null;
				return false;
			}

			triangleMeshData.dualData = polygonMeshData;
			polygonMeshData.dualData = triangleMeshData;

			dualMesh = new DualMeshData
			{
				triangleData = triangleMeshData,
				polygonData = polygonMeshData
			};

			return true;
		}
	}
}
