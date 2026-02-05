using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public static class CombatSystem
    {
        public static void Resolve(List<BaseActor> actors, IGridWorld world)
        {
            if (actors == null || world == null) return;

            // 以格子为单位分组
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

                // 1) 把玩家单独拎出来（玩家不输出）
                PlayerActor player = null;

                // 2) 参与 Actor vs Actor 的战斗者（排除 GreenActor、中立；排除玩家）
                List<BaseActor> fighters = new();

                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    if (a == null || !a.IsAlive) continue;

                    if (a is PlayerActor p)
                    {
                        player = p;
                        continue;
                    }

                    // GreenActor 中立：不参战（注意这里用类型判断，不用 CombatColor 判断）
                    if (a is GreenActor) continue;

                    fighters.Add(a);
                }

                // A) 先结算 Actor vs Actor
                ResolveActorVsActor(fighters);

                // B) 再结算 Actor vs Player（玩家只可能被杀，不可能杀人）
                if (player != null && player.IsAlive)
                {
                    ResolveActorVsPlayer(player, fighters);
                }
            }
        }

        private static void ResolveActorVsActor(List<BaseActor> fighters)
        {
            if (fighters == null || fighters.Count <= 1) return;

            int maxStrength = int.MinValue;
            for (int i = 0; i < fighters.Count; i++)
            {
                var a = fighters[i];
                if (a == null || !a.IsAlive) continue;

                int s = CombatResolver.Strength(a.CombatColor);
                if (s > maxStrength) maxStrength = s;
            }

            for (int i = 0; i < fighters.Count; i++)
            {
                var a = fighters[i];
                if (a == null || !a.IsAlive) continue;

                if (CombatResolver.Strength(a.CombatColor) < maxStrength)
                    a.Kill();
            }
        }

        private static void ResolveActorVsPlayer(PlayerActor player, List<BaseActor> fighters)
        {
            // 玩家防御强度：由面具决定（你可按“无面具=0”或“-1”调整）
            int playerDefense = CombatResolver.Strength(player.CombatColor);

            for (int i = 0; i < fighters.Count; i++)
            {
                var a = fighters[i];
                if (a == null || !a.IsAlive) continue;

                int atk = CombatResolver.Strength(a.CombatColor);

                // 规则：低阶面具会被高阶 Actor 杀死；同阶/低阶杀不死玩家
                if (atk > playerDefense)
                {
                    player.Kill();
                    return;
                }
            }
        }
    }
}
