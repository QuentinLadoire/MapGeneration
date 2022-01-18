using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry.DataStructure
{
	public static class DataStructureUtility
	{
		private static void DrawEdges(Edge[] edges, int offset, int count, Mesh mesh, Material material, Matrix4x4 matrix, float tickness)
		{
			var edgeMatrices = new List<Matrix4x4>(count);
			for (int j = 0; j < count; j++)
			{
				var p0 = edges[offset + j].FirstHalfEdge.Vertex;
				var p1 = edges[offset + j].FirstHalfEdge.Next.Vertex;

				var forward = p1 - p0;
				var upwards = -Vector3.Cross(forward, Vector3.Cross(forward, p0));

				var translation = (p0 + p1) * 0.5f + upwards.normalized * 0.0001f;
				var rotation = Quaternion.LookRotation(forward, upwards);
				var scale = new Vector3(0.005f, 1.0f, forward.magnitude);

				edgeMatrices.Add(matrix * Matrix4x4.TRS(translation, rotation, scale));
			}

			Graphics.DrawMeshInstanced(mesh, 0, material, edgeMatrices);
		}
		public static void DrawHalfEdgeData(HalfEdgeData data, Mesh mesh, Material material, Matrix4x4 matrix, float tickness = 0.005f)
		{
			var edges = data.edges;

			int drawCount = edges.Length / 1023;
			for (int i = 0; i < drawCount; i++)
				DrawEdges(edges, 1023 * i, 1023, mesh, material, matrix, tickness);

			var edgeCountLeft = edges.Length - 1023 * drawCount;
			DrawEdges(edges, 1023 * drawCount, edgeCountLeft, mesh, material, matrix, tickness);
		}
	}

	public static class DataStructureExtension
	{
		public delegate void CallBack();
		public delegate void HalfEdgeCallback(HalfEdge halfEdge, int index);

		public static void ForEachHalfEdge(this Face face, CallBack callback)
		{
			for (int i = 0; i < face.HalfEdgeCount; i++)
			{
				callback.Invoke();
			}
		}
		public static void ForEachHalfEdge(this Face face, HalfEdgeCallback callback)
		{
			for (int i = 0; i < face.HalfEdgeCount; i++)
			{
				callback.Invoke(face.GetHalfEdgeAt(i), i);
			}
		}
	}
}
