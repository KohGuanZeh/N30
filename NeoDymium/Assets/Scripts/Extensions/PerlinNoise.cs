using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
	public Renderer r;
	public int width = 256, height = 256;

	public float scale = 1; //Changes the Scale of the Generated Noise. //Bigger means more Perlin Noise

	public float xOffset, yOffset;

	private void Start()
	{
		r = GetComponent<Renderer>();
		xOffset = Random.Range(0, 1000);
		yOffset = Random.Range(0, 1000);
		r.material.mainTexture = GenerateTexture();
	}

	public void Update()
	{
		xOffset = Random.Range(0, 1000);
		yOffset = Random.Range(0, 1000);
		r.material.mainTexture = GenerateTexture();
	}

	Texture2D GenerateTexture()
	{
		Texture2D texture = new Texture2D(width, height);

		//Iterate through each pixel from width and height
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Color coordColor = CalculateColor(x, y);
				texture.SetPixel(x, y, coordColor);
			}
		}

		texture.Apply();
		return texture;
	}

	Color CalculateColor(int x, int y)
	{
		//Change to Perlin Coordinates instead of Pixel Coords
		float xCoord = (float)x / width * scale + xOffset;
		float yCoord = (float)y / height * scale + yOffset;

		float sample = Mathf.PerlinNoise(xCoord, yCoord);
		return new Color(sample, sample, sample);
	}
}
