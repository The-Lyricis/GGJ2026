using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    /// <summary>
    /// Central event bus for button triggers.
    /// Static for demo simplicity; receivers subscribe/unsubscribe in OnEnable/OnDisable.
    /// </summary>
    public static class ButtonManager
    {
        public static event Action<string, bool, BaseActor, Vector2Int> OnButtonSignal;

        private static readonly HashSet<string> latched = new();
        private static readonly Dictionary<string, int> heldCounts = new();

        public static void ResetAll()
        {
            latched.Clear();
            heldCounts.Clear();
        }

        public static bool SignalLatch(string id, BaseActor actor, Vector2Int cell)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            if (latched.Contains(id)) return false;

            latched.Add(id);
            OnButtonSignal?.Invoke(id, true, actor, cell);
            return true;
        }

        public static bool SignalHold(string id, bool active, BaseActor actor, Vector2Int cell)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            heldCounts.TryGetValue(id, out var count);
            count = active ? count + 1 : Mathf.Max(0, count - 1);

            bool wasActive = heldCounts.TryGetValue(id, out var prev) && prev > 0;
            bool isActive = count > 0;

            heldCounts[id] = count;

            if (wasActive != isActive)
            {
                OnButtonSignal?.Invoke(id, isActive, actor, cell);
                return true;
            }

            return false;
        }

        // 给 DoorTrigger 或调试用：查询当前是否 active（Latch 或 Hold）
        public static bool IsActive(string id)
            => latched.Contains(id) || (heldCounts.TryGetValue(id, out var c) && c > 0);
    
    }
}
