using Yaldabaranth.Scripts.ECS.Components;
using Friflo.Engine.ECS;
using Godot;
using GoRogue;
using Yaldabaranth.Scripts.ECS.Tags;
using Yaldabaranth.Scripts;

public partial class S
{
  public static void See(World world)
  {
    var eyesQ = world.entities.Query<C.Position, C.Eyes>();
    var visibleQ = world.entities.Query<C.Position>().AllTags(Tags.Get<T.Visible>());
    eyesQ.ForEachEntity((ref C.Position p, ref C.Eyes eyes, Entity _) => eyes.Reset());
    visibleQ.ForEachEntity((ref C.Position pv, Entity _) =>
    {
      var pvCoord = new Coord(pv.V.X + 100, pv.V.Y + 100);
      eyesQ.ForEachEntity((ref C.Position pe, ref C.Eyes eyes, Entity _) => eyes.visMap[pvCoord] = false);
    });
    eyesQ.ForEachEntity((ref C.Position pos, ref C.Eyes eyes, Entity _) => eyes.UpdateFOV(pos));
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
  public static void DisplayMenu(World world)
  {

  }
  public static void Control(World world)
  {
    var query = world.entities.Query<C.Velocity, C.Player>();
    query.ForEachEntity((ref C.Velocity v, ref C.Player _, Entity _) =>
    {
      if (Input.IsActionJustPressed("ui_up")) v.V += Vector2I.Up;
      if (Input.IsActionJustPressed("ui_down")) v.V += Vector2I.Down;
      if (Input.IsActionJustPressed("ui_left")) v.V += Vector2I.Left;
      if (Input.IsActionJustPressed("ui_right")) v.V += Vector2I.Right;
      if (Input.IsActionJustPressed("menu"))
      {
        world.gameState = GameState.Menu;
        world.menu.Visible = true;
      }
    });
  }
  public static void ControlMenu(World world)
  {
    if (Input.IsActionJustPressed("menu"))
    {
      world.gameState = GameState.Running;
      world.menu.Visible = false;
    }
    if (Input.IsActionJustPressed("ui_left"))
    {
      switch (world.menu.menuState)
      {
        case MenuState.Character:
          world.menu.menuState = MenuState.System;
          break;
        case MenuState.Map:
          world.menu.menuState = MenuState.Character;
          break;
        case MenuState.System:
          world.menu.menuState = MenuState.Map;
          break;
      }
    }
    else if (Input.IsActionJustPressed("ui_right"))
    {
      switch (world.menu.menuState)
      {
        case MenuState.Character:
          world.menu.menuState = MenuState.Map;
          break;
        case MenuState.Map:
          world.menu.menuState = MenuState.System;
          break;
        case MenuState.System:
          world.menu.menuState = MenuState.Character;
          break;
      }
    }
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
