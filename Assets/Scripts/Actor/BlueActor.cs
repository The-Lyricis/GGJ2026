using UnityEngine;

namespace GGJ2026
{
    public class BlueActor : BaseActor
    {
        [SerializeField] private FactionColor controlColor = FactionColor.Blue;

        public override FactionColor ControlColor
        {
            get => controlColor;
            set => controlColor = value;
        }

        public override FactionColor CombatColor => FactionColor.Blue;
    }
}