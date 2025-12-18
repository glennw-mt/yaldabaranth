using Godot;

namespace Yaldabaranth.Scripts;

public partial class Menu(Font font) : Node2D
{
  public override void _Draw()
  {
    var text_size = font.GetStringSize("PAUSED", fontSize: 48, alignment: HorizontalAlignment.Center);
    DrawString(
        font: font,
        pos: new Vector2(-text_size.X / 2, text_size.Y / 2),
        text: "PAUSED",
        modulate: Color.Color8(255, 255, 255),
        alignment: HorizontalAlignment.Center,
        fontSize: 48,
        justificationFlags: TextServer.JustificationFlag.None
    );
  }
}
