using UnityEngine;

namespace CUBIE
{
    public class ButtonTrigger : MonoBehaviour, ITrigger
    {
        [SerializeField] private int priority = 0;
        [SerializeField] private string id = "button";
        [SerializeField] private bool latch = false;
        [SerializeField] private bool requireActorType = true;
        [SerializeField] private ActorType requiredActorType = ActorType.Player;

        public int Priority => priority;
        public string Id => id;
        [Tooltip("Latch: Once pressed, the button stays active permanently.")]
        public bool Latch => latch;
        public bool RequireActorType => requireActorType;
        public ActorType RequiredActorType => requiredActorType;

        public static event System.Action<ButtonTrigger, BaseActor> OnPressed;

        public bool Matches(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            if (actor == null || !actor.IsAlive || world == null) return false;
            var actorCell = world.GetActorCell(actor);
            var triggerCell = GridUtil.WorldToCell(transform.position);
            if (actorCell != triggerCell) return false;
            if (requireActorType && actor.ActorType != requiredActorType) return false;
            return true;
        }

        public void Execute(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            OnPressed?.Invoke(this, actor);
        }
    }
}
