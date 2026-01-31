using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public class TurnContext
    {
        public FactionColor playerControlColor;

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
                if (!a.IsAlive) continue;
                var cell = world.GetActorCell(a);
                occupancySnapshot[cell] = a;
            }
        }

        public bool TryGetPlannedMove(BaseActor a, out Vector2Int cell) => pendingMoves.TryGetValue(a, out cell);

        public void ClearPlannedMoves()
        {
            pendingMoves.Clear();
            reservedTargets.Clear();
        }

        public void SetIntent(BaseActor a, MoveIntent i) => intents[a] = i;
        public MoveIntent GetIntent(BaseActor a) => intents.TryGetValue(a, out var i) ? i : MoveIntent.None;

        public void QueueMove(BaseActor a, Vector2Int cell)
        {
            pendingMoves[a] = cell;
            reservedTargets[cell] = a;
        }


        // Resolve a single actor move for this turn
        public Vector2Int ResolveMovement(BaseActor a, Vector2Int from, MoveDir dir, IGridWorld world)
        {
            if (dir == MoveDir.None) return from;

            var delta = MoveUtil.DirToDelta(dir);
            if (delta == Vector2Int.zero) return from;

            // Block check uses snapshot + reserved targets
            bool IsBlocked(Vector2Int cell, MoveDir moveDir, FactionColor color, FactionColor combatColor)
            {
                if (!world.InBounds(cell) || world.IsWall(cell)) return true;

                if (reservedTargets.TryGetValue(cell, out var r) && r != null && r.IsAlive)
                    return true;

                if (occupancySnapshot.TryGetValue(cell, out var occ) && occ != null && occ.IsAlive)
                {
                    if (occ.CombatColor == FactionColor.Green || combatColor == FactionColor.Green)
                        return true;

                    if (occ.ControlColor == color)
                    {
                        if (GetIntent(occ).dir != moveDir) return true;

                        if (pendingMoves.TryGetValue(occ, out var occTarget))
                        {
                            if (occTarget != cell) return false; // 前方会离开
                        }

                        return true;
                    }

                }

                return false;
            }

            // Buttons/masks are treated as mechanisms
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

            // Entering ice: if the first ice cell is blocked, stay
            if (!world.IsIce(from) && world.IsIce(next) && IsBlocked(next, dir, color, combat))
            {
                return from;
            }

            // Ice sliding (or sliding after entering ice)
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

                if (a is PlayerActor player)
                {
                    if (world.IsMaskCell(cell, out var color) && world.TryConsumeMask(cell, out color))
                    {

                        player.EquipMask(color); // 切换控制色/战斗色 + 切 Mind
                    }
                }

                if (world.IsButtonCell(cell))
                {
                }

                if (a is PlayerActor && world.IsExitCell(cell))
                {
                    Debug.LogWarning("Player is on exit");
                }
            }
        }



    }
}
