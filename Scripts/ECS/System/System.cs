using Yaldabaranth.Scripts.ECS.Component;
using Friflo.Engine.ECS;
using Godot;
using GoRogue;


public partial class S
{
  public static void Sync(World world)
  {
    for (int x = 0; x < 200; x++) for (int y = 0; y < 200; y++)
      world.visibilityMap[new Coord(x, y)] = true;
    var query = world.entities.Query<C.Position, C.Visible>();
    query.ForEachEntity((ref C.Position p, ref C.Visible _, Entity _) =>
    {
      var visibilityCoord = new Coord(p.V.X + 100, p.V.Y + 100);
      world.visibilityMap[visibilityCoord] = false;
    });
  }
  public static void Display(World world)
  {
    var scaleFactor = world.tileset.tile_size * world.tileset.scale;
    var player = world.entities.Query<C.Position, C.Player>().ToEntityList()[0];
    C.Position player_pos = player.GetComponent<C.Position>();
    world.fov.Calculate(player_pos.V.X + 100, player_pos.V.Y + 100, 5);
    var query = world.entities.Query<C.Position, C.Display>();
    query.ForEachEntity((ref C.Position p, ref C.Display d, Entity e) =>
    {
      var tile = world.tileset.GetTile(d.T);
      var gp = p.V * scaleFactor;
      var color = Color.Color8(100, 100, 100);
      if (e.HasComponent<C.Visible>())
      {
        if (world.fov.BooleanFOV[p.V.X + 100, p.V.Y + 100])
        {
          color = Color.Color8(0, 255, 0);
        }
      }
      world.DrawTexture(tile, gp, color);
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
