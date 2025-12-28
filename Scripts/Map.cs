using GoRogue.MapGeneration;
using Friflo.Engine.ECS;
using System.Collections.Generic;
using System;
using SadRogue.Primitives.GridViews;
using SadRogue.Primitives;
using Godot;

namespace Yaldabaranth.Scripts;

public enum Biome
{
  Forest, Desert, Ocean, Mountain
}

public class Region(Vector2I size) : IGridView<Entity>
{
  private readonly ArrayView2D<Entity> entities = new(width: size.X, height: size.Y);
  public Entity this[int x, int y] => entities[x, y];
  public Entity this[Point pos] => this[pos];
  public Entity this[int index1D] => this[index1D];
  int IGridView<Entity>.Height => size.Y;
  int IGridView<Entity>.Width => size.X;
  int IGridView<Entity>.Count => size.X * size.Y;
}

public class Sector(Vector2I size) : IGridView<Region>
{
  private readonly ArrayView2D<Region> regions = new(width: 3, height: 3);
  public Region this[int x, int y] => regions[x, y];
  public Region this[Point pos] => this[pos];
  public Region this[int index1D] => this[index1D];
  int IGridView<Region>.Height => size.Y;
  int IGridView<Region>.Width => size.X;
  int IGridView<Region>.Count => size.X * size.Y;
}

public class Map(World world)
{
  readonly World world = world;
  public Dictionary<Vector2I, Biome> biomeMap;
  Vector2I mapViewOffset = new(0, 0);
  public Vector2I mapSize = new(512, 256);
  public Vector2I sectorSize = new(3, 3);
  public Vector2I regionSize = new(128, 128);
  public void GenerateGlobe()
  {
    var generator = new Generator(mapSize.X, mapSize.Y);
    generator.ConfigAndGenerateSafe(gen =>
    {
      gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps(maxRooms: 500));
    });
    var wallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
    foreach (Point pos in wallFloorValues.Positions())
      if (wallFloorValues[pos]) Console.WriteLine($"{pos} is a floor.");
      else Console.WriteLine($"{pos} is a wall.");
    biomeMap = [];
    for (int x = 0; x < mapSize.X; x++) for (int y = 0; y < mapSize.Y; y++)
    {
      var biomes = Enum.GetValues<Biome>();
      var pos = new Vector2I(x, y);
      biomeMap[pos] = wallFloorValues[new Point(pos.X, pos.Y)] ? Biome.Ocean : Biome.Desert;
    }
  }
  public void DebugBlit(Node2D canvas)
  {
    for (int x = 0; x < mapSize.X; x++) for (int y = 0; y < mapSize.Y; y++)
    {
      Godot.Color map_pixel = new();
      var pos = new Vector2I(x, y);
      switch (biomeMap[pos])
      {
        case Biome.Forest: map_pixel = Godot.Color.Color8(0, 255, 0); break;
        case Biome.Desert: map_pixel = Godot.Color.Color8(255, 255, 100); break;
        case Biome.Ocean: map_pixel = Godot.Color.Color8(0, 0, 255); break;
        case Biome.Mountain: map_pixel = Godot.Color.Color8(255, 255, 255); break;
      }
      canvas.DrawRect(new Rect2(pos - mapSize / 2 + mapViewOffset, new Vector2(1, 1)), map_pixel);
    }
  }
}
