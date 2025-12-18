using Yaldabaranth.Scripts.ECS.Component;
using Friflo.Engine.ECS;
using Godot;
using GoRogue;
using System.Collections.Generic;
using System.Linq;
using Yaldabaranth.Scripts.ECS.Tag;


public partial class S
{
  public static void See(World world)
  {
    var query = world.entities.Query<C.Position, C.Eyes>();
    var eyes_pos_list = new List<(C.Eyes, C.Position)>();
    query.ForEachEntity((ref C.Position p, ref C.Eyes eyes, Entity _) =>
    {
      eyes.Reset();
      eyes_pos_list.Append((eyes, p));
    });
    var visible_query = world.entities.Query<C.Position>().AllTags(Tags.Get<T.Visible>());
    visible_query.ForEachEntity((ref C.Position pv, Entity _) =>
    {
      foreach ((C.Eyes, C.Position) eyes_pos in eyes_pos_list)
      {
        var eyes = eyes_pos.Item1;
        var pos = eyes_pos.Item2;
        var visibilityCoord = new Coord(pos.V.X + 100, pos.V.Y + 100);
        eyes.visibilityMap[visibilityCoord] = false;
      }
    });
    query.ForEachEntity((ref C.Position p, ref C.Eyes eyes, Entity _) =>
    {
      eyes.fov.Calculate(new Coord(p.V.X + 100, p.V.Y + 100), radius: 5);
    });
  }
  public static void Display(World world)
  {
    var scaleFactor = world.tileset.tile_size * world.tileset.scale;
    var player = world.entities.Query<C.Player>().ToEntityList()[0];
    C.Position player_pos = player.GetComponent<C.Position>();
    C.Eyes player_eyes = player.GetComponent<C.Eyes>();
    var query = world.entities.Query<C.Position, C.Display>();
    query.ForEachEntity((ref C.Position p, ref C.Display d, Entity e) =>
    {
      var tile = world.tileset.GetTile(d.T);
      var gp = p.V * scaleFactor;
      var color = Color.Color8(100, 100, 100);
      if (e.Tags.HasAll(Tags.Get<T.Visible>()))
      {
        if (player_eyes.fov.BooleanFOV[p.V.X + 100, p.V.Y + 100])
        {
          color = d.C;
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
  public static void MoveEntities(World world)
  {
    var query = world.entities.Query<C.Position, C.Velocity>();
    query.ForEachEntity((ref C.Position p, ref C.Velocity v, Entity e) =>
    {
      p.V += v.V;
      v.V = Vector2I.Zero;
    });
  }
  public static void MoveCamera(World world, float delta)
  {
    var player_pos = world.entities.Query<C.Position, C.Player>().ToEntityList()[0].GetComponent<C.Position>();
    var player_canvas_pos = player_pos.V * world.tileset.scale * world.tileset.tile_size;
    world.camera.Translate((player_canvas_pos - world.camera.Position) * delta * 4);
  }
}
