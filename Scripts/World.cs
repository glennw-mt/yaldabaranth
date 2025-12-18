using Godot;
using Friflo.Engine.ECS;
using Yaldabaranth.Scripts.ECS.Component;
using System;
using GoRogue.MapViews;
using GoRogue;

public partial class World : Node2D
{
  public EntityStore entities;
  public Tileset tileset;
  private Font _defaultFont = ThemeDB.FallbackFont;
  public ArrayMap2D<bool> visibilityMap;
  public FOV fov;
  public override void _Ready()
  {
    visibilityMap = new(200, 200);
    fov = new FOV(visibilityMap);
    Random rnd = new();
    entities = new EntityStore();
    tileset = new Tileset("res://assets/urizen_tileset.png");
    entities.CreateEntity(
        new C.Position(10, 10),
        new C.Velocity(new Vector2I(0, 0)),
        new C.Player(),
        new C.Visible(),
        new C.Display(T: Tile.Man, C: Color.Color8(255, 255, 255))
    );
    for (int x = -100; x < 100; x++)
    {
      for (int y = -100; y < 100; y++)
      {
        if (rnd.Next(0, 100) < 80)
        {
          continue;
        }
        entities.CreateEntity(
          new C.Position(x, y),
          new C.Velocity(new Vector2I(0, 0)),
          new C.Visible(),
          new C.Display(T: Tile.Tree, C: Color.Color8(255, 0, 0))
        );
      }
    }
  }
  public override void _Process(double delta)
  {
    S.Sync(this);
    S.Control(this);
    S.Movement(this);
    QueueRedraw();
  }
  public override void _Draw()
  {
    S.Display(this);
  }
}
