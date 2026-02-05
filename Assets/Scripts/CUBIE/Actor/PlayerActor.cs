using UnityEngine;

namespace CUBIE
{
    public class PlayerActor:BaseActor
    {
        [SerializeField] private ActorType controlType = ActorType.Player;

        public override ActorType ActorType => ActorType.Player;

        public ActorType ControlType
        {
            get => controlType;
            set => SetControlType(value);
        }

        public event System.Action<ActorType> OnControlTypeChanged;

        public void SetControlType(ActorType value)
        {
            if (controlType == value) return;
            controlType = value;
            OnControlTypeChanged?.Invoke(controlType);
        }
    }
}
