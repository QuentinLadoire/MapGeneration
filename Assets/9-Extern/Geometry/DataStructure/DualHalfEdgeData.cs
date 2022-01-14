using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry.DataStructure
{
	public struct HalfEdge
	{
		public int vertexIndex;

		public int nextIndex;
		public int previousIndex;
		public int oppositeIndex;

		public int faceIndex;

		public int edgeIndex;

		public HalfEdgeData parentData;

		public Vector3 Vertex => parentData.vertices[vertexIndex];

		public HalfEdge Next => parentData.halfEdges[nextIndex];
		public HalfEdge Previous => parentData.halfEdges[previousIndex];
		public HalfEdge Opposite => parentData.halfEdges[oppositeIndex];

		public Face Face => parentData.faces[faceIndex];

		public HalfEdge(int vertexIndex, int nextIndex, int previousIndex, int oppositeIndex, int faceIndex, HalfEdgeData parentData)
		{
			this.vertexIndex = vertexIndex;

			this.nextIndex = nextIndex;
			this.previousIndex = previousIndex;
			this.oppositeIndex = oppositeIndex;

			this.faceIndex = faceIndex;

			this.edgeIndex = -1;

			this.parentData = parentData;
		}
	}

	public struct Edge
	{
		public int firstHalfEdge;
		public int secondHalfEdge;

		public HalfEdgeData parentData;

		public HalfEdge FirstHalfEdge => parentData.halfEdges[firstHalfEdge];
		public HalfEdge SecondHalfEdge => parentData.halfEdges[secondHalfEdge];
	}

	public struct Face
	{
		public int firstIndex;
		public int lastIndex;

		public int[] halfEdgeIndexes;

		public HalfEdgeData parentData;

		public HalfEdge First => parentData.halfEdges[firstIndex];
		public HalfEdge Last => parentData.halfEdges[lastIndex];

		public int HalfEdgeCount => halfEdgeIndexes.Length;
		public HalfEdge GetHalfEdgeAt(int index)
		{
			return parentData.halfEdges[halfEdgeIndexes[index]];
		}

		public Face(int firstIndex, int lastIndex, int halfEdgeCount, HalfEdgeData parentData)
		{
			this.firstIndex = firstIndex;
			this.lastIndex = lastIndex;

			this.halfEdgeIndexes = new int[halfEdgeCount];

			this.parentData = parentData;
		}
	}

	public class HalfEdgeData
	{
		public Vector3[] vertices = null;
		public HalfEdge[] halfEdges = null;
		public Face[] faces = null;
		public Edge[] edges = null;

		public HalfEdgeData dualData = null;
	}

	public class DualHalfEdgeData
	{
		public HalfEdgeData triangleData = null;
		public HalfEdgeData polygonData = null;
	}
}
