using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry.DataStructure
{
	public struct HalfEdge
	{
		public HalfEdgeDataStructure dataStructure;

		public int vertexIndex;

		public int nextIndex;
		public int previousIndex;
		public int oppositeIndex;

		public int faceIndex;

		public Vector3 Vertex => dataStructure.vertices[vertexIndex];

		public HalfEdge Next => dataStructure.halfEdges[nextIndex];
		public HalfEdge Previous => dataStructure.halfEdges[previousIndex];
		public HalfEdge Opposite => dataStructure.halfEdges[oppositeIndex];

		public Face Face => dataStructure.faces[faceIndex];
	}

	public struct Face
	{
		public HalfEdgeDataStructure dataStructure;

		public int halfEdgeIndex;

		public HalfEdge HalfEdge => dataStructure.halfEdges[halfEdgeIndex];
	}

	public class HalfEdgeDataStructure
	{
		public int[] triangles = null;
		public Vector3[] vertices = null;
		public HalfEdge[] halfEdges = null;
		public Face[] faces = null;
	}

	public static class HalfEdgeDataStructureBuilder
	{
		private static HalfEdgeDataStructure dataStructure = null;

		private static void InitializeDataStructure(MeshData meshData)
		{
			dataStructure = new HalfEdgeDataStructure();
			dataStructure.vertices = meshData.vertices;
			dataStructure.triangles = meshData.triangles;
		}

		private static void ComputeHalfEdgesAndFaces()
		{
			var triangles = dataStructure.triangles;

			var faceCount = triangles.Length / 3;
			dataStructure.faces = new Face[faceCount];

			var halfEdgeCount = triangles.Length;
			dataStructure.halfEdges = new HalfEdge[halfEdgeCount];

			int faceIndex = 0;
			for (int i = 0; i < triangles.Length; i += 3)
			{
				var halfEdgeIndex0 = i;
				var halfEdgeIndex1 = i + 1;
				var halfEdgeIndex2 = i + 2;

				var vertexIndex0 = triangles[i];
				var vertexIndex1 = triangles[i + 1];
				var vertexIndex2 = triangles[i + 2];

				var halfEdge0 = new HalfEdge
				{
					dataStructure = dataStructure,

					vertexIndex = vertexIndex0,

					nextIndex = halfEdgeIndex1,
					previousIndex = halfEdgeIndex2,
					oppositeIndex = -1,

					faceIndex = faceIndex
				};
				var halfEdge1 = new HalfEdge
				{
					dataStructure = dataStructure,

					vertexIndex = vertexIndex1,

					nextIndex = halfEdgeIndex2,
					previousIndex = halfEdgeIndex0,
					oppositeIndex = -1,

					faceIndex = faceIndex
				};
				var halfEdge2 = new HalfEdge
				{
					dataStructure = dataStructure,

					vertexIndex = vertexIndex2,

					nextIndex = halfEdgeIndex0,
					previousIndex = halfEdgeIndex1,
					oppositeIndex = -1,

					faceIndex = faceIndex
				};

				var face = new Face
				{
					dataStructure = dataStructure,

					halfEdgeIndex = halfEdgeIndex0
				};

				dataStructure.halfEdges[halfEdgeIndex0] = halfEdge0;
				dataStructure.halfEdges[halfEdgeIndex1] = halfEdge1;
				dataStructure.halfEdges[halfEdgeIndex2] = halfEdge2;

				dataStructure.faces[faceIndex] = face;

				faceIndex++;
			}
		}
		private static void LinkOppositeHalfEdge()
		{
			var halfEdges = dataStructure.halfEdges;
			var processList = new List<int>(dataStructure.triangles);

			while (processList.Count > 0)
			{
				var current = processList[0];

				var vertexIndex0 = halfEdges[current].vertexIndex;
				var vertexIndex1 = halfEdges[halfEdges[current].nextIndex].vertexIndex;

				for (int i = 1; i < processList.Count; i++)
				{
					var toTest = processList[i];

					var vertexIndex2 = halfEdges[toTest].vertexIndex;
					var vertexIndex3 = halfEdges[halfEdges[toTest].nextIndex].vertexIndex;

					if (vertexIndex0 == vertexIndex2 && vertexIndex1 == vertexIndex3)
					{
						halfEdges[current].oppositeIndex = toTest;
						halfEdges[toTest].oppositeIndex = current;

						processList.RemoveAt(i);

						break;
					}
				}

				processList.RemoveAt(0);
			}
		}
		private static void ComputeDataStructure()
		{
			ComputeHalfEdgesAndFaces();
			LinkOppositeHalfEdge();
		}

		public static bool CreateHalfEdgeDataStructure(MeshData meshData, out HalfEdgeDataStructure dataStructure)
		{
			if (meshData.vertices == null || meshData.vertices.Length == 0 || meshData.triangles == null || meshData.triangles.Length < 3)
			{
				dataStructure = null;
				return false;
			}

			InitializeDataStructure(meshData);
			ComputeDataStructure();

			dataStructure = HalfEdgeDataStructureBuilder.dataStructure;
			HalfEdgeDataStructureBuilder.dataStructure = null;

			return true;
		}
	}
}
