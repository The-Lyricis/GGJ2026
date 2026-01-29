using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    using UnityEngine;

    public class PlayerMind : MonoBehaviour, IMind
    {
        public MoveIntent ReadMoveIntent()
        {
            // TODO: 可替换为 Unity Input System
            if (Input.GetKeyDown(KeyCode.W)) return new MoveIntent { dir = MoveDir.Up };
            if (Input.GetKeyDown(KeyCode.S)) return new MoveIntent { dir = MoveDir.Down };
            if (Input.GetKeyDown(KeyCode.A)) return new MoveIntent { dir = MoveDir.Left };
            if (Input.GetKeyDown(KeyCode.D)) return new MoveIntent { dir = MoveDir.Right };
            return MoveIntent.None;
        }
    }
}
