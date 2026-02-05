using UnityEngine;

namespace CUBIE
{
    public enum BlockType
    {
        None,
        Wall,
        Stone,
        Wood,
    }
    public enum SurfaceType
    {
        None,
        Ice,
        Water,
        Lava,
        Pit
    }

    public interface IGridWorld
    {
        public bool HasGround(Vector2Int cell);
        BlockType GetBlock(Vector2Int cell);

        SurfaceType GetSurface(Vector2Int cell);

        void SetBlock(Vector2Int cell, BlockType type);
        void SetSurface(Vector2Int cell, SurfaceType type);

        void RegisterActor(BaseActor actor, Vector2Int cell);
        void UnregisterActor(BaseActor actor);

        Vector2Int GetActorCell(BaseActor actor);
        float MoveActor(BaseActor actor, Vector2Int toCell);
    }

}
