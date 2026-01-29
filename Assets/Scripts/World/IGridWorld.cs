using UnityEngine;

namespace GGJ2026
{
    public interface IGridWorld
    {
        bool InBounds(Vector2Int cell);
        bool IsWall(Vector2Int cell);
        bool IsIce(Vector2Int cell);

        bool IsOccupied(Vector2Int cell);
        BaseActor GetOccupant(Vector2Int cell);

        bool IsMaskCell(Vector2Int cell, out FactionColor color);
        bool IsButtonCell(Vector2Int cell);

        bool IsExitCell(Vector2Int cell);

        void MoveActor(BaseActor actor, Vector2Int toCell);
        Vector2Int GetActorCell(BaseActor actor);

        bool TryConsumeMask(Vector2Int cell, out FactionColor color);
    }
}