using UnityEngine;

namespace CUBIE
{
    public class PlayerInputSource : MonoBehaviour, IInputSource
    {
        public MoveIntent ReadMoveIntent()
        {
            if (Input.GetKeyDown(KeyCode.W)) return new MoveIntent { dir = MoveDir.Up };
            if (Input.GetKeyDown(KeyCode.S)) return new MoveIntent { dir = MoveDir.Down };
            if (Input.GetKeyDown(KeyCode.A)) return new MoveIntent { dir = MoveDir.Left };
            if (Input.GetKeyDown(KeyCode.D)) return new MoveIntent { dir = MoveDir.Right };
            return MoveIntent.None;
        }
    }
}