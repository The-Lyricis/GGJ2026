namespace CUBIE
{
    public enum MoveDir { None, Up, Down, Left, Right }

    public struct MoveIntent
    {
        public MoveDir dir; 
        public static MoveIntent None => new MoveIntent { dir = MoveDir.None };
    }
}