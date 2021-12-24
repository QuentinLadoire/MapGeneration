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

			this.parentData = parentData;
		}
	}

	public struct Face
	{
		public int firstIndex;
		public int lastIndex;

		public HalfEdgeData parentData;

		public HalfEdge First => parentData.halfEdges[firstIndex];
		public HalfEdge Last => parentData.halfEdges[lastIndex];

		public Face(int firstIndex, int lastIndex, HalfEdgeData parentData)
		{
			this.firstIndex = firstIndex;
			this.lastIndex = lastIndex;

			this.parentData = parentData;
		}
	}

	public class HalfEdgeData
	{
		public Vector3[] vertices = null;
		public HalfEdge[] halfEdges = null;
		public Face[] faces = null;

		public HalfEdgeData dualData = null;
	}

	public class DualMeshData
	{
		public HalfEdgeData triangleData = null;
		public HalfEdgeData polygonData = null;
	}
}
