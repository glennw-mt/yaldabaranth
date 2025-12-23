using Godot;
using Friflo.Engine.ECS;
using SadRogue.Primitives.GridViews;
using GoRogue.FOV;
using SadRogue.Primitives;

namespace Yaldabaranth.Scripts.ECS.Components;

public static class C
{
  public record struct Position(Vector2I V) : IComponent
  {
    public Position(int x, int y) : this(new Vector2I(x, y)) { }
  }
  public record struct Velocity(Vector2I V) : IComponent;
  public record struct Display(Tile T, Godot.Color C) : IComponent;
  public record struct Player : IComponent;
  public record struct Memory : IComponent;
  public record struct Eyes : IComponent
  {
    public ArrayView2D<bool> visMap = new(200, 200);
    public RecursiveShadowcastingFOV fov;
    public float eyeSight = 5.0f;
    public Eyes()
    {
      fov = new RecursiveShadowcastingFOV(visMap);
    }
    public readonly void UpdateFOV(Position pos)
    {
      var posCoord = new Point(pos.V.X + 100, pos.V.Y + 100);
      fov.Calculate(posCoord, radius: eyeSight);
    }
    public readonly void Reset()
    {
      for (int x = 0; x < 200; x++) for (int y = 0; y < 200; y++)
        visMap[new Point(x, y)] = true;
    }
  }
}

