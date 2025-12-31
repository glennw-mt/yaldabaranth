using Microsoft.Xna.Framework;

namespace Yaldabaranth.Core.Scripts.Map;

public static class Generation
{
  public static Color[] NoisePixels(int seed, float freq, int w, int h)
  {
    FastNoiseLite noiseGenerator = new(seed);
    noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    noiseGenerator.SetFrequency(freq);
    Color[] pixels = new Color[w * h];
    for (int x = 0; x < w; x++) for (int y = 0; y < h; y++)
    {
      float noise = noiseGenerator.GetNoise(x, y);
      noise = (noise + 1f) * 0.5f;
      float pixelValue = noise;
      pixels[y * w + x] = new(pixelValue, pixelValue, pixelValue, 1f);
    }
    return pixels;
  }
  public static Color[] AveragePixels(Color[] left, Color[] right, float weightLeft = 1f, float weightRight = 1f)
  {
    var result = new Color[left.Length];
    float totalWeight = weightLeft + weightRight;

    for (int i = 0; i < result.Length; i++)
    {
      float r = (left[i].R / 255f * weightLeft + right[i].R / 255f * weightRight) / totalWeight;
      float g = (left[i].G / 255f * weightLeft + right[i].G / 255f * weightRight) / totalWeight;
      float b = (left[i].B / 255f * weightLeft + right[i].B / 255f * weightRight) / totalWeight;

      result[i] = new Color(r, g, b, 1f);
    }

    return result;
  }
  public static Color[] PixelsToBiomes(Color[] pixels)
  {
    var result = new Color[pixels.Length];
    for (int i = 0; i < pixels.Length; i++)
    {
      if (pixels[i].R > 210) result[i] = Color.White * (pixels[i].R / 255f);
      else if (pixels[i].R > 175) result[i] = Color.DarkGreen * (pixels[i].R / 255f);
      else if (pixels[i].R > 130) result[i] = Color.Green * (pixels[i].R / 255f);
      else if (pixels[i].R > 125) result[i] = Color.Yellow * (pixels[i].R / 255f);
      else result[i] = Color.DarkBlue * (pixels[i].R / 255f);
      result[i].A = 255;
    }
    return result;
  }
}
