using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

        public void QueueMove(BaseActor a, Vector2Int cell)
        {
            pendingMoves[a] = cell;
            Debug.Log($"[QueueMove] {a.name} -> {cell}");
        }

        public Vector2Int ResolveMovement(BaseActor a, Vector2Int from, MoveDir dir, IGridWorld world)
        {
            if (dir == MoveDir.None) return from;

            var delta = MoveUtil.DirToDelta(dir);
            if (delta == Vector2Int.zero) return from;

            bool CanShiftChain(Vector2Int cell, MoveDir chainDir, FactionColor color)
            {
                var step = MoveUtil.DirToDelta(chainDir);
                var check = cell;

                while (true)
                {
                    if (!occupancySnapshot.TryGetValue(check, out var occ)) return true;
                    if (occ == null || !occ.IsAlive) return true;

                    if (occ.ControlColor != color) return false;      // 同色才允许排队
                    if (GetIntent(occ).dir != chainDir) return false; // 队列方向一致

                    check += step;
                    if (!world.InBounds(check) || world.IsWall(check)) return false;

                    if (world.IsButtonCell(check)) return true;
                    if (world.IsMaskCell(check, out _)) return true;
                }
            }
            // 判定目标格子是否阻，包括占位和墙
            bool IsBlocked(Vector2Int cell, MoveDir moveDir, FactionColor color, FactionColor combatColor)
            {
                // 墙或越界阻挡
                if (!world.InBounds(cell) || world.IsWall(cell)) return true;
                
                // 占位阻挡
                if (occupancySnapshot.TryGetValue(cell, out var occ) && occ != null && occ.IsAlive)
                {
                    // 中立阻挡（绿）
                    if (occ.CombatColor == FactionColor.Green || combatColor == FactionColor.Green)
                        return true;

                    // 同色阻挡（但允许排队推进）
                    if (occ.ControlColor == color)
                    {
                        if (GetIntent(occ).dir != moveDir) return true;
                        return !CanShiftChain(cell, moveDir, color);
                    }

                    // 异色允许进入（重合结算）
                    return false;
                }

                return false;
            }

            bool IsMechanism(Vector2Int cell)
                => world.IsButtonCell(cell) || world.IsMaskCell(cell, out _);

            var color = a.ControlColor;
            var combat = a.CombatColor;

            var next = from + delta;

            // ????????????
            if (!world.IsIce(from) && !world.IsIce(next))
            {
                return IsBlocked(next, dir, color, combat) ? from : next;
            }

            // ????????????????????????????? next ?????
            if (!world.IsIce(from) && world.IsIce(next) && IsBlocked(next, dir, color, combat))
            {
                return from;
            }

            // ????????????????? next ?????
            // if (!world.IsIce(from) && world.IsIce(next)) { ... }
            
            //统一划在冰面外第一格子上停止的写法
            // var current = from;
            // while (true)
            // {
            //     var probe = current + delta;
            //     if (IsBlocked(probe, dir, color, combat)) return current;

            //     if (IsMechanism(probe)) return probe;

            //     current = probe;
            //     if (!world.IsIce(current)) return current;
            // }

            // ===== 冰面排队滑行（队列出冰）=====

            // 1) 找到冰面上的队头（只在冰面内前移）
            // ??????????????? next ????
            var startCell = world.IsIce(from) ? from : next;
            Vector2Int headCell = startCell;
            while (true)
            {
                var ahead = headCell + delta;

                // 只在冰面上继续前移
                if (!world.IsIce(ahead)) break;

                if (!occupancySnapshot.TryGetValue(ahead, out var occ) || occ == null || !occ.IsAlive) break;
                if (occ.ControlColor != color) break;
                if (GetIntent(occ).dir != dir) break;

                headCell = ahead;
            }

            // 2) 从队头往后收集“冰面上的”队列
            List<BaseActor> chain = new();
            var cur = headCell;
            while (true)
            {
                if (!world.IsIce(cur)) break;

                if (!occupancySnapshot.TryGetValue(cur, out var occ) || occ == null || !occ.IsAlive) break;
                if (occ.ControlColor != color) break;
                if (GetIntent(occ).dir != dir) break;

                chain.Add(occ);

                var back = cur - delta;
                if (!world.IsIce(back)) break;

                if (!occupancySnapshot.TryGetValue(back, out var occBack) || occBack == null || !occBack.IsAlive) break;
                if (occBack.ControlColor != color) break;
                if (GetIntent(occBack).dir != dir) break;

                cur = back;
            }


            // 3) 计算队头滑到哪里（冰外第一格 or 被挡停下）
            Vector2Int headTarget = headCell;
            var slide = headCell;
            while (true)
            {
                var probe = slide + delta;
                if (IsBlocked(probe, dir, color, combat))
                {
                    headTarget = slide;
                    break;
                }

                if (IsMechanism(probe))
                {
                    headTarget = probe;
                    break;
                }

                slide = probe;
                if (!world.IsIce(slide))
                {
                    headTarget = slide;
                    break;
                }
            }

            // 队头不动 -> 整队不动
            if (headTarget == headCell) return from;

            // 4) 给队列分配格子（队头出冰，其余排在后面冰上）
            int idx = chain.IndexOf(a);
            if (idx < 0) return from;

            var target = headTarget - (delta * idx);

            // 队尾只能排在冰面上（不能跑到冰外）
            if (idx > 0 && !world.IsIce(target)) return from;

            if (!world.InBounds(target) || world.IsWall(target)) return from;
            return target;
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

        public void ResolveTriggers(IGridWorld world, List<BaseActor> actors)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (!a.IsAlive) continue;

                var cell = world.GetActorCell(a);

                // 1) 面具（玩家）
                if (a is PlayerActor player)
                {
                    if (world.IsMaskCell(cell, out var color) && world.TryConsumeMask(cell, out color))
                    {

                        player.EquipMask(color); // 切换控制色/战斗色 + 切 Mind
                        // TODO: 如果要“旧面具回原位”，由 MaskManager/World 处理
                    }
                }

                // 2) 按钮（玩家+同色NPC）
                if (world.IsButtonCell(cell))
                {
                    // TODO: 触发按钮逻辑（可通知 ButtonManager）
                    Debug.LogWarning($"{a.name} triggered a button at {cell}");
                }

                // 3) 出口（玩家）
                if (a is PlayerActor && world.IsExitCell(cell))
                {
                    // TODO: 关卡胜利
                    Debug.LogWarning($"Player reached exit at {cell}");
                }
            }
        }

    }
}
