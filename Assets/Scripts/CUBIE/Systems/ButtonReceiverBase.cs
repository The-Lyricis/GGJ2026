using UnityEngine;

namespace CUBIE
{
    public abstract class ButtonReceiverBase : MonoBehaviour
    {
        [SerializeField] private string[] requiredIds;
        [SerializeField] private bool latch = true;

        private bool isActive;
        private bool latched;

        public string[] RequiredIds => requiredIds;

        protected virtual void OnEnable()
        {
            var system = FindSystem();
            system?.RegisterReceiver(this);
        }

        protected virtual void OnDisable()
        {
            var system = FindSystem();
            system?.UnregisterReceiver(this);
        }

        public void ApplyActive(bool activeThisTurn)
        {
            if (latch && (latched || activeThisTurn))
            {
                latched = true;
                if (!isActive)
                {
                    isActive = true;
                    ApplyInternal(true);
                }
                return;
            }

            if (isActive != activeThisTurn)
            {
                isActive = activeThisTurn;
                ApplyInternal(isActive);
            }
        }

        protected abstract void ApplyInternal(bool active);

        private static ButtonSystem FindSystem()
        {
            var systems = FindObjectsByType<ButtonSystem>(FindObjectsSortMode.None);
            return systems.Length > 0 ? systems[0] : null;
        }
    }
}
