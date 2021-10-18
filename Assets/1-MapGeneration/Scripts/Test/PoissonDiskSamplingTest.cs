using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDiskSamplingTest : MonoBehaviour
{
	[Header("PoissonDiskSampling Settings")]
	[SerializeField] private float radius = 1.0f;
	[SerializeField] private float areaWidth = 30.0f;
	[SerializeField] private float areaHeight = 30.0f;
	[SerializeField] private int sampleLimitBeforeRejection = 30;
	[SerializeField] private int seed = 0;

	Point2D[] points = null;
	PoissonDiskSampling poissonDiskSampling = null;

	private void GenerateTexture()
	{
		var texture = new Texture2D(256, 256);
		texture.filterMode = FilterMode.Point;
		foreach (var point in points)
			texture.SetPixel((int)(256 * (point.x / areaWidth)), (int)(256 * (point.y / areaHeight)), Color.red);

		texture.Apply();

		GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
	}

	private void OnValidate()
	{
		if (poissonDiskSampling == null)
			poissonDiskSampling = new PoissonDiskSampling();

		poissonDiskSampling.Radius = radius;
		poissonDiskSampling.AreaWidth = areaWidth;
		poissonDiskSampling.AreaHeight = areaHeight;
		poissonDiskSampling.SampleLimitBeforeRejection = sampleLimitBeforeRejection;
		poissonDiskSampling.Seed = seed;

		poissonDiskSampling.GeneratePoints(ref points);
		GenerateTexture();
	}
}
