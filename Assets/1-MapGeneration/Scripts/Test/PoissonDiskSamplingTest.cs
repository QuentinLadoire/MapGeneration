using UnityEngine;

namespace PoissonDisk.Unity
{
	public class PoissonDiskSamplingTest : MonoBehaviour
	{
		[Header("PoissonDiskSampling Settings")]
		[SerializeField] private float radius = 1.0f;
		[SerializeField] private float areaWidth = 30.0f;
		[SerializeField] private float areaHeight = 30.0f;
		[SerializeField] private int sampleLimitBeforeRejection = 30;
		[SerializeField] private int seed = 0;
		[SerializeField] private StartPointPickMode startPointPickMode = StartPointPickMode.Random;
		[SerializeField] private Vector2 customStartPoint = Vector2.zero;

		Point2D[] points = null;
		PoissonDiskSampling poissonDiskSampling = null;
		
		private void GenerateTexture()
		{
			var texture = new Texture2D(256, 256);
			texture.filterMode = FilterMode.Point;
			foreach (var point in points)
				texture.SetPixel((int)((256 - 1) * (1 - (point.x / areaWidth))), (int)((256 - 1) * (1 - (point.y / areaHeight))), Color.red);

			texture.Apply();

			GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
		}
		private void Generate()
		{
			if (poissonDiskSampling == null)
				poissonDiskSampling = new PoissonDiskSampling();

			poissonDiskSampling.Radius = radius;
			poissonDiskSampling.AreaWidth = areaWidth;
			poissonDiskSampling.AreaHeight = areaHeight;
			poissonDiskSampling.SampleLimitBeforeRejection = sampleLimitBeforeRejection;
			poissonDiskSampling.Seed = seed;
			poissonDiskSampling.StartPointPickMode = startPointPickMode;
			poissonDiskSampling.CustomStartPoint = new Point2D(customStartPoint.x, customStartPoint.y);

			poissonDiskSampling.GeneratePoints(ref points);
			GenerateTexture();
		}

		private void OnValidate()
		{
			Generate();
		}
	}
}
