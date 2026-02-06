using System.Collections.Generic;
using UnityEngine;

namespace CUBIE
{
    public class ButtonSystem : MonoBehaviour
    {
        private readonly HashSet<string> pressedThisTurn = new();
        private readonly HashSet<string> latchedButtons = new();
        private readonly List<ButtonReceiverBase> receivers = new();

        private void OnEnable()
        {
            ButtonTrigger.OnPressed += HandlePressed;
        }

        private void OnDisable()
        {
            ButtonTrigger.OnPressed -= HandlePressed;
        }

        public void RegisterReceiver(ButtonReceiverBase receiver)
        {
            if (receiver == null) return;
            if (!receivers.Contains(receiver)) receivers.Add(receiver);
        }

        public void UnregisterReceiver(ButtonReceiverBase receiver)
        {
            if (receiver == null) return;
            receivers.Remove(receiver);
        }

        private void HandlePressed(ButtonTrigger trigger, BaseActor actor)
        {
            if (trigger == null) return;
            if (!string.IsNullOrEmpty(trigger.Id))
                pressedThisTurn.Add(trigger.Id);

            if (trigger.Latch && !string.IsNullOrEmpty(trigger.Id))
                latchedButtons.Add(trigger.Id);
        }

        public void ResolveAndClear()
        {
            for (int i = 0; i < receivers.Count; i++)
            {
                var r = receivers[i];
                if (r == null) continue;
                bool satisfied = AreRequirementsSatisfied(r.RequiredIds);
                r.ApplyActive(satisfied);
            }

            pressedThisTurn.Clear();
        }

        private bool AreRequirementsSatisfied(string[] requiredIds)
        {
            if (requiredIds == null || requiredIds.Length == 0) return false;
            for (int i = 0; i < requiredIds.Length; i++)
            {
                var id = requiredIds[i];
                if (string.IsNullOrEmpty(id)) return false;
                if (!pressedThisTurn.Contains(id) && !latchedButtons.Contains(id))
                    return false;
            }
            return true;
        }
    }
}
