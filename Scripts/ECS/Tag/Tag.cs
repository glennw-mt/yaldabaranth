using Friflo.Engine.ECS;

namespace Yaldabaranth.Scripts.ECS.Tag;

public static class T
{
  public record struct Visible : ITag;
  public record struct Opaque : ITag;
}
