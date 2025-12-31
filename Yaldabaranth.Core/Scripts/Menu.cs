using FontStashSharp;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Yaldabaranth.Core.Scripts;

public enum MenuState { None = 0, Character = 1, Map = 2, System = 3 }

public partial class Menu(YaldabaranthGame game)
{
  public MenuState SelectedMenuState = MenuState.Character;
  public MenuState ActiveMenuState = MenuState.None;
  readonly YaldabaranthGame game = game;
  Color selectedColor = Color.Yellow;
  readonly SpriteFontBase font = game.FontSystem.GetFont(48);
  public void Draw()
  {
    Rectangle screenRect = new(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
    game.Canvas.FillRectangle(screenRect, new Color(0, 0, 0, 150));

    if (SelectedMenuState == MenuState.Map && ActiveMenuState == MenuState.Map) game.Map.DebugBlit();
    float barHeight = 64f;
    game.Canvas.FillRectangle(new Rectangle(0, 0, screenRect.Width, (int)barHeight), Color.Black);

    float yPos = barHeight / 2 - font.FontSize / 2;
    void DrawMenuOption(string text, float xPos, float alignment, MenuState state)
    {
      Vector2 size = font.MeasureString(text);
      float adjustedXPos = xPos - size.X * alignment;
      Color c = (SelectedMenuState == state) ? (ActiveMenuState == state ? Color.Red : selectedColor) : Color.White;
      game.Canvas.DrawString(font, text, new Vector2(adjustedXPos, yPos), c);
    }
    DrawMenuOption("Character", font.FontSize / 2, 0, MenuState.Character);
    DrawMenuOption("Map", screenRect.Width / 2, 0.5f, MenuState.Map);
    DrawMenuOption("System", screenRect.Width - font.FontSize / 2, 1f, MenuState.System);
  }
}
