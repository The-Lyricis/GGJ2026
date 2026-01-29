using System.Collections.Generic;

namespace GGJ2026
{
    public static class CombatSystem
    {
        public static void Resolve(List<BaseActor> actors, IGridWorld world)
        {
            // TODO（核心规则落点）：
            // 1) 仅在移动后执行一次
            // 2) 收集四向相邻冲突边（忽略 Green、中立不参战；忽略同 CombatColor）
            // 3) 生成冲突集合（连通分量），对每个集合按 Strength 裁决
            // 4) Kill 后从 world 占位移除/隐藏（建议由 world 或 TurnSystem 统一处理）
        }
    }
}