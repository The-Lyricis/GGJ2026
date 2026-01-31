using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace GGJ2026
{
     /// <summary>
    /// Data definition for a button tile.
    /// Placed on Marker Tilemap; runtime uses tile->def mapping.
    /// </summary>
    [System.Serializable]
    public class ButtonDef
    {
        [Tooltip("Which TileBase on MarkerTilemap represents this button.")]
        public TileBase tile;

        [Tooltip("Logical id used by receivers (doors/platforms). Must be unique.")]
        public string id;

        [Tooltip("Button type: Normal triggers by anyone; ColorOnly checks allowColors.")]
        public ButtonType type = ButtonType.Normal;

        [Tooltip("Allowed colors when type == ColorOnly.")]
        public List<FactionColor> allowColors = new();

        [Tooltip("If true, only triggers when an actor enters the cell (edge trigger). " +
                 "If false, triggers every turn while standing on the cell.")]
        public bool triggerOnEnterOnly = false;

        // ===== Runtime cache (optional but recommended) =====
        [System.NonSerialized] private HashSet<FactionColor> allowSet;

        /// <summary>
        /// Build runtime cache for fast Contains checks.
        /// Call from GridWorldBehaviour.BuildTileCaches().
        /// </summary>
        public void BuildRuntimeCache()
        {
            if (type != ButtonType.ColorOnly)
            {
                allowSet = null;
                return;
            }

            allowSet = new HashSet<FactionColor>();
            if (allowColors == null) return;

            for (int i = 0; i < allowColors.Count; i++)
                allowSet.Add(allowColors[i]);
        }

        /// <summary>
        /// Check whether the given actor is allowed to trigger this button.
        /// By default uses actor.ControlColor (fits mask/control design).
        /// </summary>
        public bool IsAllowed(BaseActor actor)
        {
            if (actor == null) return false;

            if (type == ButtonType.Normal) return true;

            // ColorOnly
            if (allowSet == null) BuildRuntimeCache();
            return allowSet != null && allowSet.Contains(actor.ControlColor);
        }
    }
}