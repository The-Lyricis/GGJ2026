using System.Collections.Generic;
using UnityEngine;

namespace CUBIE
{
    public class TurnContext
    {
        // 1) 意图
        private readonly Dictionary<BaseActor, MoveIntent> intents = new();

        // 2) 计划移动
        private readonly Dictionary<BaseActor, Vector2Int> plannedCellByActor = new();

        // 3) 回合起始占位快照
        private readonly Dictionary<Vector2Int, BaseActor> startCellToActor = new();

        // 4) 目标预占（避免同格冲突）
        private readonly Dictionary<Vector2Int, BaseActor> reservedCellToActor = new();

        // 5) 阻挡标记
        private readonly Dictionary<BaseActor, bool> blockedByActor = new();
        
        public void BuildSnapshot(IReadOnlyList<BaseActor> actors, IGridWorld world)
        {
            startCellToActor.Clear();
            reservedCellToActor.Clear();
            blockedByActor.Clear();

            if (actors == null || world == null) return;

            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (a == null || !a.IsAlive) continue;

                var cell = world.GetActorCell(a);
                startCellToActor[cell] = a;
            }
        }

        public void SetIntent(BaseActor actor, MoveIntent intent)
        {
            intents[actor] = intent;
        }

        public MoveIntent GetIntent(BaseActor actor)
        {
            return intents.TryGetValue(actor, out var i) ? i : MoveIntent.None;
        }

        public void QueueMove(BaseActor actor, Vector2Int targetCell)
        {
            plannedCellByActor[actor] = targetCell;
            reservedCellToActor[targetCell] = actor;
        }

        public bool TryGetPlannedMove(BaseActor actor, out Vector2Int cell)
        {
            return plannedCellByActor.TryGetValue(actor, out cell);
        }

        public void ClearPlannedMoves()
        {
            plannedCellByActor.Clear();
            reservedCellToActor.Clear();
        }

        public void SetBlocked(BaseActor actor, bool blocked)
        {
            if (actor == null) return;
            blockedByActor[actor] = blocked;
        }

        public bool IsBlocked(BaseActor actor)
        {
            return actor != null && blockedByActor.TryGetValue(actor, out var b) && b;
        }
        

        public float ApplyMoves(IGridWorld world)
        {
            float max = 0f;
            if (world == null) return max;

            foreach (var kv in plannedCellByActor)
            {
                if (kv.Key == null || !kv.Key.IsAlive) continue;
                float t = world.MoveActor(kv.Key, kv.Value);
                if (t > max) max = t;
            }

            plannedCellByActor.Clear();
            reservedCellToActor.Clear();
            return max;
        }
    }
}
