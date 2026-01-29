using UnityEngine;
namespace GGJ2026
{
    public class PlayerActor : BaseActor
    {
        [Header("Mask/Control")]
        [SerializeField] private FactionColor controlColor = FactionColor.White;

        public override FactionColor ControlColor => controlColor;

        // 共生免死（战斗判定色跟随控制色）
        public override FactionColor CombatColor => controlColor;

        public void EquipMask(FactionColor color)
        {
            controlColor = color;
            // TODO: 切换 Mind（PlayerMind / SymbiosisMind）由外部系统或此处执行
        }

        public void UnequipMask()
        {
            controlColor = FactionColor.White;
            // TODO: 切换回 PlayerMind
        }
    }

}