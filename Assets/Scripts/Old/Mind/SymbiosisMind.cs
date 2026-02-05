using UnityEngine;

namespace GGJ2026
{
    public class SymbiosisMind : MonoBehaviour, IMind
    {
        [SerializeField] private PlayerMind playerInput; // 读取玩家输入
        [SerializeField] private TurnSystem turnSystem;  // 或注入 controller/ctx 的入口

        public MoveIntent ReadMoveIntent()
        {
            // 注意：SymbiosisMind 本身仍“只产出意图”
            // 广播发生在 TurnSystem/SymbiosisController，不在 Mind 内部做副作用，便于测试与回放。
            if (playerInput == null) return MoveIntent.None;
            return playerInput.ReadMoveIntent();
        }
    }
}