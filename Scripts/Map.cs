using GoRogue.MapViews;
using GoRogue;
using Friflo.Engine.ECS;
using System.Collections.Generic;
using Godot;
using System;

namespace Yaldabaranth.Scripts;

public enum Biome
{
  Forest, Desert, Ocean, Mountain
}

public class Region : IMapView<Entity>
{
  private readonly ArrayMap2D<Entity> entities = new(width: 128, height: 128);
  public Entity this[int x, int y] => entities[x, y];
  public Entity this[Coord pos] => this[pos];
  public Entity this[int index1D] => this[index1D];
  int IMapView<Entity>.Height => 128;
  int IMapView<Entity>.Width => 128;
}

public class Sector : IMapView<Region>
{
  private readonly ArrayMap2D<Region> regions = new(width: 3, height: 3);
  public Region this[int x, int y] => regions[x, y];
  public Region this[Coord pos] => this[pos];
  public Region this[int index1D] => this[index1D];
  int IMapView<Region>.Height => 3;
  int IMapView<Region>.Width => 3;
}

public class Map(DatabaseManager db)
{
  readonly DatabaseManager db = db;
  public Dictionary<Vector2I, Biome> biomeMap;
  public void Generate()
  {
    biomeMap = [];
    for (int x = 0; x < 512; x++)
    {
      for (int y = 0; y < 128; y++)
      {
        var biomes = Enum.GetValues<Biome>();
        var pos = new Vector2I(x, y);
        biomeMap[pos] = biomes[Random.Shared.Next(biomes.Length)];
      }
    }
  }
  public void DebugBlit(Node2D canvas)
  {
    for (int x = 0; x < 512; x++)
    {
      for (int y = 0; y < 128; y++)
      {
        Color map_pixel = new();
        var pos = new Vector2I(x, y);
        switch (biomeMap[pos])
        {
          case Biome.Forest:
            map_pixel = Color.Color8(0, 255, 0);
            break;
          case Biome.Desert:
            map_pixel = Color.Color8(255, 255, 100);
            break;
          case Biome.Ocean:
            map_pixel = Color.Color8(0, 0, 255);
            break;
          case Biome.Mountain:
            map_pixel = Color.Color8(255, 255, 255);
            break;
        }
        canvas.DrawRect(new Rect2(pos * 2, new Vector2(2, 2)), map_pixel);
      }
    }
  }
}
