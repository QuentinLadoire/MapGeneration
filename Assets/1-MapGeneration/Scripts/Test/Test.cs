using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
	FastNoiseLite noise = new FastNoiseLite();

	MeshRenderer meshRenderer = null;

	private void Awake()
	{
		meshRenderer = GetComponent<MeshRenderer>();
	}
	private void Start()
	{
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

		var resolution = 1024;
		var texture = new Texture2D(resolution, resolution);
		for (int y = 0; y < resolution; y++)
		{
			for (int x = 0; x < resolution; x++)
			{
				var noiseValue = ((noise.GetNoise(x, y) + 1.0f) * 0.5f);
				texture.SetPixel(x, y, new Color(noiseValue, noiseValue, noiseValue));
			}
		}
		texture.Apply();

		meshRenderer.material.mainTexture = texture;

		
	}
}
