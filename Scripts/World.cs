using Godot;
using Friflo.Engine.ECS;
using System;
using Yaldabaranth.Scripts.ECS.Entity;
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
  public override void _Ready()
  {
    font = ThemeDB.FallbackFont;
    camera = new Camera2D();
    menu = new Menu(font) { Visible = false };
    camera.AddChild(menu);
    AddChild(camera);
    Random rnd = new();
    entities = new EntityStore();
    tileset = new Tileset("res://Assets/urizen_tileset.png");
    E.SpawnPlayer(this, 10, 10);
    for (int x = -100; x < 100; x++) for (int y = -100; y < 100; y++)
    {
      if (rnd.Next(0, 100) < 80) continue;
      E.SpawnTree(this, x, y);
    }
  }
  public override void _Process(double delta)
  {
    GD.Print(gameState);
    switch (gameState)
    {
      case GameState.Running:
        S.Control(this);
        S.See(this);
        S.MoveEntities(this);
        S.MoveCamera(this, (float)delta);
        break;
      case GameState.Menu:
        S.ControlMenu(this);
        break;
    }
    QueueRedraw();
  }
  public override void _Draw()
  {
    switch (gameState)
    {
      case GameState.Running:
        S.Display(this);
        break;
      case GameState.Menu:
        S.DisplayMenu(this);
        break;
    }
  }
}
