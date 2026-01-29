using UnityEngine;

namespace GGJ2026
{
    public enum MoveDir { None, Up, Down, Left, Right }

    public struct MoveIntent
    {
        public MoveDir dir;
        public static MoveIntent None => new MoveIntent { dir = MoveDir.None };
    }

    public static class MoveUtil
    {
        public static Vector2Int DirToDelta(MoveDir dir) => dir switch
        {
            MoveDir.Up => Vector2Int.up,
            MoveDir.Down => Vector2Int.down,
            MoveDir.Left => Vector2Int.left,
            MoveDir.Right => Vector2Int.right,
            _ => Vector2Int.zero
        };
    }
}
