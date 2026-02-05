using UnityEngine;

namespace CUBIE
{
    public class StoneActor : BaseActor, IActorEffect
    {
        public override ActorType ActorType => ActorType.Stone;

        public void OnResolve(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            if (actor == null || world == null || !actor.IsAlive) return;

            var cell = world.GetActorCell(actor);
            if (world.GetSurface(cell) == SurfaceType.Pit)
            {
                world.SetSurface(cell, SurfaceType.None);
                actor.Kill();
            }
        }
    }
}
