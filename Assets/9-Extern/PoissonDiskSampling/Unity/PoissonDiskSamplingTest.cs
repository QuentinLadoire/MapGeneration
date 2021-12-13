using UnityEngine;
using Miscellaneous;

namespace PoissonDisk.Unity
{
	public class PoissonDiskSamplingTest : MonoBehaviour
	{
		[SerializeField] private PoissonDiskSetting setting = new PoissonDiskSetting();

		Point2D[] points = null;
		PoissonDiskSampling poissonDiskSampling = null;
		
		private void Generate()
		{
			if (poissonDiskSampling == null)
				poissonDiskSampling = new PoissonDiskSampling();

			poissonDiskSampling.ComputePoints(setting, out points);

			GetComponent<MeshRenderer>().sharedMaterial.mainTexture = PoissonDiskUtility.GenerateTexture(points, setting.areaWidth, setting.areaHeight, 256);
		}

		private void OnValidate()
		{
			Generate();
		}
	}
}
