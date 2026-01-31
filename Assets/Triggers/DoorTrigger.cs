using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public class DoorTrigger : MonoBehaviour
    {

        [Header("Behavior")]
        [SerializeField] private bool toggleOnTrigger = true;
        [SerializeField] private bool openOnTrigger = true;

        [Header("References")]
        [SerializeField] private GridWorldBehaviour world;   
        [SerializeField] private DoorMarker marker;          
        private GameObject doorVisual;      

        private bool isOpen;

        private void Awake()
        {
            if (marker == null) marker = GetComponent<DoorMarker>();
            if (world == null) world = FindFirstObjectByType<GridWorldBehaviour>();

            if (marker == null || world == null || world.GroundTilemap == null)
            {
                Debug.LogError("[DoorTrigger] Missing marker/world/groundTilemap.", this);
                enabled = false;
                return;
            }

            // 1) 先计算并写入 cell（避免 Open/Close 用错格子）
            var c = world.GroundTilemap.WorldToCell(transform.position);
            marker.cell = (Vector2Int)c;

            // 2) 再按 startClosed 应用初始状态（会正确 Add/Remove 到该 cell）
            isOpen = !marker.startClosed;
            if (marker.startClosed) Close();
            else Open();
            

            ApplyVisual(isOpen);
        }

        private readonly HashSet<string> activeIds = new();

        private void OnEnable()
        {
            ButtonManager.OnButtonSignal += HandleSignal;
        }

        private void OnDisable()
        {
            ButtonManager.OnButtonSignal -= HandleSignal;
        }


        private readonly HashSet<string> pressed = new();

        private void HandleSignal(string id, bool active, BaseActor actor, Vector2Int cell)
        {
            if (marker == null || marker.listenIds == null || marker.listenIds.Count == 0) return;
            if (!marker.listenIds.Contains(id)) return;

            if (active) activeIds.Add(id);
            else activeIds.Remove(id);

            // AND：全部激活才开门，否则关门
            if (AllRequiredActive()) Open();
            else Close();
        }
        
        private bool AllRequiredActive()
        {
            for (int i = 0; i < marker.listenIds.Count; i++)
            {
                var req = marker.listenIds[i];
                if (string.IsNullOrWhiteSpace(req)) continue;
                if (!activeIds.Contains(req)) return false;
            }
            return true;
        }

        public void Open()
        {
            if (marker == null || marker.cell == null) return;

            // 逻辑：移除阻挡
            world?.RemoveBlocks(marker.cell);

            isOpen = true;
            ApplyVisual(isOpen);
        }

        public void Close()
        {
            if (marker == null || marker.cell == null) return;

            // 逻辑：添加阻挡
            world?.AddBlocks(marker.cell);

            isOpen = false;
            ApplyVisual(isOpen);

        }

        private void ApplyVisual(bool open)
        {
            // 你可以按需求替换为 Animator 参数、SpriteRenderer 切图、播放音效等
            var v = doorVisual != null ? doorVisual : gameObject;

            // 例：开门 = 隐藏门外观（或播放开门动画）
            v.GetComponent<Renderer>().enabled = !open;
        }
    }
}
