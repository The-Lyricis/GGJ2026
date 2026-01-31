using System;
using UnityEngine;

namespace GGJ2026
{
    /// <summary>
    /// Central event bus for button triggers.
    /// Static for demo simplicity; receivers subscribe/unsubscribe in OnEnable/OnDisable.
    /// </summary>
    public static class ButtonManager
    {
        /// <summary>
        /// Fired when a button is triggered.
        /// id: logical id of button; actor: who triggered; cell: where it happened.
        /// </summary>
        public static event Action<string, BaseActor, Vector2Int> OnButtonTriggered;

        public static void Trigger(string id, BaseActor actor, Vector2Int cell)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("[ButtonManager] Trigger called with empty id.");
                return;
            }

            OnButtonTriggered?.Invoke(id, actor, cell);
        }
    }
}