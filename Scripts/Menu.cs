using Godot;

namespace Yaldabaranth.Scripts;

public enum MenuState
{
  Character = 1, Map = 2, System = 3
}

public partial class Menu(World world, Font font) : Node2D
{
  public MenuState menuState = MenuState.Character;
  Color selectedColor = Color.Color8(255, 255, 0);
  float GetStringWidth(string text)
  {
    return font.GetStringSize(text, fontSize: 48).X;
  }
  public override void _Draw()
  {
    DrawRect(new Rect2(position: new Vector2(-4000, -4000), size: new Vector2(8000, 8000)), Color.Color8(0, 0, 0, 100));
    Vector2 viewport_size = GetViewportRect().Size;
    Vector2 top_left = -viewport_size / 2;
    Vector2 top_right = top_left + Vector2.Right * viewport_size.X;
    float sixth_width = viewport_size.X / 6;
    float top_bar_height = viewport_size.Y * 0.1f;
    Vector2 top_bar_mid_offset = Vector2.Down * (24 + top_bar_height * 0.5f);
    DrawRect(new Rect2(position: top_left, size: new Vector2(viewport_size.X, top_bar_height)), Color.Color8(0, 0, 0));
    DrawString(
        font,
        pos: top_left + top_bar_mid_offset + Vector2.Right * sixth_width / 4,
        text: "Character",
        fontSize: 48,
        modulate: menuState == MenuState.Character ? selectedColor : Color.Color8(255, 255, 255)
    );
    DrawString(
        font,
        pos: top_left + top_bar_mid_offset + Vector2.Right * viewport_size.X / 2 + Vector2.Left * (GetStringWidth("MAP") / 2),
        text: "Map",
        fontSize: 48,
        modulate: menuState == MenuState.Map ? selectedColor : Color.Color8(255, 255, 255)
    );
    DrawString(
        font,
        pos: top_right + top_bar_mid_offset + Vector2.Left * (GetStringWidth("SYSTEM") + sixth_width / 4),
        text: "System",
        fontSize: 48,
        modulate: menuState == MenuState.System ? selectedColor : Color.Color8(255, 255, 255)
    );
    DrawString(
        font: font,
        pos: new Vector2(-GetStringWidth("PAUSED") / 2, viewport_size.Y / 2 - top_bar_height * 0.5f),
        text: "PAUSED",
        modulate: Color.Color8(255, 255, 255),
        fontSize: 48
    );
    if (menuState == MenuState.Map)
    {
      world.map.DebugBlit(this);
    }
  }
  public override void _Process(double delta) => QueueRedraw();
}
