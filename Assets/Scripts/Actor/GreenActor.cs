using UnityEngine;

namespace GGJ2026
{
    public class GreenActor : BaseActor
    {
        [SerializeField] private FactionColor controlColor = FactionColor.Green;

        public override FactionColor ControlColor
        {
            get => controlColor;
            set => controlColor = value;
        }

        public override FactionColor CombatColor => FactionColor.Green;
    }
}
