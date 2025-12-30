using Yaldabaranth.Scripts.ECS.Components;
using Friflo.Engine.ECS;
using Godot;
using Yaldabaranth.Scripts.ECS.Tags;
using Yaldabaranth.Scripts;
using SadRogue.Primitives;

public partial class S
{
  public static void See(World world)
  {
    var eyesQuery = world.entities.Query<C.Position, C.Eyes>();
    var visibleQuery = world.entities.Query<C.Position>().AllTags(Tags.Get<T.Visible>());
    eyesQuery.ForEachEntity((ref C.Position p, ref C.Eyes eyes, Entity _) => eyes.Reset());
    visibleQuery.ForEachEntity((ref C.Position pv, Entity _) =>
    {
      var pvCoord = new Point(pv.R.X, pv.R.Y);
      eyesQuery.ForEachEntity((ref C.Position pe, ref C.Eyes eyes, Entity _) => eyes.visMap[pvCoord] = false);
    });
    eyesQuery.ForEachEntity((ref C.Position pos, ref C.Eyes eyes, Entity _) => eyes.UpdateFOV(pos));
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
      var gp = p.A * scaleFactor;
      var color = Godot.Color.Color8(100, 100, 100);
      if (e.Tags.HasAll(Tags.Get<T.Visible>()))
      {
        if (player_eyes.fov.BooleanResultView[p.R.X, p.R.Y]) color = d.C;
      }
      world.DrawTexture(tile, gp, color);
    });
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
      if (Input.IsActionJustPressed("ui_cancel"))
      {
        world.gameState = GameState.Menu;
        world.menu.Visible = true;
      }
    });
  }
  public static void ControlMenu(World world)
  {
    if (Input.IsActionJustPressed("ui_cancel"))
    {
      if (world.menu.activeMenuState == MenuState.None)
      {
        world.gameState = GameState.Running;
        world.menu.Visible = false;
      }
      else world.menu.activeMenuState = MenuState.None;
    }
    if (world.menu.activeMenuState == MenuState.None)
    {
      if (Input.IsActionJustPressed("ui_left"))
      {
        switch (world.menu.selectedMenuState)
        {
          case MenuState.Character:
            world.menu.selectedMenuState = MenuState.System; break;
          case MenuState.Map:
            world.menu.selectedMenuState = MenuState.Character; break;
          case MenuState.System:
            world.menu.selectedMenuState = MenuState.Map; break;
        }
      }
      else if (Input.IsActionJustPressed("ui_right"))
      {
        switch (world.menu.selectedMenuState)
        {
          case MenuState.Character:
            world.menu.selectedMenuState = MenuState.Map; break;
          case MenuState.Map:
            world.menu.selectedMenuState = MenuState.System; break;
          case MenuState.System:
            world.menu.selectedMenuState = MenuState.Character; break;
        }
      }
    }
    else if (world.menu.activeMenuState == MenuState.Map)
    {
      if (Input.IsActionPressed("ui_left"))
      {
        world.map.mapViewOffset.X += world.map.mapViewZoom;
      }
      else if (Input.IsActionPressed("ui_right"))
      {
        world.map.mapViewOffset.X -= world.map.mapViewZoom;
      }
      if (Input.IsActionPressed("ui_up"))
      {
        world.map.mapViewOffset.Y += world.map.mapViewZoom;
      }
      else if (Input.IsActionPressed("ui_down"))
      {
        world.map.mapViewOffset.Y -= world.map.mapViewZoom;
      }
      if (Input.IsActionJustPressed("zoom_in"))
      {
        world.map.mapViewZoom += 1;
      }
      else if (Input.IsActionJustPressed("zoom_out"))
      {
        world.map.mapViewZoom -= 1;
        if (world.map.mapViewZoom <= 0) world.map.mapViewZoom = 1;
      }
    }
    if (Input.IsActionPressed("ui_accept"))
    {
      switch (world.menu.selectedMenuState)
      {
        case MenuState.Character:
          world.menu.activeMenuState = MenuState.Character; break;
        case MenuState.Map:
          world.menu.activeMenuState = MenuState.Map; break;
        case MenuState.System:
          world.menu.activeMenuState = MenuState.System; break;
      }
    }
  }
  public static void MoveEntities(World world)
  {
    var query = world.entities.Query<C.Position, C.Velocity>();
    query.ForEachEntity((ref C.Position p, ref C.Velocity v, Entity e) =>
    {
      p.Move(v.V);
      v.V = Vector2I.Zero;
    });
  }
  public static void MoveCamera(World world, float delta)
  {
    var player_pos = world.entities.Query<C.Position, C.Player>().ToEntityList()[0].GetComponent<C.Position>();
    var player_canvas_pos = player_pos.A * world.tileset.scale * world.tileset.tile_size;
    world.camera.Translate((player_canvas_pos - world.camera.Position) * delta * 4);
  }
}
