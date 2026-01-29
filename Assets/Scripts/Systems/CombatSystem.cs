using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public static class CombatSystem
    {
        /// <summary>
        /// 重合后才结算：按同一格子内的全部参与者进行一次性裁决。
        /// 只处理战斗结果，不做阻挡合法性判定。
        /// </summary>
        public static void Resolve(List<BaseActor> actors, IGridWorld world)
        {
            if (actors == null || world == null) return;

            // 以格子为单位分组：同一格子内的 actor 视为一次冲突
            Dictionary<Vector2Int, List<BaseActor>> groups = new();
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (a == null || !a.IsAlive) continue;

                var cell = world.GetActorCell(a);
                if (!groups.TryGetValue(cell, out var list))
                {
                    list = new List<BaseActor>(4);
                    groups[cell] = list;
                }
                list.Add(a);
            }

            foreach (var kv in groups)
            {
                var list = kv.Value;
                if (list.Count <= 1) continue;

                // 中立不参战
                List<BaseActor> fighters = new();
                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    if (a.CombatColor == FactionColor.Green) continue;
                    fighters.Add(a);
                }

                if (fighters.Count <= 1) continue;

                // 一次性裁决：最高强度存活，其余死亡
                int maxStrength = -1;
                for (int i = 0; i < fighters.Count; i++)
                {
                    int s = CombatResolver.Strength(fighters[i].CombatColor);
                    if (s > maxStrength) maxStrength = s;
                }

                for (int i = 0; i < fighters.Count; i++)
                {
                    var a = fighters[i];
                    if (CombatResolver.Strength(a.CombatColor) < maxStrength)
                    {
                        a.Kill();
                    }
                }
            }
        }
    }
}
