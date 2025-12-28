using Godot;
using Friflo.Engine.ECS;
using SadRogue.Primitives.GridViews;
using GoRogue.FOV;
using SadRogue.Primitives;

namespace Yaldabaranth.Scripts.ECS.Components;

public static class C
{
  public record struct Position(World World, Vector2I A) : IComponent
  {
    public World World = World;
    public Vector2I A = A;
    public readonly Vector2I G => A / (World.map.sectorSize * World.map.regionSize);
    public readonly Vector2I S => A / World.map.regionSize % World.map.sectorSize;
    public readonly Vector2I R => A % World.map.regionSize;
    public Position(
      World world,
      Vector2I g, Vector2I s, Vector2I r
    ) : this(
      world, (g * world.map.sectorSize * world.map.regionSize) + (s * world.map.regionSize) + r)
    { }
    public void Move(Vector2I absOffset) => A += absOffset;
  }
  public record struct Velocity(Vector2I V) : IComponent;
  public record struct Display(Tile T, Godot.Color C) : IComponent;
  public record struct Player : IComponent;
  public record struct Memory : IComponent;
  public record struct Eyes : IComponent
  {
    public World world;
    public ArrayView2D<bool> visMap;
    public RecursiveShadowcastingFOV fov;
    public float eyeSight = 5.0f;
    public Eyes(World world)
    {
      this.world = world;
      visMap = new(world.map.regionSize.X * 3, world.map.regionSize.Y * 3);
      fov = new RecursiveShadowcastingFOV(visMap);
    }
    public readonly void UpdateFOV(Position pos)
    {
      var posCoord = new Point(pos.R.X, pos.R.Y);
      fov.Calculate(posCoord, radius: eyeSight);
    }
    public readonly void Reset()
    {
      for (int x = 0; x < world.map.regionSize.X * 3; x++)
        for (int y = 0; y < world.map.regionSize.Y * 3; y++) visMap[new Point(x, y)] = true;
    }
  }
  public record struct Body : IComponent;
}
