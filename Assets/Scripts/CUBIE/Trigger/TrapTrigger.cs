// TrapTrigger.cs
using UnityEngine;

namespace CUBIE
{
    public class TrapTrigger : MonoBehaviour, ITrigger
    {
        [SerializeField] private int priority = 0;
        [SerializeField] private bool killOnTrigger = true;

        public int Priority => priority;

        public bool Matches(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            if (actor == null || !actor.IsAlive || world == null) return false;
            var actorCell = world.GetActorCell(actor);
            var triggerCell = GridUtil.WorldToCell(transform.position);
            return actorCell == triggerCell;
        }

        public void Execute(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            if (killOnTrigger)
                actor.Kill();
        }
    }
}
