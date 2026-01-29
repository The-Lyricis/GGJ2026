using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public class TurnContext
    {
        // 玩家当前控制色（由 TurnSystem 每回合写入）
        public FactionColor playerControlColor;

        // 每个 Actor 的回合意图
        private readonly Dictionary<BaseActor, MoveIntent> intents = new();

        // 统一应用移动
        private readonly Dictionary<BaseActor, Vector2Int> pendingMoves = new();

        // 回合开始占位快照：用于“阻挡按回合开始快照”
        private readonly Dictionary<Vector2Int, BaseActor> occupancySnapshot = new();

        public void BuildSnapshot(List<BaseActor> actors, IGridWorld world)
        {
            occupancySnapshot.Clear();
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (!a.IsAlive) continue;
                var cell = world.GetActorCell(a);
                occupancySnapshot[cell] = a;
            }
        }

        public void SetIntent(BaseActor a, MoveIntent i) => intents[a] = i;
        public MoveIntent GetIntent(BaseActor a) => intents.TryGetValue(a, out var i) ? i : MoveIntent.None;

        public void QueueMove(BaseActor a, Vector2Int cell) => pendingMoves[a] = cell;

        public Vector2Int ResolveMovement(BaseActor a, Vector2Int from, MoveDir dir, IGridWorld world)
        {
            // TODO（核心规则落点）：
            // - 使用 occupancySnapshot 做阻挡判定（墙、边界、占位、同色排队链）
            // - 冰面：滑行到尽头/被挡前一格；遇人/机关停
            // - 同步冲突：可在这里或 TurnSystem 中先做“玩家阻挡总闸门”
            return from;
        }

        public void ApplyMoves(IGridWorld world)
        {
            foreach (var kv in pendingMoves)
            {
                if (!kv.Key.IsAlive) continue;
                world.MoveActor(kv.Key, kv.Value);
            }
            pendingMoves.Clear();
        }

        public void ResolveTriggers(IGridWorld world)
        {
            // TODO：面具拾取、按钮触发
        }
    }
}
