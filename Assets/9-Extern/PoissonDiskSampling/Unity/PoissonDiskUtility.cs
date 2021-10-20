using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoissonDisk.Unity
{
	public static class PoissonDiskUtility
	{
		public static Texture2D GenerateTexture(Point2D[] points, float areaWidth, float areaHeight, int resolution)
		{
			var texture = new Texture2D(resolution, resolution);
			texture.filterMode = FilterMode.Point;
			foreach (var point in points)
				texture.SetPixel((int)((resolution - 1) * (point.x / areaWidth)), (int)((resolution - 1) * (point.y / areaHeight)), Color.red);

			texture.Apply();

			return texture;
		}
	}
}
