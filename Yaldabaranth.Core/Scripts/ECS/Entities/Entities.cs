using Friflo.Engine.ECS;
using Yaldabaranth.Core.Scripts.ECS.Components;
using Microsoft.Xna.Framework;
using Yaldabaranth.Core.Scripts.ECS.Tags;

namespace Yaldabaranth.Core.Scripts.ECS.Entities;

public static class E
{
  public static void SpawnPlayer(YaldabaranthGame game, int x, int y)
  {
    var e = game.Entities.CreateEntity(
      new C.Position(game, new Vector2(x, y)),
      new C.Velocity(new Vector2(0, 0)),
      new C.Player(),
      new C.Display(Tile.Man, Color.White),
      new C.Eyes(game)
    );
    e.AddTag<T.Visible>();
    e.AddTag<T.Opaque>();
  }
  public static void SpawnTree(YaldabaranthGame game, int x, int y)
  {
    var e = game.Entities.CreateEntity(
      new C.Position(game, new Vector2(x, y)),
      new C.Velocity(new Vector2(0, 0)),
      new C.Display(T: Tile.Tree, C: Color.Yellow)
    );
    e.AddTag<T.Visible>();
    e.AddTag<T.Opaque>();
  }
}
