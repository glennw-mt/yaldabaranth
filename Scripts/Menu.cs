using Godot;

namespace Yaldabaranth.Scripts;

public enum SelectedMenuState { None = 0, Character = 1, Map = 2, System = 3 }

public partial class Menu(World world, Font font) : Node2D
{
  public SelectedMenuState selectedMenuState = SelectedMenuState.Character;
  public SelectedMenuState activeMenuState = SelectedMenuState.None;
  Color selectedColor = Color.Color8(255, 255, 0);
  float GetStringWidth(string text) => font.GetStringSize(text, fontSize: 48).X;
  public override void _Draw()
  {
    DrawRect(new Rect2(position: new Vector2(-4000, -4000), size: new Vector2(8000, 8000)), Color.Color8(0, 0, 0, 100));
    Vector2 viewSize = GetViewportRect().Size;
    Vector2 topLeft = -viewSize / 2;
    Vector2 topRight = topLeft + Vector2.Right * viewSize.X;
    float sixthWidth = viewSize.X / 6;
    float barHeight = viewSize.Y * 0.1f;
    Vector2 midOff = Vector2.Down * (24 + barHeight * 0.5f);
    Color white = Color.Color8(255, 255, 255);
    Color red = Color.Color8(255, 0, 0);
    DrawRect(new Rect2(position: topLeft, size: new Vector2(viewSize.X, barHeight)), Color.Color8(0, 0, 0));
    void DrawMenuOption(string text, Vector2 pos, SelectedMenuState state)
    {
      DrawString(font, pos, text, fontSize: 48,
          modulate: selectedMenuState == state ? activeMenuState == state ? red : selectedColor : white);
    }
    DrawMenuOption("Character", topLeft + midOff + Vector2.Right * (sixthWidth / 4), SelectedMenuState.Character);
    DrawMenuOption("Map", topLeft + midOff + Vector2.Right * (viewSize.X / 2) + Vector2.Left * (GetStringWidth("MAP") / 2), SelectedMenuState.Map);
    DrawMenuOption("System", topRight + midOff + Vector2.Left * (GetStringWidth("SYSTEM") + sixthWidth / 4), SelectedMenuState.System);
    if (selectedMenuState == SelectedMenuState.Map) world.map.DebugBlit(this);
  }
  public override void _Process(double delta) => QueueRedraw();
}
