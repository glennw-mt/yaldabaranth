using Friflo.Engine.ECS;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Yaldabaranth.Core.Scripts.ECS.Components;
using MonoGame.Extended;

namespace Yaldabaranth.Core.Scripts.Map;

public enum BiomeType { Plains, Forest, Deep, Shallows, Mountain, Snow }

public enum Environment { }

public class Region(YaldabaranthGame game)
{
  public readonly YaldabaranthGame game = game;
  public Dictionary<Vector2, Entity> EntityMap = [];
  public Dictionary<Vector2, Environment> EnvironmentMap = [];
}

public class Sector(YaldabaranthGame game, Vector2 regionSize)
{
  public readonly YaldabaranthGame game = game;
  public Dictionary<Vector2, Region> RegionMap = [];
  public Vector2 RegionSize = regionSize;
}

public class Biome(YaldabaranthGame game, Vector2 biomeSize)
{
  public readonly YaldabaranthGame game = game;
  public Dictionary<Vector2, Sector> SectorMap = [];
  public Vector2 BiomeSize = biomeSize;
}

public class Map(YaldabaranthGame game)
{
  public readonly YaldabaranthGame game = game;
  Texture2D mapTexture;
  public Dictionary<Vector2, Biome> BiomeMap = [];
  public Vector2 MapViewOffset = new(0, 0);
  public int MapViewZoom = 2;
  public Vector2 MapSize = new(512, 256);
  public Vector2 SectorSize = new(3, 3);
  public Vector2 RegionSize = new(128, 128);
  public C.Position PlayerPosition = new(game, Vector2.Zero);
  public void GenerateGlobe()
  {
    Random rnd = new();
    var seed = rnd.Next();
    var heightMap = Generation.GenerateHeightMap(seed, (int)MapSize.X, (int)MapSize.Y);
    var biomeTypes = Generation.PixelsToBiomes(heightMap, (int)MapSize.X, (int)MapSize.Y);
    mapTexture = new Texture2D(game.GraphicsDevice, (int)MapSize.X, (int)MapSize.Y);
    mapTexture.SetData(heightMap);
  }
  public void Blit()
  {
    if (mapTexture == null) return;
    Vector2 screenCenter = new(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
    Vector2 displaySize = MapSize * MapViewZoom;
    Vector2 mapTopLeft = screenCenter - (displaySize / 2) + (MapViewOffset * MapViewZoom);
    game.Canvas.Draw(mapTexture, mapTopLeft, null, Color.White, 0f, Vector2.Zero, MapViewZoom, SpriteEffects.None, 0f);
    Vector2 playerScreenPos = mapTopLeft + (PlayerPosition.G * MapViewZoom);
    game.Canvas.DrawRectangle(playerScreenPos, new Vector2(MapViewZoom, MapViewZoom), Color.Red, thickness: 2);
  }
}
