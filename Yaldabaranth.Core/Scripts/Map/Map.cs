using Friflo.Engine.ECS;
using System.Collections.Generic;
using SadRogue.Primitives.GridViews;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Yaldabaranth.Core.Scripts.Map;

public enum Biome
{
  Forest, Ocean, Mountain
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
  public readonly YaldabaranthGame game = game;
  Texture2D mapTexture;
  public Dictionary<Vector2, Biome> BiomeMap;
  public Vector2 MapViewOffset = new(0, 0);
  public int MapViewZoom = 2;
  public Vector2 MapSize = new(512, 256);
  public Vector2 SectorSize = new(3, 3);
  public Vector2 RegionSize = new(128, 128);
  public void GenerateGlobe()
  {
    Random rnd = new();
    var seed = rnd.Next();
    var lowNoise = Generation.NoisePixels(seed, 0.002f, (int)MapSize.X, (int)MapSize.Y);
    var medNoise = Generation.NoisePixels(seed ^ 2, 0.02f, (int)MapSize.X, (int)MapSize.Y);
    var highNoise = Generation.NoisePixels(seed ^ 3, 0.2f, (int)MapSize.X, (int)MapSize.Y);
    var averageNoise = Generation.AveragePixels(lowNoise, medNoise, 1f, 0.25f);
    averageNoise = Generation.AveragePixels(averageNoise, highNoise, 1f, 0.05f);
    var biomePixels = Generation.PixelsToBiomes(averageNoise);
    mapTexture = new Texture2D(game.GraphicsDevice, (int)MapSize.X, (int)MapSize.Y);
    mapTexture.SetData(biomePixels);
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
