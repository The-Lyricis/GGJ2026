using UnityEngine;

namespace CUBIE
{
    public class FireActor : BaseActor, IActorEffect
    {
        public override ActorType ActorType => ActorType.Fire;

        public void OnResolve(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            if (actor == null || world == null || !actor.IsAlive) return;

            var cell = world.GetActorCell(actor);
            var dirs = new[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            for (int i = 0; i < dirs.Length; i++)
            {
                var c = cell + dirs[i];
                if (world.GetBlock(c) == BlockType.Wood)
                    world.SetBlock(c, BlockType.None);
            }
        }
    }
}
