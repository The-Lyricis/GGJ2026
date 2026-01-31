using UnityEngine;
namespace GGJ2026
{
    public class PlayerActor : BaseActor
    {
        [Header("Mask/Control")]
        [SerializeField] private FactionColor controlColor = FactionColor.White;

        [Header("Minds")]
        [SerializeField] private PlayerMind playerMind;
        [SerializeField] private SymbiosisMind symbiosisMind;

        public override FactionColor ControlColor => controlColor;
        public override FactionColor CombatColor => controlColor;

        public void EquipMask(FactionColor color)
        {
            controlColor = color;

            // 切到共生 Mind
            if (symbiosisMind != null)
                SetMind(symbiosisMind);
        }

        public void UnequipMask()
        {
            controlColor = FactionColor.White;

            // 切回玩家 Mind
            if (playerMind != null)
                SetMind(playerMind);
        }
    }

}