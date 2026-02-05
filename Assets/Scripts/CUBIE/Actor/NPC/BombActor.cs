using UnityEngine;

namespace CUBIE
{
    public class BombActor : BaseActor
    {
        [SerializeField] private int explosionRadius = 1; // Chebyshev radius: 1 => 3x3

        public override ActorType ActorType => ActorType.Bomb;

        public override void Kill()
        {
            if (!IsAlive) return;
            Explode();
            base.Kill();
        }

        private void Explode()
        {
            var world = FindWorld();
            if (world == null) return;

            var center = world.GetActorCell(this);
            var actors = FindObjectsByType<BaseActor>(FindObjectsSortMode.None);
            for (int i = 0; i < actors.Length; i++)
            {
                var a = actors[i];
                if (a == null || !a.IsAlive) continue;
                if (a == this) continue;

                var cell = world.GetActorCell(a);
                if (Mathf.Abs(cell.x - center.x) <= explosionRadius &&
                    Mathf.Abs(cell.y - center.y) <= explosionRadius)
                {
                    a.Kill();
                }
            }
        }

        private static IGridWorld FindWorld()
        {
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IGridWorld world)
                    return world;
            }
            return null;
        }
    }
}
