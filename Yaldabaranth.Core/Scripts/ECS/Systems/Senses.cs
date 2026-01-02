using Friflo.Engine.ECS;
using Yaldabaranth.Core;
using Yaldabaranth.Core.Scripts.ECS.Components;
using Yaldabaranth.Core.Scripts.ECS.Tags;

public partial class S
{
  public static void See(YaldabaranthGame game)
  {
    var eyesQuery = game.Entities.Query<C.Position, C.Eyes>();
    var visibleQuery = game.Entities.Query<C.Position>().AllTags(Tags.Get<T.Visible>());
    eyesQuery.ForEachEntity((ref C.Position p, ref C.Eyes eyes, Entity _) => eyes.Reset());
    visibleQuery.ForEachEntity((ref C.Position pv, Entity _) =>
    {
      var pvCoord = new SadRogue.Primitives.Point(pv.R.ToPoint().X, pv.R.ToPoint().Y);
      eyesQuery.ForEachEntity((ref C.Position pe, ref C.Eyes eyes, Entity _) => eyes.VisMap[pvCoord] = false);
    });
    eyesQuery.ForEachEntity((ref C.Position pos, ref C.Eyes eyes, Entity _) => eyes.UpdateFOV(pos));
  }

}
