using Microsoft.Xna.Framework.Input;

namespace Yaldabaranth.Core.Scripts;

public class InputManager
{
  KeyboardState oldState = Keyboard.GetState();
  KeyboardState newState = Keyboard.GetState();
  public void UpdateKeyboardState()
  {
    oldState = newState;
    newState = Keyboard.GetState();
  }
  public bool IsKeyJustPressed(Keys key) => oldState.IsKeyUp(key) && newState.IsKeyDown(key);
  public bool IsKeyJustPressedExclusive(Keys key)
  {
    var result = IsKeyJustPressed(key);
    UpdateKeyboardState();
    return result;
  }
  public bool IsKeyReleased(Keys key) => oldState.IsKeyDown(key) && newState.IsKeyUp(key);
  public bool IsKeyPressed(Keys key) => newState.IsKeyDown(key);
}
