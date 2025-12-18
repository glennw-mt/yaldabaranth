using Godot;
using Friflo.Engine.ECS;

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
}


