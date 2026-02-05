using UnityEngine;
namespace GGJ2026
{
    public class GridMovementTrait : MonoBehaviour, IActorTrait
    {
        private BaseActor actor;
        private IGridWorld world;

        public void OnAdded(BaseActor a)
        {
            actor = a;
            // 推荐：由 TurnSystem/World 注入；此处先给出最简可跑方式
            world = Object.FindFirstObjectByType<GridWorldBehaviour>();
        }

        public void OnRemoved(BaseActor a) { }

        public void OnIntent(BaseActor a, MoveIntent intent, TurnContext ctx)
        {
            Debug.Log($"[OnIntent] {a.name} intent={intent.dir}");
            if (!a.IsAlive) return;
            if (intent.dir == MoveDir.None) return;
            if (world == null) return;

            var from = world.GetActorCell(a);
            var to = ctx.ResolveMovement(a, from, intent.dir, world);

            if (to != from)
                ctx.QueueMove(a, to);
        }
    }
}
