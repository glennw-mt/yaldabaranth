using Yaldabaranth.Scripts.ECS.Component;
using Friflo.Engine.ECS;
using Godot;


public partial class S
{
  public static void Display(World world)
  {
    var scaleFactor = world.tileset.tile_size * world.tileset.scale;
    var query = world.entities.Query<C.Position, C.Display>();
    query.ForEachEntity((ref C.Position p, ref C.Display d, Entity e) =>
    {
      world.DrawTexture(
          world.tileset.GetTile(d.T),
          p.V * scaleFactor);
    });
  }
  public static void Control(World world)
  {
    var query = world.entities.Query<C.Velocity, C.Player>();
    query.ForEachEntity((ref C.Velocity v, ref C.Player _, Entity _) =>
    {
      if (Input.IsActionJustPressed("ui_up"))
      {
        v.V += Vector2I.Up;
      }
      if (Input.IsActionJustPressed("ui_down"))
      {
        v.V += Vector2I.Down;
      }
      if (Input.IsActionJustPressed("ui_left"))
      {
        v.V += Vector2I.Left;
      }
      if (Input.IsActionJustPressed("ui_right"))
      {
        v.V += Vector2I.Right;
      }
    });
  }
  public static void Movement(World world)
  {
    var query = world.entities.Query<C.Position, C.Velocity>();
    query.ForEachEntity((ref C.Position p, ref C.Velocity v, Entity e) =>
    {
      p.V += v.V;
      v.V = Vector2I.Zero;
    });
  }
}
