using UnityEngine;

namespace CUBIE
{
    public static class MoveUtil
    {
        public static Vector2Int DirToDelta(MoveDir dir)
        {
            return dir switch
            {
                MoveDir.Up => Vector2Int.up,
                MoveDir.Down => Vector2Int.down,
                MoveDir.Left => Vector2Int.left,
                MoveDir.Right => Vector2Int.right,
                _ => Vector2Int.zero
            };
        }

        public static int Projection(Vector2Int cell, Vector2Int delta)
        {
            return cell.x * delta.x + cell.y * delta.y;
        }
    }
}
