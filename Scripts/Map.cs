using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapGeneration.Steps;
using GoRogue.MapGeneration.Steps.Translation;
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

public class Region : IGridView<Entity>
{
  private readonly ArrayView2D<Entity> entities = new(width: 128, height: 128);
  public Entity this[int x, int y] => entities[x, y];
  public Entity this[Point pos] => this[pos];
  public Entity this[int index1D] => this[index1D];
  int IGridView<Entity>.Height => 128;
  int IGridView<Entity>.Width => 128;
  int IGridView<Entity>.Count => 128 * 128;
}

public class Sector : IGridView<Region>
{
  private readonly ArrayView2D<Region> regions = new(width: 3, height: 3);
  public Region this[int x, int y] => regions[x, y];
  public Region this[Point pos] => this[pos];
  public Region this[int index1D] => this[index1D];
  int IGridView<Region>.Height => 3;
  int IGridView<Region>.Width => 3;
  int IGridView<Region>.Count => 3 * 3;
}

public class Map(DatabaseManager db)
{
  readonly DatabaseManager db = db;
  public Dictionary<Vector2I, Biome> biomeMap;
  public void Generate()
  {
    var generator = new Generator(512, 128);
    generator.ConfigAndGenerateSafe(gen =>
    {
      gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps(maxRooms: 1000));
    });
    var wallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
    foreach (Point pos in wallFloorValues.Positions())
      if (wallFloorValues[pos]) Console.WriteLine($"{pos} is a floor.");
      else Console.WriteLine($"{pos} is a wall.");
    biomeMap = [];
    for (int x = 0; x < 512; x++) for (int y = 0; y < 128; y++)
    {
      var biomes = Enum.GetValues<Biome>();
      var pos = new Vector2I(x, y);
      biomeMap[pos] = wallFloorValues[new Point(pos.X, pos.Y)] ? Biome.Desert : Biome.Ocean;
    }
  }
  public void DebugBlit(Node2D canvas)
  {
    for (int x = 0; x < 512; x++) for (int y = 0; y < 128; y++)
    {
      Godot.Color map_pixel = new();
      var pos = new Vector2I(x, y);
      switch (biomeMap[pos])
      {
        case Biome.Forest:
          map_pixel = Godot.Color.Color8(0, 255, 0);
          break;
        case Biome.Desert:
          map_pixel = Godot.Color.Color8(255, 255, 100);
          break;
        case Biome.Ocean:
          map_pixel = Godot.Color.Color8(0, 0, 255);
          break;
        case Biome.Mountain:
          map_pixel = Godot.Color.Color8(255, 255, 255);
          break;
      }
      canvas.DrawRect(new Rect2(pos * 2, new Vector2(2, 2)), map_pixel);
    }
  }
}
