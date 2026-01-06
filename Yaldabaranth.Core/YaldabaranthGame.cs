using System;
using System.IO;
using FontStashSharp;
using Friflo.Engine.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Yaldabaranth.Core.Scripts;
using Yaldabaranth.Core.Scripts.Cosmos;
using Yaldabaranth.Core.Scripts.ECS.Entities;
using Yaldabaranth.Core.Scripts.Map;

namespace Yaldabaranth.Core
{
  public enum GameState
  {
    Generating, Running, Menu
  }
  public class YaldabaranthGame : Game
  {
    private readonly GraphicsDeviceManager graphicsDeviceManager;
    public SpriteBatch Canvas;
    public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
    public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();
    public Cosmos Cosmos;
    public Map Map;
    public Menu Menu;
    public EntityStore Entities;
    public GameState GameState;
    public Tileset Tileset;
    public FontSystem FontSystem;
    public InputManager Input;
    public OrthographicCamera Camera;
    public YaldabaranthGame()
    {
      graphicsDeviceManager = new GraphicsDeviceManager(this);
      Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);
      Content.RootDirectory = "Content";
      graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
    }
    protected override void Initialize()
    {
      Canvas = new(GraphicsDevice);
      Map = new(this);
      Map.GenerateGlobe();
      Cosmos = new(this);
      Entities = new();
      GameState = GameState.Generating;
      FontSystem = new();
      FontSystem.AddFont(File.ReadAllBytes("./Content/Assets/dungeon-mode.ttf"));
      Tileset = new Tileset(this, "./Content/Assets/urizen_tileset.png", 12, 1, 1);
      Camera = new(GraphicsDevice);
      Menu = new(this);
      Input = new();
      E.SpawnPlayer(this, new Vector2(0, 0), Vector2.Zero, Vector2.Zero, Vector2.Zero);
    }
    protected override void LoadContent()
    {
      base.LoadContent();
    }
    protected override void Update(GameTime gameTime)
    {
      if (GameState == GameState.Running)
      {
        S.MoveCamera(this, gameTime.ElapsedGameTime.TotalSeconds);
        S.ControlRunning(this);
        S.MoveEntities(this);
        S.See(this);
      }
      else if (GameState == GameState.Menu) S.ControlMenu(this);
      else if (GameState == GameState.Generating)
      {
        S.MoveCamera(this, gameTime.ElapsedGameTime.TotalSeconds);
        S.ControlRunning(this);
        S.MoveEntities(this);
      }
    }
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.Black);
      Canvas.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.GetViewMatrix());
      if (GameState != GameState.Generating) S.Display(this);
      else for (int i = 0; i < 1; i++) Cosmos.Update();
      Canvas.End();
      Canvas.Begin(samplerState: SamplerState.PointClamp);
      if (GameState == GameState.Menu) Menu.Draw();
      Canvas.End();
    }
  }
}
