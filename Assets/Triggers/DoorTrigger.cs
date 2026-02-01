using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public class DoorTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridWorldBehaviour world;
        [SerializeField] private DoorMarker marker;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer doorRenderer; // optional: auto from self
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;

        [Header("Behavior")]
        [Tooltip("If true, door opens only once; later button events are ignored.")]
        [SerializeField] private bool openOnce = true;

        private bool isOpen;
        private readonly HashSet<string> latchedIds = new();

        private void Awake()
        {
            var p = transform.position;

            float sx = 1;
            float sy = 1;

            float cx = Mathf.Floor(p.x / sx) * sx + sx * 0.5f;
            float cy = Mathf.Floor(p.y / sy) * sy + sy * 0.5f;

            transform.position = new Vector3(cx, cy, 0f);

            if (marker == null) marker = GetComponent<DoorMarker>();
            if (world == null) world = FindFirstObjectByType<GridWorldBehaviour>();
            if (doorRenderer == null) doorRenderer = GetComponent<SpriteRenderer>();

            if (marker == null || world == null || world.GroundTilemap == null)
            {
                Debug.LogError("[DoorTrigger] Missing marker/world/groundTilemap.", this);
                enabled = false;
                return;
            }

            var c = world.GroundTilemap.WorldToCell(transform.position);
            marker.cell = (Vector2Int)c;

            isOpen = !marker.startClosed;
            if (marker.startClosed) Close();
            else Open();
        }

        private void OnEnable()
        {
            ButtonManager.OnButtonLatched += HandleButtonLatched;
        }

        private void OnDisable()
        {
            ButtonManager.OnButtonLatched -= HandleButtonLatched;
        }

        private void HandleButtonLatched(string id, BaseActor actor, Vector2Int cell)
        {
            if (marker == null || marker.listenIds == null || marker.listenIds.Count == 0) return;
            if (string.IsNullOrWhiteSpace(id)) return;
            if (isOpen && openOnce) return;

            if (!marker.listenIds.Contains(id)) return;

            latchedIds.Add(id);

            if (AllRequiredLatched())
                Open();
        }

        private bool AllRequiredLatched()
        {
            for (int i = 0; i < marker.listenIds.Count; i++)
            {
                var req = marker.listenIds[i];
                if (string.IsNullOrWhiteSpace(req)) continue;
                if (!latchedIds.Contains(req)) return false;
            }
            return true;
        }

        public void Open()
        {
            if (world == null || marker == null) return;
            if (isOpen) return;

            world.RemoveBlocks(marker.cell);

            isOpen = true;
            ApplyVisual(true);
        }

        public void Close()
        {
            if (world == null || marker == null) return;
            if (!isOpen) return;

            world.AddBlocks(marker.cell);

            isOpen = false;
            ApplyVisual(false);
        }

        private void ApplyVisual(bool open)
        {
            if (doorRenderer == null) return;

            if (open)
            {
                if (openSprite != null)
                {
                    doorRenderer.enabled = true;
                    doorRenderer.sprite = openSprite;
                }
                else
                {
                    doorRenderer.enabled = false;
                }
            }
            else
            {
                doorRenderer.enabled = true;
                if (closedSprite != null)
                    doorRenderer.sprite = closedSprite;
            }
        }
    }
}
