using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public class GridWorldBehaviour : MonoBehaviour, IGridWorld
    {
        // TODO: 替换为 Tilemap
        private readonly Dictionary<BaseActor, Vector2Int> actorCell = new();
        private readonly Dictionary<Vector2Int, BaseActor> occupancy = new();

        public bool InBounds(Vector2Int cell) => true; // TODO
        public bool IsWall(Vector2Int cell) => false;  // TODO
        public bool IsIce(Vector2Int cell) => false;   // TODO

        public bool IsOccupied(Vector2Int cell) => occupancy.ContainsKey(cell);
        public BaseActor GetOccupant(Vector2Int cell) => occupancy.TryGetValue(cell, out var a) ? a : null;

        public bool IsMaskCell(Vector2Int cell, out FactionColor color) { color = FactionColor.White; return false; } // TODO
        public bool IsButtonCell(Vector2Int cell) => false; // TODO

        public void MoveActor(BaseActor actor, Vector2Int toCell)
        {
            // 基础占位更新：后续会在 TurnContext.ApplyMoves 统一调用它
            var from = GetActorCell(actor);
            occupancy.Remove(from);
            occupancy[toCell] = actor;
            actorCell[actor] = toCell;
        }

        public Vector2Int GetActorCell(BaseActor actor)
            => actorCell.TryGetValue(actor, out var c) ? c : Vector2Int.zero;

        public void RegisterActor(BaseActor actor, Vector2Int cell)
        {
            actorCell[actor] = cell;
            occupancy[cell] = actor;
        }

        public void UnregisterActor(BaseActor actor)
        {
            var cell = GetActorCell(actor);
            occupancy.Remove(cell);
            actorCell.Remove(actor);
        }
    }

}