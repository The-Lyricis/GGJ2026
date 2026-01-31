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
        [SerializeField] private Renderer doorRenderer; // 可选：不填则自动从自身获取

        [Header("Behavior")]
        [Tooltip("If true, door opens only once; later button events are ignored.")]
        [SerializeField] private bool openOnce = true;

        private bool isOpen;
        private readonly HashSet<string> latchedIds = new();

        private void Awake()
        {
            var p = transform.position;

            // 防止 tileSize 为 0
            float sx = 1;
            float sy = 1;

            // “向下取整到格子起点”再“+半格到中心”
            float cx = Mathf.Floor(p.x / sx) * sx + sx * 0.5f;
            float cy = Mathf.Floor(p.y / sy) * sy + sy * 0.5f;

            transform.position = new Vector3(cx, cy, 0f);

            if (marker == null) marker = GetComponent<DoorMarker>();
            if (world == null) world = FindFirstObjectByType<GridWorldBehaviour>();
            if (doorRenderer == null) doorRenderer = GetComponent<Renderer>();

            if (marker == null || world == null || world.GroundTilemap == null)
            {
                Debug.LogError("[DoorTrigger] Missing marker/world/groundTilemap.", this);
                enabled = false;
                return;
            }

            // 计算门所在格子
            var c = world.GroundTilemap.WorldToCell(transform.position);
            marker.cell = (Vector2Int)c;

            // 初始状态
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

            // 只关心门配置的 ids
            if (!marker.listenIds.Contains(id)) return;

            // 记录已触发的按钮 id（Latch：只增不减）
            latchedIds.Add(id);

            // AND：全部满足才开门
            if (AllRequiredLatched())
                Open();
        }

        private bool AllRequiredLatched()
        {
            for (int i = 0; i < marker.listenIds.Count; i++)
            {
                var req = marker.listenIds[i];
                if (string.IsNullOrWhiteSpace(req)) continue;

                // 如果你希望“门在场景加载时就尊重已经 latched 的按钮状态”
                // 可以改为：if (!(latchedIds.Contains(req) || ButtonManager.IsLatched(req))) return false;
                if (!latchedIds.Contains(req)) return false;
            }
            return true;
        }

        public void Open()
        {
            if (world == null || marker == null) return;
            if (isOpen) return;

            // Vector2Int 是值类型，不要判 null
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
            // 简单表现：开门隐藏渲染（你也可以换 Animator）
            if (doorRenderer != null)
                doorRenderer.enabled = !open;
        }
    }
}
