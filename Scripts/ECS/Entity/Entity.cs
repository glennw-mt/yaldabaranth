using Yaldabaranth.Scripts.ECS.Component;
using Godot;
using Friflo.Engine.ECS;
using Yaldabaranth.Scripts.ECS.Tag;

namespace Yaldabaranth.Scripts.ECS.Entity;

public static class E
{
  public static void SpawnPlayer(World world, int x, int y)
  {
    var e = world.entities.CreateEntity(
      new C.Position(x, y),
      new C.Velocity(new Vector2I(0, 0)),
      new C.Player(),
      new C.Display(T: Tile.Man, C: Color.Color8(255, 255, 255)),
      new C.Eyes()
    );
    e.AddTag<T.Visible>();
    e.AddTag<T.Opaque>();
  }
  public static void SpawnTree(World world, int x, int y)
  {
    var e = world.entities.CreateEntity(
      new C.Position(x, y),
      new C.Velocity(new Vector2I(0, 0)),
      new C.Display(T: Tile.Tree, C: Color.Color8(255, 255, 0))
    );
    e.AddTag<T.Visible>();
    e.AddTag<T.Opaque>();
  }
}
