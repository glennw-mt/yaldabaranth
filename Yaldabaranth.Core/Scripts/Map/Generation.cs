using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Yaldabaranth.Core.Scripts.Map;

public static class Generation
{
  public static Color[] GenerateNoise(int seed, float freq, int w, int h)
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
  public static Color[] RadialFalloff(Color[] pixels, int w, int h)
  {
    var result = new Color[w * h];
    var cx = w / 2f;
    var cy = h / 2f;
    for (int y = 0; y < h; y++)
    {
      float ny = (y - cy) / cy;
      for (int x = 0; x < w; x++)
      {
        float nx = (x - cx) / cy;
        float d = MathF.Sqrt(nx * nx * 0.25f + ny * ny);
        d = MathF.Min(d, 1f);
        float falloff = 1f - d * d;
        int i = y * w + x;
        var c = pixels[i];
        result[i] = new Color(
            (byte)(c.R * falloff),
            (byte)(c.G * falloff),
            (byte)(c.B * falloff),
            (byte)255
        );
      }
    }
    return result;
  }
  public static Color[] Normalize(Color[] pixels)
  {
    byte min = 255;
    byte max = 0;
    for (int i = 0; i < pixels.Length; i++)
    {
      byte v = pixels[i].R;
      if (v < min) min = v;
      if (v > max) max = v;
    }
    if (max == min) return pixels;
    float invRange = 1f / (max - min);
    var result = new Color[pixels.Length];
    for (int i = 0; i < pixels.Length; i++)
    {
      byte v = pixels[i].R;
      byte n = (byte)((v - min) * invRange * 255f);
      result[i] = new Color(n, n, n, pixels[i].A);
    }
    return result;
  }

  public static Dictionary<Vector2, BiomeType> PixelsToBiomes(Color[] pixels, int w, int h)
  {
    var result = new Dictionary<Vector2, BiomeType>();
    for (int x = 0; x < w; x++) for (int y = 0; y < h; y++)
    {
      var i = y * w + x;
      if (pixels[i].R > 210) result[new Vector2(x, y)] = BiomeType.Mountain;
      else if (pixels[i].R > 175) result[new Vector2(x, y)] = BiomeType.Forest;
      else if (pixels[i].R > 125) result[new Vector2(x, y)] = BiomeType.Plains;
      else if (pixels[i].R > 120) result[new Vector2(x, y)] = BiomeType.Plains;
      else if (pixels[i].R > 90) result[new Vector2(x, y)] = BiomeType.Shallows;
      else result[new Vector2(x, y)] = BiomeType.Deep;
    }
    return result;
  }
  public static Color[] GenerateHeightMap(int seed, int w, int h)
  {
    var lowNoise = GenerateNoise(seed, 0.01f, w, h);
    var medNoise = GenerateNoise(seed ^ 2, 0.05f, w, h);
    var highNoise = GenerateNoise(seed ^ 3, 0.2f, w, h);
    var averageNoise = AveragePixels(lowNoise, medNoise, 1f, 0.25f);
    averageNoise = AveragePixels(averageNoise, highNoise, 1f, 0.05f);
    averageNoise = RadialFalloff(averageNoise, w, h);
    return Normalize(averageNoise);
  }
}
