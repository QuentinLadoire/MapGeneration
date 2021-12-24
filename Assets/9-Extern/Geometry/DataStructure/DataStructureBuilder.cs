using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry.DataStructure
{
	public static class DataStructureBuilder
	{
		private class HalfEdgeDataBuilderFromMeshData
		{
			private HalfEdgeData halfEdgeDataResult = null;

			private void Initialize(Vector3[] vertices, int[] triangles)
			{
				halfEdgeDataResult = new HalfEdgeData
				{
					vertices = vertices,
					faces = new Face[triangles.Length / 3],
					halfEdges = new HalfEdge[triangles.Length],
					edges = new Edge[triangles.Length / 2],
					dualData = null
				};
			}
			private void ComputeHalfEdges(int[] triangles)
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

					halfEdgeDataResult.halfEdges[halfEdgeIndex0] = new HalfEdge(vertexIndex0, halfEdgeIndex1, halfEdgeIndex2, -1, faceIndex, halfEdgeDataResult);
					halfEdgeDataResult.halfEdges[halfEdgeIndex1] = new HalfEdge(vertexIndex1, halfEdgeIndex2, halfEdgeIndex0, -1, faceIndex, halfEdgeDataResult);
					halfEdgeDataResult.halfEdges[halfEdgeIndex2] = new HalfEdge(vertexIndex2, halfEdgeIndex0, halfEdgeIndex1, -1, faceIndex, halfEdgeDataResult);

					halfEdgeDataResult.faces[faceIndex] = new Face(halfEdgeIndex0, halfEdgeIndex2, halfEdgeDataResult);

					faceIndex++;
				}
			}
			private void ComputeEdges()
			{
				var processList = new List<int>(halfEdgeDataResult.halfEdges.Length);
				for (int i = 0; i < halfEdgeDataResult.halfEdges.Length; i++)
					processList.Add(i);

				int edgeIndex = 0;
				while (processList.Count > 0)
				{
					var current = processList[0];

					halfEdgeDataResult.edges[edgeIndex].parentData = halfEdgeDataResult;
					halfEdgeDataResult.edges[edgeIndex].firstHalfEdge = current;

					for (int i = 1; i < processList.Count; i++)
					{
						var toTest = processList[i];

						if (halfEdgeDataResult.halfEdges[current].vertexIndex == halfEdgeDataResult.halfEdges[toTest].Next.vertexIndex &&
							halfEdgeDataResult.halfEdges[current].Next.vertexIndex == halfEdgeDataResult.halfEdges[toTest].vertexIndex)
						{
							halfEdgeDataResult.halfEdges[current].oppositeIndex = toTest;
							halfEdgeDataResult.halfEdges[toTest].oppositeIndex = current;

							halfEdgeDataResult.edges[edgeIndex].secondHalfEdge = toTest;

							processList.RemoveAt(i);

							break;
						}
					}

					edgeIndex++;

					processList.RemoveAt(0);
				}
			}
			private void GenerateResult(out HalfEdgeData result)
			{
				result = halfEdgeDataResult;
			}
			private void Clear()
			{
				halfEdgeDataResult = null;
			}

			public bool CreateHalfEdgeData(MeshData data, out HalfEdgeData result)
			{
				if (data == null || data.triangles == null || data.triangles.Length < 3 || data.vertices == null)
				{
					result = null;
					return false;
				}

				Initialize(data.vertices, data.triangles);
				ComputeHalfEdges(data.triangles);
				ComputeEdges();
				GenerateResult(out result);
				Clear();

				return true;
			}
		}
		private class HalfEdgeDataBuilderFromHalfEdgeData
		{
			private HalfEdgeData halfEdgeDataResult = null;

			private int GetFaceEdgeCount(int startIndex, HalfEdge startHalfEdge)
			{
				var triangleHalfEdge = startHalfEdge;

				int currentIndex;
				int edgeCount = 0;
				do
				{
					edgeCount++;

					currentIndex = triangleHalfEdge.Opposite.previousIndex;
					triangleHalfEdge = triangleHalfEdge.Opposite.Previous;
				}
				while (currentIndex != startIndex);

				return edgeCount;
			}

			private void Initialize(HalfEdgeData data)
			{
				halfEdgeDataResult = new HalfEdgeData
				{
					vertices = new Vector3[data.faces.Length],
					faces = new Face[data.vertices.Length],
					halfEdges = new HalfEdge[data.halfEdges.Length],
					edges = new Edge[data.edges.Length],
					dualData = null
				};
			}
			private void FillVertices(Face[] faces)
			{
				for (int i = 0; i < faces.Length; i++)
				{
					var face = faces[i];

					var p0 = face.First.Vertex;
					var p1 = face.First.Next.Vertex;
					var p2 = face.Last.Vertex;

					var vertex = GeometryUtility.CalculateBarycenter(p0, p1, p2);
					vertex = GeometryUtility.ProjectOnSphere(vertex, p0.magnitude);

					halfEdgeDataResult.vertices[i] = vertex;
				}
			}
			private void ComputeHalfEdges(HalfEdge[] triangleHalfEdges)
			{
				var processList = new List<int>(triangleHalfEdges.Length);
				for (int i = 0; i < triangleHalfEdges.Length; i++)
					processList.Add(i);

				var halfEdgesCount = 0;
				while (processList.Count > 0)
				{
					var processIndex = processList[0];
					var triangleHalfEdge = triangleHalfEdges[processIndex];

					int faceEdgeCount = GetFaceEdgeCount(processIndex, triangleHalfEdge);

					var faceIndex = triangleHalfEdge.Next.vertexIndex;
					halfEdgeDataResult.faces[faceIndex] = new Face(halfEdgesCount, halfEdgesCount + faceEdgeCount - 1, halfEdgeDataResult);

					for (int i = 0; i < faceEdgeCount; i++)
					{
						var vertexIndex = triangleHalfEdge.faceIndex;
						var nextIndex = (i != faceEdgeCount - 1) ? i + 1 : 0;
						var previousIndex = (i != 0) ? i - 1 : faceEdgeCount - 1;

						halfEdgeDataResult.halfEdges[halfEdgesCount + i] = new HalfEdge(vertexIndex, halfEdgesCount + nextIndex, halfEdgesCount + previousIndex, -1, faceIndex, halfEdgeDataResult);
						
						processList.Remove(processIndex);

						processIndex = triangleHalfEdge.Opposite.previousIndex;
						triangleHalfEdge = triangleHalfEdge.Opposite.Previous;
					}

					halfEdgesCount += faceEdgeCount;
				}
			}
			private void ComputeEdges()
			{
				var processList = new List<int>(halfEdgeDataResult.halfEdges.Length);
				for (int i = 0; i < halfEdgeDataResult.halfEdges.Length; i++)
					processList.Add(i);

				int edgeIndex = 0;
				while (processList.Count > 0)
				{
					var current = processList[0];

					halfEdgeDataResult.edges[edgeIndex].parentData = halfEdgeDataResult;
					halfEdgeDataResult.edges[edgeIndex].firstHalfEdge = current;

					for (int i = 1; i < processList.Count; i++)
					{
						var toTest = processList[i];

						if (halfEdgeDataResult.halfEdges[current].vertexIndex == halfEdgeDataResult.halfEdges[toTest].Next.vertexIndex &&
							halfEdgeDataResult.halfEdges[current].Next.vertexIndex == halfEdgeDataResult.halfEdges[toTest].vertexIndex)
						{
							halfEdgeDataResult.halfEdges[current].oppositeIndex = toTest;
							halfEdgeDataResult.halfEdges[toTest].oppositeIndex = current;

							halfEdgeDataResult.edges[edgeIndex].secondHalfEdge = toTest;

							processList.RemoveAt(i);

							break;
						}
					}

					edgeIndex++;

					processList.RemoveAt(0);
				}
			}
			private void Compute(HalfEdgeData triangleData)
			{
				ComputeHalfEdges(triangleData.halfEdges);
				ComputeEdges();
			}
			private void GenerateResult(out HalfEdgeData result)
			{
				result = halfEdgeDataResult;
			}
			private void Clear()
			{
				halfEdgeDataResult = null;
			}

			public bool CreateHalfEdgeData(HalfEdgeData data, out HalfEdgeData result)
			{
				if (data == null)
				{
					result = null;
					return false;
				}

				Initialize(data);
				FillVertices(data.faces);
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
