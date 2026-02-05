using UnityEngine;

namespace CUBIE
{
    public class ControlTrigger : MonoBehaviour, ITrigger
    {
        [SerializeField] private int priority = 0;
        [SerializeField] private ActorType controlType = ActorType.Player;
        [SerializeField] private bool disableOnPickup = true;

        public int Priority => priority;

        public bool Matches(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            if (actor == null || !actor.IsAlive || world == null) return false;
            if (actor is not PlayerActor) return false;

            var actorCell = world.GetActorCell(actor);
            var triggerCell = GridUtil.WorldToCell(transform.position);
            return actorCell == triggerCell;
        }

        public void Execute(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            if (actor is not PlayerActor player) return;
            player.SetControlType(controlType);

            if (disableOnPickup)
                gameObject.SetActive(false);
        }
    }
}
