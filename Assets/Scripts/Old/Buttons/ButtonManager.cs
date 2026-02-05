using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    /// <summary>
    /// Central event bus for button triggers.
    /// Latch-only: a button id can only be latched once until ResetAll() is called.
    /// </summary>
    public static class ButtonManager
    {
        /// <summary>
        /// Fired when a button id is latched for the first time.
        /// </summary>
        public static event Action<string, BaseActor, Vector2Int> OnButtonLatched;

        private static readonly HashSet<string> latched = new();

        /// <summary>
        /// Clear all latched states (call on level restart).
        /// </summary>
        public static void ResetAll()
        {
            latched.Clear();
        }

        /// <summary>
        /// Try to latch a button id. Returns true only on the first latch.
        /// </summary>
        public static bool SignalLatch(string id, BaseActor actor, Vector2Int cell)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            if (!latched.Add(id)) return false;

            OnButtonLatched?.Invoke(id, actor, cell);
            return true;
        }

        public static bool IsLatched(string id) => !string.IsNullOrWhiteSpace(id) && latched.Contains(id);
    }
}
