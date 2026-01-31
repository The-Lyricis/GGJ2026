using UnityEngine;

namespace GGJ2026
{
    public class RedActor : BaseActor
    {
        [SerializeField] private FactionColor controlColor = FactionColor.Red;

        public override FactionColor ControlColor
        {
            get => controlColor;
            set => controlColor = value;
        }

        public override FactionColor CombatColor => FactionColor.Red;
    }
}
