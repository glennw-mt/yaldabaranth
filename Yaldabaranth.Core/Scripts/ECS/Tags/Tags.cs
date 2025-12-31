using Friflo.Engine.ECS;

namespace Yaldabaranth.Core.Scripts.ECS.Tags;

public static class T
{
  public record struct Visible : ITag;
  public record struct Opaque : ITag;
}
