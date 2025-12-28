using Godot;
using Friflo.Engine.ECS;
using System;
using Yaldabaranth.Scripts.ECS.Entities;
using Yaldabaranth.Scripts;

public enum GameState
{
  Running, Menu
}

public partial class World : Node2D
{
  public EntityStore entities;
  public Tileset tileset;
  public Camera2D camera;
  public Menu menu;
  public GameState gameState = GameState.Running;
  public Font font;
  public Map map;
  public DatabaseManager db;
  public int seed;
  public override void _Ready()
  {
    db = new DatabaseManager();
    map = new Map(this);
    font = ThemeDB.FallbackFont;
    camera = new Camera2D();
    menu = new Menu(this, font) { Visible = false };
    camera.AddChild(menu);
    seed = 42;
    Random rnd = new(seed);
    AddChild(camera);
    entities = new EntityStore();
    tileset = new Tileset("res://Assets/urizen_tileset.png");
    E.SpawnPlayer(this, 10 + map.regionSize.X * 3, 10 + map.regionSize.Y * 3);
    for (int x = 0; x < 100; x++) for (int y = 0; y < 100; y++)
    {
      if (rnd.Next(0, 100) < 80) continue;
      E.SpawnTree(this, x + map.regionSize.X * 3, y + map.regionSize.Y * 3);
    }
    map.GenerateGlobe();
  }
  public override void _Process(double delta)
  {
    switch (gameState)
    {
      case GameState.Running:
        S.Control(this);
        S.See(this);
        S.MoveEntities(this);
        S.MoveCamera(this, (float)delta);
        break;
      case GameState.Menu: S.ControlMenu(this); break;
    }
    QueueRedraw();
  }
  public override void _Draw() => S.Display(this);
}
