using UnityEngine;

namespace CUBIE
{
    public class PlayerInputSource : MonoBehaviour, IInputSource
    {
        public MoveDir ReadDownDir()
        {
            if (Input.GetKeyDown(KeyCode.W)) return MoveDir.Up;
            if (Input.GetKeyDown(KeyCode.S)) return MoveDir.Down;
            if (Input.GetKeyDown(KeyCode.A)) return MoveDir.Left;
            if (Input.GetKeyDown(KeyCode.D)) return MoveDir.Right;
            return MoveDir.None;
        }

        public MoveDir ReadHoldDir()
        {
            if (Input.GetKey(KeyCode.W)) return MoveDir.Up;
            if (Input.GetKey(KeyCode.S)) return MoveDir.Down;
            if (Input.GetKey(KeyCode.A)) return MoveDir.Left;
            if (Input.GetKey(KeyCode.D)) return MoveDir.Right;
            return MoveDir.None;
        }
    }
}
