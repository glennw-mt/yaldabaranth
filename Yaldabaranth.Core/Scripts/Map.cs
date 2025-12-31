using GoRogue.MapGeneration;
using Friflo.Engine.ECS;
using System.Collections.Generic;
using System;
using SadRogue.Primitives.GridViews;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Graphics;

namespace Yaldabaranth.Core.Scripts;

public enum Biome
{
  Forest, Desert, Ocean, Mountain
}

public class Region(Vector2 size) : IGridView<Entity>
{
  private readonly ArrayView2D<Entity> entities = new(width: (int)size.X, height: (int)size.Y);
  public Entity this[int x, int y] => entities[x, y];
  public Entity this[SadRogue.Primitives.Point pos] => this[pos];
  public Entity this[int index1D] => this[index1D];
  int IGridView<Entity>.Height => (int)size.Y;
  int IGridView<Entity>.Width => (int)size.X;
  int IGridView<Entity>.Count => (int)(size.X * size.Y);
}

public class Sector(Vector2 size) : IGridView<Region>
{
  private readonly ArrayView2D<Region> regions = new(width: 3, height: 3);
  public Region this[int x, int y] => regions[x, y];
  public Region this[SadRogue.Primitives.Point pos] => this[pos];
  public Region this[int index1D] => this[index1D];
  int IGridView<Region>.Height => (int)size.Y;
  int IGridView<Region>.Width => (int)size.X;
  int IGridView<Region>.Count => (int)(size.X * size.Y);
}

public class Map(YaldabaranthGame game)
{
  readonly YaldabaranthGame game = game;
  Texture2D mapTexture;
  public Dictionary<Vector2, Biome> BiomeMap;
  public Vector2 MapViewOffset = new(0, 0);
  public int MapViewZoom = 1;
  public Vector2 MapSize = new(512, 256);
  public Vector2 SectorSize = new(3, 3);
  public Vector2 RegionSize = new(128, 128);
  public void GenerateGlobe()
  {
    var generator = new Generator((int)MapSize.X, (int)MapSize.Y);
    generator.ConfigAndGenerateSafe(gen =>
    {
      gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps(maxRooms: 500));
    });
    var wallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
    foreach (SadRogue.Primitives.Point pos in wallFloorValues.Positions())
      if (wallFloorValues[pos]) Console.WriteLine($"{pos} is a floor.");
      else Console.WriteLine($"{pos} is a wall.");
    BiomeMap = [];
    for (int x = 0; x < MapSize.X; x++) for (int y = 0; y < MapSize.Y; y++)
    {
      var biomes = Enum.GetValues<Biome>();
      var pos = new Vector2(x, y);
      BiomeMap[pos] = wallFloorValues[new SadRogue.Primitives.Point((int)pos.X, (int)pos.Y)] ? Biome.Ocean : Biome.Desert;
    }
    mapTexture = new Texture2D(game.GraphicsDevice, (int)MapSize.X, (int)MapSize.Y);
    Color[] colorData = new Color[(int)(MapSize.X * MapSize.Y)];
    for (int i = 0; i < colorData.Length; i++)
    {
      int x = i % (int)MapSize.X;
      int y = i / (int)MapSize.X;
      var biome = BiomeMap[new Vector2(x, y)];
      colorData[i] = biome switch
      {
        Biome.Forest => Color.Green,
        Biome.Desert => Color.Yellow,
        Biome.Ocean => Color.Blue,
        Biome.Mountain => Color.White,
        _ => Color.Black
      };
    }
    mapTexture.SetData(colorData);
  }
  public void DebugBlit()
  {
    if (mapTexture == null) return;
    Vector2 screenCenter = new(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
    Vector2 displaySize = MapSize * MapViewZoom;
    Vector2 drawPos = screenCenter - (displaySize / 2) + MapViewOffset;
    game.Canvas.Draw(mapTexture, drawPos, null, Color.White, 0f, Vector2.Zero, MapViewZoom, SpriteEffects.None, 0f);
  }
}
