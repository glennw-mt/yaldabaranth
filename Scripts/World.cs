using Godot;
using Friflo.Engine.ECS;
using System;
using GoRogue.MapViews;
using GoRogue;
using Yaldabaranth.Scripts.ECS.Entity;

public partial class World : Node2D
{
  public EntityStore entities;
  public Tileset tileset;
  private Font _defaultFont = ThemeDB.FallbackFont;
  public ArrayMap2D<bool> collisionMap;
  public FOV fov;
  public Camera2D camera;
  public override void _Ready()
  {
    camera = new Camera2D();
    AddChild(camera);
    Random rnd = new();
    entities = new EntityStore();
    tileset = new Tileset("res://assets/urizen_tileset.png");
    E.SpawnPlayer(this, 10, 10);
    for (int x = -100; x < 100; x++) for (int y = -100; y < 100; y++)
    {
      if (rnd.Next(0, 100) < 80) continue;
      E.SpawnTree(this, x, y);
    }
  }
  public override void _Process(double delta)
  {
    S.See(this);
    S.Control(this);
    S.MoveEntities(this);
    S.MoveCamera(this, (float)delta);
    QueueRedraw();
  }
  public override void _Draw()
  {
    S.Display(this);
  }
}
