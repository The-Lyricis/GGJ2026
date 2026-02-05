using System.Collections.Generic;
using UnityEngine;

namespace CUBIE
{
    public class MovementResolver
    {
        private Dictionary<BaseActor, Vector2Int> startCellByActor;
        private Dictionary<Vector2Int, BaseActor> startOccupancy;
        private Dictionary<BaseActor, Vector2Int> plannedByActor;
        private Dictionary<Vector2Int, BaseActor> reservedByCell;

        public void ResolveAll(TurnContext ctx, IGridWorld world, IReadOnlyList<BaseActor> actors)
        {
            if (ctx == null || world == null || actors == null) return;

            startCellByActor = new Dictionary<BaseActor, Vector2Int>();
            startOccupancy = new Dictionary<Vector2Int, BaseActor>();
            plannedByActor = new Dictionary<BaseActor, Vector2Int>();
            reservedByCell = new Dictionary<Vector2Int, BaseActor>();

            ctx.ClearPlannedMoves();

            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (a == null || !a.IsAlive) continue;
                var cell = world.GetActorCell(a);
                startCellByActor[a] = cell;
                startOccupancy[cell] = a;
                ctx.SetBlocked(a, false);
            }

            // 1) 先预占不移动的 Actor
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (a == null || !a.IsAlive) continue;
                var intent = ctx.GetIntent(a);
                if (intent.dir != MoveDir.None) continue;

                if (!startCellByActor.TryGetValue(a, out var cell))
                    cell = world.GetActorCell(a);

                plannedByActor[a] = cell;
                reservedByCell[cell] = a;
                ctx.QueueMove(a, cell);
                ctx.SetBlocked(a, false);
            }

            // 2) 按方向分组并排序
            ResolveByDirection(MoveDir.Up, ctx, world, actors);
            ResolveByDirection(MoveDir.Down, ctx, world, actors);
            ResolveByDirection(MoveDir.Left, ctx, world, actors);
            ResolveByDirection(MoveDir.Right, ctx, world, actors);
        }

        public Vector2Int ResolveActor(BaseActor actor, MoveIntent intent, TurnContext ctx, IGridWorld world)
        {
            if (actor == null || world == null) return Vector2Int.zero;
            if (intent.dir == MoveDir.None) return GetStartCell(actor, world);

            var delta = MoveUtil.DirToDelta(intent.dir);
            if (delta == Vector2Int.zero) return GetStartCell(actor, world);

            var current = GetStartCell(actor, world);
            var target = current + delta;

            bool fromIce = IsIce(world, current);
            bool nextIce = IsIce(world, target);

            // Normal ground movement
            if (!fromIce && !nextIce)
                return IsBlocked(target, actor, world) ? current : target;

            // Entering ice: if first ice cell blocked, stay
            if (!fromIce && nextIce && IsBlocked(target, actor, world))
                return current;

            // Ice sliding
            var cursor = current;
            while (true)
            {
                var probe = cursor + delta;
                if (IsBlocked(probe, actor, world)) return cursor;

                cursor = probe;
                if (!IsIce(world, cursor)) return cursor;
            }
        }

        private void ResolveByDirection(MoveDir dir, TurnContext ctx, IGridWorld world, IReadOnlyList<BaseActor> actors)
        {
            var delta = MoveUtil.DirToDelta(dir);
            if (delta == Vector2Int.zero) return;

            var list = new List<BaseActor>();
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (a == null || !a.IsAlive) continue;
                var intent = ctx.GetIntent(a);
                if (intent.dir == dir) list.Add(a);
            }

            list.Sort((a, b) =>
            {
                var ca = GetStartCell(a, world);
                var cb = GetStartCell(b, world);
                var pa = MoveUtil.Projection(ca, delta);
                var pb = MoveUtil.Projection(cb, delta);
                return pb.CompareTo(pa); // front-to-back
            });

            for (int i = 0; i < list.Count; i++)
            {
                var actor = list[i];
                if (plannedByActor.ContainsKey(actor)) continue;

                var intent = ctx.GetIntent(actor);
                var target = ResolveActor(actor, intent, ctx, world);
                var blocked = IsBlocked(target, actor, world);
                if (blocked)
                    target = GetStartCell(actor, world);

                plannedByActor[actor] = target;
                reservedByCell[target] = actor;
                ctx.QueueMove(actor, target);
                ctx.SetBlocked(actor, blocked);
            }
        }

        private Vector2Int GetStartCell(BaseActor actor, IGridWorld world)
        {
            if (actor == null || world == null) return Vector2Int.zero;
            if (startCellByActor != null && startCellByActor.TryGetValue(actor, out var cell)) return cell;
            return world.GetActorCell(actor);
        }

        private bool IsBlocked(Vector2Int cell, BaseActor mover, IGridWorld world)
        {
            if (world == null) return true;
            if (!world.HasGround(cell)) return true;
            if (world.GetBlock(cell) != BlockType.None) return true;
            if (!CanEnterSurface(mover, world.GetSurface(cell))) return true;

            if (reservedByCell != null && reservedByCell.TryGetValue(cell, out var r) && r != null && r != mover)
                return true;

            if (startOccupancy != null && startOccupancy.TryGetValue(cell, out var occ) && occ != null && occ != mover)
            {
                if (plannedByActor != null && plannedByActor.TryGetValue(occ, out var occTarget))
                {
                    if (occTarget != cell) return false; // occupant will leave
                    return true;
                }

                return true;
            }

            return false;
        }

        private static bool IsIce(IGridWorld world, Vector2Int cell)
        {
            return world != null && world.GetSurface(cell) == SurfaceType.Ice;
        }

        private bool CanEnterSurface(BaseActor actor, SurfaceType surface)
        {
            // Movement-only rule: all surfaces are enterable.
            // Death/transform effects are resolved in turn effects.
            return true;
        }
    }
}
