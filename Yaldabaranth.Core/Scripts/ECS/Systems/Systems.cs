using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using Yaldabaranth.Core;
using Yaldabaranth.Core.Scripts;
using Microsoft.Xna.Framework.Input;
using Yaldabaranth.Core.Scripts.ECS.Components;
using Yaldabaranth.Core.Scripts.ECS.Tags;
using System;

public partial class S
{
  public static void See(YaldabaranthGame game)
  {
    var eyesQuery = game.Entities.Query<C.Position, C.Eyes>();
    var visibleQuery = game.Entities.Query<C.Position>().AllTags(Tags.Get<T.Visible>());
    eyesQuery.ForEachEntity((ref C.Position p, ref C.Eyes eyes, Entity _) => eyes.Reset());
    visibleQuery.ForEachEntity((ref C.Position pv, Entity _) =>
    {
      var pvCoord = new SadRogue.Primitives.Point(pv.R.ToPoint().X, pv.R.ToPoint().Y);
      eyesQuery.ForEachEntity((ref C.Position pe, ref C.Eyes eyes, Entity _) => eyes.VisMap[pvCoord] = false);
    });
    eyesQuery.ForEachEntity((ref C.Position pos, ref C.Eyes eyes, Entity _) => eyes.UpdateFOV(pos));
  }
  public static void Display(YaldabaranthGame game)
  {
    var scaleFactor = game.Tileset.TileSize * game.Tileset.Scale;
    var player = game.Entities.Query<C.Player>().ToEntityList()[0];
    C.Position player_pos = player.GetComponent<C.Position>();
    C.Eyes player_eyes = player.GetComponent<C.Eyes>();
    var query = game.Entities.Query<C.Position, C.Display>();
    query.ForEachEntity((ref C.Position p, ref C.Display d, Entity e) =>
    {
      var gp = p.A;
      var color = Color.DarkGray;
      if (e.Tags.HasAll(Tags.Get<T.Visible>()))
      {
        if (player_eyes.Fov.BooleanResultView[(int)p.R.X, (int)p.R.Y]) color = d.C;
      }
      game.Tileset.DrawTile(d.T, (int)gp.X, (int)gp.Y, color);
    });
  }
  public static void Control(YaldabaranthGame game)
  {
    game.Input.UpdateKeyboardState();
    var query = game.Entities.Query<C.Velocity, C.Player>();
    query.ForEachEntity((ref C.Velocity v, ref C.Player _, Entity _) =>
    {
      if (game.Input.IsKeyJustPressed(Keys.Up)) v.V += new Vector2(0, -1);
      if (game.Input.IsKeyJustPressed(Keys.Down)) v.V += new Vector2(0, 1);
      if (game.Input.IsKeyJustPressed(Keys.Left)) v.V += new Vector2(-1, 0);
      if (game.Input.IsKeyJustPressed(Keys.Right)) v.V += new Vector2(1, 0);
      if (game.Input.IsKeyJustPressedExclusive(Keys.Escape))
      {
        if (game.GameState == GameState.Running)
        {
          Console.WriteLine("MENU");
          game.GameState = GameState.Menu;
        }
      }
    });
  }
  public static void ControlMenu(YaldabaranthGame game)
  {
    if (game.Input.IsKeyJustPressedExclusive(Keys.Escape))
    {
      if (game.Menu.ActiveMenuState == MenuState.None) game.GameState = GameState.Running;
      else game.Menu.ActiveMenuState = MenuState.None;
    }
    if (game.Menu.ActiveMenuState == MenuState.None)
    {
      if (game.Input.IsKeyJustPressed(Keys.Left))
      {
        switch (game.Menu.SelectedMenuState)
        {
          case MenuState.Character:
            game.Menu.SelectedMenuState = MenuState.System; break;
          case MenuState.Map:
            game.Menu.SelectedMenuState = MenuState.Character; break;
          case MenuState.System:
            game.Menu.SelectedMenuState = MenuState.Map; break;
        }
      }
      else if (game.Input.IsKeyJustPressed(Keys.Right))
      {
        switch (game.Menu.SelectedMenuState)
        {
          case MenuState.Character:
            game.Menu.SelectedMenuState = MenuState.Map; break;
          case MenuState.Map:
            game.Menu.SelectedMenuState = MenuState.System; break;
          case MenuState.System:
            game.Menu.SelectedMenuState = MenuState.Character; break;
        }
      }
    }
    else if (game.Menu.ActiveMenuState == MenuState.Map)
    {
      if (game.Input.IsKeyPressed(Keys.Left))
      {
        game.Map.MapViewOffset.X += game.Map.MapViewZoom;
      }
      else if (game.Input.IsKeyPressed(Keys.Right))
      {
        game.Map.MapViewOffset.X -= game.Map.MapViewZoom;
      }
      if (game.Input.IsKeyPressed(Keys.Up))
      {
        game.Map.MapViewOffset.Y += game.Map.MapViewZoom;
      }
      else if (game.Input.IsKeyPressed(Keys.Down))
      {
        game.Map.MapViewOffset.Y -= game.Map.MapViewZoom;
      }
      if (game.Input.IsKeyJustPressed(Keys.Z))
      {
        game.Map.MapViewZoom += 1;
      }
      else if (game.Input.IsKeyJustPressed(Keys.X))
      {
        game.Map.MapViewZoom -= 1;
        if (game.Map.MapViewZoom <= 0) game.Map.MapViewZoom = 1;
      }
    }
    if (game.Input.IsKeyPressed(Keys.Enter))
    {
      switch (game.Menu.SelectedMenuState)
      {
        case MenuState.Character:
          game.Menu.ActiveMenuState = MenuState.Character; break;
        case MenuState.Map:
          game.Menu.ActiveMenuState = MenuState.Map; break;
        case MenuState.System:
          game.Menu.ActiveMenuState = MenuState.System; break;
      }
    }
  }
  public static void MoveEntities(YaldabaranthGame game)
  {
    var query = game.Entities.Query<C.Position, C.Velocity>();
    query.ForEachEntity((ref C.Position p, ref C.Velocity v, Entity e) =>
    {
      p.Move(v.V);
      v.V = Vector2.Zero;
    });
  }
  public static void MoveCamera(YaldabaranthGame game, double delta)
  {
    var playerPos = game.Entities.Query<C.Position, C.Player>().ToEntityList()[0].GetComponent<C.Position>();
    var playerCanvasPos = playerPos.A * game.Tileset.Scale * game.Tileset.TileSize - game.GraphicsDevice.Viewport.Bounds.Size.ToVector2() / 2;
    game.Camera.Move((playerCanvasPos - game.Camera.Position) * (float)delta * 4);
  }
}
