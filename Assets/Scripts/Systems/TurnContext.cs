using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public class TurnContext
    {
        public FactionColor playerControlColor;

        // intents
        private readonly Dictionary<BaseActor, MoveIntent> intents = new();

        // Planned moves for this turn
        private readonly Dictionary<BaseActor, Vector2Int> pendingMoves = new();

        // Start-of-turn occupancy snapshot
        private readonly Dictionary<Vector2Int, BaseActor> occupancySnapshot = new();

        // Reserved target cells for this turn
        private readonly Dictionary<Vector2Int, BaseActor> reservedTargets = new();

        public void BuildSnapshot(List<BaseActor> actors, IGridWorld world)
        {
            occupancySnapshot.Clear();
            reservedTargets.Clear();

            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (a == null || !a.IsAlive) continue;

                var cell = world.GetActorCell(a);
                occupancySnapshot[cell] = a;
            }
        }

        public bool TryGetPlannedMove(BaseActor a, out Vector2Int cell)
            => pendingMoves.TryGetValue(a, out cell);

        public void ClearPlannedMoves()
        {
            pendingMoves.Clear();
            reservedTargets.Clear();
        }

        public void SetIntent(BaseActor a, MoveIntent i) => intents[a] = i;
        public MoveIntent GetIntent(BaseActor a)
            => intents.TryGetValue(a, out var i) ? i : MoveIntent.None;

        public void QueueMove(BaseActor a, Vector2Int cell)
        {
            pendingMoves[a] = cell;
            reservedTargets[cell] = a;
        }

        /// <summary>
        /// Resolve a single actor move for this turn.
        /// </summary>
        public Vector2Int ResolveMovement(BaseActor a, Vector2Int from, MoveDir dir, IGridWorld world)
        {
            if (dir == MoveDir.None) return from;

            var delta = MoveUtil.DirToDelta(dir);
            if (delta == Vector2Int.zero) return from;

            bool IsBlocked(Vector2Int cell, MoveDir moveDir, FactionColor color, FactionColor combatColor)
            {
                if (!world.InBounds(cell) || world.IsBlocked(cell)) return true;

                // reserved by someone else this turn
                if (reservedTargets.TryGetValue(cell, out var r) && r != null && r.IsAlive)
                    return true;

                // occupied at turn start
                if (occupancySnapshot.TryGetValue(cell, out var occ) && occ != null && occ.IsAlive)
                {
                    // green is always blocking / cannot be entered
                    if (occ.CombatColor == FactionColor.Green || combatColor == FactionColor.Green)
                        return true;

                    // same control color: queue-follow rule
                    if (occ.ControlColor == color)
                    {
                        // if the one in front is not moving in the same direction, block
                        if (GetIntent(occ).dir != moveDir) return true;

                        // if front actor already has a planned move and will leave this cell, allow
                        if (pendingMoves.TryGetValue(occ, out var occTarget))
                        {
                            if (occTarget != cell) return false;
                        }

                        return true;
                    }

                    // different color occupant blocks movement (combat happens after overlap in your design,
                    // but you currently still treat occupied start cells as blocking unless same-color-follow allows it)
                    return true;
                }

                return false;
            }

            bool IsMechanism(Vector2Int cell)
                => world.IsButtonCell(cell) || world.IsMaskCell(cell, out _);

            var color = a.ControlColor;
            var combat = a.CombatColor;

            var next = from + delta;

            // Normal ground movement
            if (!world.IsIce(from) && !world.IsIce(next))
            {
                return IsBlocked(next, dir, color, combat) ? from : next;
            }

            // Entering ice: if first ice cell is blocked, stay
            if (!world.IsIce(from) && world.IsIce(next) && IsBlocked(next, dir, color, combat))
            {
                return from;
            }

            // Ice sliding
            var current = from;
            while (true)
            {
                var probe = current + delta;

                if (IsBlocked(probe, dir, color, combat)) return current;
                if (IsMechanism(probe)) return probe;

                current = probe;
                if (!world.IsIce(current)) return current;
            }
        }

        public float ApplyMoves(IGridWorld world)
        {
            float max = 0f;
            foreach (var kv in pendingMoves)
            {
                if (!kv.Key.IsAlive) continue;
                float d = world.MoveActor(kv.Key, kv.Value);
                if (d > max) max = d;
            }
            pendingMoves.Clear();
            return max;
        }

        /// <summary>
        /// Post-move triggers (Latch-only buttons, masks, exit).
        /// Buttons affect next turn in this simplified version.
        /// </summary>
        public void ResolveTriggers(IGridWorld world, List<BaseActor> actors)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (a == null || !a.IsAlive) continue;

                var cell = world.GetActorCell(a);
                
                // kill floor
                if (world.IsKillFloor(cell))
                {
                    a.Kill();
                    continue; // 死了就不再处理面具/按钮/出口
                }
                // mask
                if (a is PlayerActor player)
                {
                    if (world.IsMaskCell(cell, out var color) && world.TryConsumeMask(cell, out color))
                        player.EquipMask(color);
                }

                // button (Latch-only): current cell only
                TriggerButtonIfAny(world, a, cell);

                // exit
                if (a is PlayerActor && world.IsExitCell(cell))
                    LevelManager.Instance?.LoadNextLevel();
            }
        }

        private static void TriggerButtonIfAny(IGridWorld world, BaseActor a, Vector2Int cell)
        {
            if (world is not GridWorldBehaviour gw) return;
            if (!gw.TryGetButton(cell, out var def)) return;
            if (def == null || !def.IsAllowed(a)) return;

            ButtonManager.SignalLatch(def.id, a, cell);
        }

        
    }
}
