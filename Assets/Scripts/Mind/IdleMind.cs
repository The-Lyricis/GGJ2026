using UnityEngine;
namespace GGJ2026
{
    public class IdleMind : MonoBehaviour, IMind
    {
        public MoveIntent ReadMoveIntent() => MoveIntent.None;
    }

}