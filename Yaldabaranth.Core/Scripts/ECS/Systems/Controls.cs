using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Yaldabaranth.Core;
using Yaldabaranth.Core.Scripts;
using Yaldabaranth.Core.Scripts.ECS.Components;

public partial class S
{
  public static void ControlRunning(YaldabaranthGame game)
  {
    game.Input.UpdateKeyboardState();
    var query = game.Entities.Query<C.Velocity, C.Player>();
    query.ForEachEntity((ref C.Velocity v, ref C.Player _, Entity _) =>
    {
      if (game.Input.IsKeyJustPressed(Keys.Up)) v.V += new Vector2(0, -1);
      if (game.Input.IsKeyJustPressed(Keys.Down)) v.V += new Vector2(0, 1);
      if (game.Input.IsKeyJustPressed(Keys.Left)) v.V += new Vector2(-1, 0);
      if (game.Input.IsKeyJustPressed(Keys.Right)) v.V += new Vector2(1, 0);
      if (game.Input.IsKeyJustPressedExclusive(Keys.Escape) && game.GameState == GameState.Running)
        game.GameState = GameState.Menu;
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
      if (game.Input.IsKeyJustPressed(Keys.Left)) switch (game.Menu.SelectedMenuState)
        {
          case MenuState.Character:
            game.Menu.SelectedMenuState = MenuState.System; break;
          case MenuState.Map:
            game.Menu.SelectedMenuState = MenuState.Character; break;
          case MenuState.System:
            game.Menu.SelectedMenuState = MenuState.Map; break;
        }
      else if (game.Input.IsKeyJustPressed(Keys.Right)) switch (game.Menu.SelectedMenuState)
        {
          case MenuState.Character:
            game.Menu.SelectedMenuState = MenuState.Map; break;
          case MenuState.Map:
            game.Menu.SelectedMenuState = MenuState.System; break;
          case MenuState.System:
            game.Menu.SelectedMenuState = MenuState.Character; break;
        }
    }
    else if (game.Menu.ActiveMenuState == MenuState.Map)
    {
      if (game.Input.IsKeyPressed(Keys.Left)) game.Map.MapViewOffset.X += 1;
      else if (game.Input.IsKeyPressed(Keys.Right)) game.Map.MapViewOffset.X -= 1;
      if (game.Input.IsKeyPressed(Keys.Up)) game.Map.MapViewOffset.Y += 1;
      else if (game.Input.IsKeyPressed(Keys.Down)) game.Map.MapViewOffset.Y -= 1;
      if (game.Input.IsKeyJustPressed(Keys.Z)) game.Map.MapViewZoom *= 2;
      else if (game.Input.IsKeyJustPressed(Keys.X)) game.Map.MapViewZoom /= 2;
      if (game.Map.MapViewZoom <= 2) game.Map.MapViewZoom = 2;
      if (game.Map.MapViewZoom >= 32) game.Map.MapViewZoom = 32;
    }
    if (game.Input.IsKeyPressed(Keys.Enter)) switch (game.Menu.SelectedMenuState)
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
