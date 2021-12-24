using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PlanetGenerator : MonoBehaviour
{
	[SerializeField] private Material planetMaterial = null;
	[SerializeField] private PlanetShapeSetting shapeSetting = new PlanetShapeSetting();

	[SerializeField] private bool renderMesh = false;
	[SerializeField] private bool renderTriangleData = false;
	[SerializeField] private bool renderPolygonData = false;

	private Mesh mesh = null;

	public void Generate()
	{
		shapeSetting.GenerateShape();

		mesh = new Mesh
		{
			vertices = shapeSetting.meshData.vertices,
			triangles = shapeSetting.meshData.triangles
		};
		mesh.RecalculateNormals();
	}

	private void Update()
	{
		if (mesh != null && planetMaterial != null && renderMesh)
			Graphics.DrawMesh(mesh, transform.localToWorldMatrix, planetMaterial, 0);
	}

	private static Material lineMaterial;
	private static void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			// Unity has a built-in shader that is useful for drawing
			// simple colored things.
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			lineMaterial = new Material(shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			// Turn on alpha blending
			lineMaterial.SetInteger("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineMaterial.SetInteger("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			// Turn backface culling off
			lineMaterial.SetInteger("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			// Turn off depth writes
			lineMaterial.SetInteger("_ZWrite", 0);
		}
	}
	private void OnRenderObject()
	{
		if (shapeSetting != null && shapeSetting.dualMeshData != null)
		{
			CreateLineMaterial();
			lineMaterial.SetPass(0);

			GL.PushMatrix();
			var matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale + new Vector3(0.002f, 0.002f, 0.002f));
			GL.MultMatrix(matrix);

			GL.Begin(GL.LINES);
			{
				if (renderTriangleData)
				{
					GL.Color(Color.black);
					foreach (var halfEdge in shapeSetting.dualMeshData.triangleData.halfEdges)
					{
						GL.Vertex(halfEdge.Vertex);
						GL.Vertex(halfEdge.Next.Vertex);
					}
				}

				if (renderPolygonData)
				{
					GL.Color(Color.red);
					foreach (var halfEdge in shapeSetting.dualMeshData.polygonData.halfEdges)
					{
						GL.Vertex(halfEdge.Vertex);
						GL.Vertex(halfEdge.Next.Vertex);
					}
				}
			}
			GL.End();
			GL.PopMatrix();
		}
	}
}
