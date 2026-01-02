using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using Yaldabaranth.Core;
using Yaldabaranth.Core.Scripts.ECS.Components;
using Yaldabaranth.Core.Scripts.ECS.Tags;

public partial class S
{
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
      if (e.Tags.HasAll(Tags.Get<T.Visible>()) && player_eyes.Fov.BooleanResultView[(int)p.R.X, (int)p.R.Y]) color = d.C;
      game.Tileset.DrawTile(d.T, (int)gp.X, (int)gp.Y, color);
    });
  }
  public static void MoveEntities(YaldabaranthGame game)
  {
    var query = game.Entities.Query<C.Position, C.Velocity>();
    query.ForEachEntity((ref C.Position p, ref C.Velocity v, Entity e) =>
    {
      p.Move(v.V);
      v.V = Vector2.Zero;
      if (e.HasComponent<C.Player>()) game.Map.PlayerPosition = p;
    });
  }
  public static void MoveCamera(YaldabaranthGame game, double delta)
  {
    var playerPos = game.Entities.Query<C.Position, C.Player>().ToEntityList()[0].GetComponent<C.Position>();
    var playerCanvasPos = playerPos.A * game.Tileset.Scale * game.Tileset.TileSize - game.GraphicsDevice.Viewport.Bounds.Size.ToVector2() / 2;
    game.Camera.Move((playerCanvasPos - game.Camera.Position) * (float)delta * 4);
  }
}
