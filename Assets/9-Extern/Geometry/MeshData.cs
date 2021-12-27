using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
	public class MeshData
	{
		private List<Color> colors = null;
		private List<int> triangles = null;
		private List<Vector3> vertices = null;

		public int VerticesCount => vertices.Count;
		public int TrianglesCount => triangles.Count;

		public Color[] Colors => colors.ToArray();
		public int[] Triangles => triangles.ToArray();
		public Vector3[] Vertices => vertices.ToArray();

		public MeshData()
		{
			colors = new List<Color>();
			triangles = new List<int>();
			vertices = new List<Vector3>();
		}
		public MeshData(Vector3[] vertices, int[] triangles)
		{
			this.colors = new List<Color>();
			this.triangles = new List<int>(triangles);
			this.vertices = new List<Vector3>(vertices);
		}
		public MeshData(Vector3[] vertices, int[] triangles, Color[] colors)
		{
			this.colors = new List<Color>(colors);
			this.triangles = new List<int>(triangles);
			this.vertices = new List<Vector3>(vertices);
		}
		public MeshData(List<Vector3> vertices, List<int> triangles)
		{
			this.vertices = vertices;
			this.triangles = triangles;
			colors = new List<Color>();
		}
		public MeshData(List<Vector3> vertices, List<int> triangles, List<Color> colors)
		{
			this.colors = colors;
			this.vertices = vertices;
			this.triangles = triangles;
		}

		public void SetColors(Color[] colors)
		{
			this.colors.Clear();
			this.colors.AddRange(colors);
		}
		public void SetColors(List<Color> colors)
		{
			this.colors.Clear();
			this.colors.AddRange(colors);
		}
		public void SetTriangles(int[] triangles)
		{
			this.triangles.Clear();
			this.triangles.AddRange(triangles);
		}
		public void SetTriangles(List<int> triangles)
		{
			this.triangles.Clear();
			this.triangles.AddRange(triangles);
		}
		public void SetVertices(Vector3[] vertices)
		{
			this.vertices.Clear();
			this.vertices.AddRange(vertices);
		}
		public void SetVertices(List<Vector3> vertices)
		{
			this.vertices.Clear();
			this.vertices.AddRange(vertices);
		}

		public void AddColor(Color color)
		{
			colors.Add(color);
		}
		public void AddTriangle(int index0, int index1, int index2)
		{
			triangles.Add(index0);
			triangles.Add(index1);
			triangles.Add(index2);
		}
		public void AddVertex(Vector3 vertex)
		{
			vertices.Add(vertex);
		}

		public void ClearColors()
		{
			colors.Clear();
		}
		public void ClearTriangles()
		{
			triangles.Clear();
		}
		public void ClearVertices()
		{
			vertices.Clear();
		}
		public void Clear()
		{
			ClearVertices();
			ClearTriangles();
			ClearColors();
		}
	}
}
