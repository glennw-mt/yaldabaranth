using Friflo.Engine.ECS;
using SadRogue.Primitives.GridViews;
using GoRogue.FOV;
using Microsoft.Xna.Framework;
using System;

namespace Yaldabaranth.Core.Scripts.ECS.Components;

public static class C
{
  public record struct Position(YaldabaranthGame Game, Vector2 A) : IComponent
  {
    public YaldabaranthGame Game = Game;
    public Vector2 A = A;
    public readonly Vector2 G
    {
      get
      {
        var G = A / (Game.Map.SectorSize * Game.Map.RegionSize);
        G.Floor();
        return G;
      }
    }
    public readonly Vector2 S
    {
      get
      {
        var S = A / Game.Map.RegionSize;
        S.X %= Game.Map.SectorSize.X;
        S.Y %= Game.Map.SectorSize.Y;
        S.Floor();
        return S;
      }
    }
    public readonly Vector2 R
    {
      get
      {
        var R = A;
        R.X %= Game.Map.RegionSize.X;
        R.Y %= Game.Map.RegionSize.Y;
        R.Floor();
        return R;
      }
    }

    public Position(
      YaldabaranthGame game,
      Vector2 g, Vector2 s, Vector2 r
    ) : this(game, (g * game.Map.SectorSize * game.Map.RegionSize) + (s * game.Map.RegionSize) + r) { }
    public void Move(Vector2 absOffset) => A += absOffset;
  }
  public record struct Velocity(Vector2 V) : IComponent;
  public record struct Display(Tile T, Color C) : IComponent;
  public record struct Player : IComponent;
  public record struct Memory : IComponent;
  public record struct Eyes : IComponent
  {
    readonly YaldabaranthGame game;
    public ArrayView2D<bool> VisMap;
    public RecursiveShadowcastingFOV Fov;
    public float EyeSight = 5.0f;
    public Eyes(YaldabaranthGame game)
    {
      this.game = game;
      VisMap = new((int)game.Map.RegionSize.X * 3, (int)game.Map.RegionSize.Y * 3);
      Fov = new RecursiveShadowcastingFOV(VisMap);
    }
    public readonly void UpdateFOV(Position pos)
    {
      var posCoord = new SadRogue.Primitives.Point((int)pos.R.X, (int)pos.R.Y);
      Fov.Calculate(posCoord, radius: EyeSight);
    }
    public readonly void Reset()
    {
      for (int x = 0; x < game.Map.RegionSize.X * 3; x++)
        for (int y = 0; y < game.Map.RegionSize.Y * 3; y++) VisMap[new SadRogue.Primitives.Point(x, y)] = true;
    }
  }
  public record struct Body : IComponent;
}
