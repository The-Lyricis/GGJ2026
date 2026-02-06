namespace CUBIE
{
    public interface IInputSource
    {
        MoveDir ReadDownDir();
        MoveDir ReadHoldDir();
    }
}
