using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
	[System.Serializable]
	struct FastNoiseSetting
	{
		[Header("General Setting")]
		public FastNoiseLite.NoiseType noiseType;
		public FastNoiseLite.RotationType3D rotationType3D;
		public int seed;
		public float frequency;

		[Header("Fractal Setting")]
		public FastNoiseLite.FractalType fractalType;
		public int octaves;
		public float lacunarity;
		public float gain;
		public float weightedStrength;

		public static FastNoiseSetting Default = new FastNoiseSetting
		{
			noiseType = FastNoiseLite.NoiseType.OpenSimplex2,
			rotationType3D = FastNoiseLite.RotationType3D.None,
			seed = 0,
			frequency = 0.01f,

			fractalType = FastNoiseLite.FractalType.None,
			octaves = 3,
			lacunarity = 2.0f,
			gain = 0.5f,
			weightedStrength = 0.0f,
		};
	}

	[SerializeField] private int textureResolution = 256;
	[SerializeField] private FastNoiseSetting fastNoiseSetting = FastNoiseSetting.Default;

	[Header("NoiseGenerator Setting")]
	[SerializeField] private bool autoGenerate = false;

	private FastNoiseLite fastNoise = new FastNoiseLite();
	float[] noiseData = null;

	private void InitFastNoise()
	{
		fastNoise.SetNoiseType(fastNoiseSetting.noiseType);
		fastNoise.SetRotationType3D(fastNoiseSetting.rotationType3D);
		fastNoise.SetSeed(fastNoiseSetting.seed);
		fastNoise.SetFrequency(fastNoiseSetting.frequency);

		fastNoise.SetFractalType(fastNoiseSetting.fractalType);
		fastNoise.SetFractalOctaves(fastNoiseSetting.octaves);
		fastNoise.SetFractalLacunarity(fastNoiseSetting.lacunarity);
		fastNoise.SetFractalGain(fastNoiseSetting.gain);
		fastNoise.SetFractalWeightedStrength(fastNoiseSetting.weightedStrength);
	}

	public void Generate()
	{
		InitFastNoise();

		noiseData = new float[textureResolution * textureResolution];
		var colors = new Color[textureResolution * textureResolution];
		int index = 0;
		for (int y = 0; y < textureResolution; y++)
		{
			for (int x = 0; x < textureResolution; x++)
			{
				var noise = fastNoise.GetNoise(x, y);
				noiseData[index] = noise;

				var noiseScale = (noise + 1.0f) * 0.5f;
				colors[index] = new Color(noiseScale, noiseScale, noiseScale);

				index++;
			}
		}

		var texture = new Texture2D(textureResolution, textureResolution);
		texture.SetPixels(colors);
		texture.Apply();

		GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
	}

	private void OnValidate()
	{
		if (autoGenerate)
			Generate();
	}
}
