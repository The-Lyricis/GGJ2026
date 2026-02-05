using UnityEngine;

namespace CUBIE
{
    public class IceActor : BaseActor, IActorEffect
    {
        public override ActorType ActorType => ActorType.Ice;

        public void OnResolve(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            if (actor == null || world == null || !actor.IsAlive) return;

            var cell = world.GetActorCell(actor);
            if (world.GetSurface(cell) == SurfaceType.Water)
                world.SetSurface(cell, SurfaceType.Ice);
        }
    }
}
