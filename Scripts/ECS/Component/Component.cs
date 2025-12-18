using Godot;
using Friflo.Engine.ECS;
using GoRogue.MapViews;
using GoRogue;

namespace Yaldabaranth.Scripts.ECS.Component;

public static class C
{
  public record struct Position(Vector2I V) : IComponent
  {
    public Position(int x, int y) : this(new Vector2I(x, y)) { }
  }
  public record struct Velocity(Vector2I V) : IComponent;
  public record struct Display(Tile T, Color C) : IComponent;
  public record struct Player : IComponent;
  public record struct Eyes : IComponent
  {
    public ArrayMap2D<bool> visibilityMap = new(200, 200);
    public FOV fov;
    public Eyes()
    {
      fov = new FOV(visibilityMap);
    }
    public readonly void Reset()
    {
      for (int x = 0; x < 200; x++)
      {
        for (int y = 0; y < 200; y++)
        {
          visibilityMap[new Coord(x, y)] = true;
        }
      }
    }
  }
}

