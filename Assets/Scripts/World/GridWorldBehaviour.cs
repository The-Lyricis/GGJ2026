using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGJ2026
{
    [System.Serializable]
    public class MaskTileDef
    {
        public TileBase tile;
        public FactionColor color;
    }

    public class GridWorldBehaviour : MonoBehaviour, IGridWorld
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap wallTilemap;
        [SerializeField] private Tilemap surfaceTilemap;
        [SerializeField] private Tilemap markerTilemap;

        [Header("Surface Tiles")]
        [SerializeField] private List<TileBase> iceTiles = new();

        [Header("Marker Tiles")]
        [SerializeField] private List<TileBase> buttonTiles = new();
        [SerializeField] private List<TileBase> exitTiles = new();
        [SerializeField] private List<MaskTileDef> maskTiles = new();

        // ===== Runtime caches (O(1) lookup) =====
        private HashSet<TileBase> iceSet;
        private HashSet<TileBase> buttonSet;
        private HashSet<TileBase> exitSet;
        private Dictionary<TileBase, FactionColor> maskMap;

        private readonly Dictionary<BaseActor, Vector2Int> actorCell = new();
        private readonly Dictionary<Vector2Int, BaseActor> occupancy = new();

        private void Awake()
        {
            BuildTileCaches();
        }

#if UNITY_EDITOR
        // 在编辑器中修改 Inspector 列表后也能刷新缓存（运行时不频繁调用）
        private void OnValidate()
        {
            // OnValidate 可能在序列化阶段触发，避免依赖运行时对象状态
            BuildTileCaches();
        }
#endif

        private void Start()
        {
            var actors = FindObjectsOfType<BaseActor>();
            for (int i = 0; i < actors.Length; i++)
            {
                var a = actors[i];
                var cell = (Vector2Int)groundTilemap.WorldToCell(a.transform.position);
                RegisterActor(a, cell);
            }
        }


        /// <summary>
        /// 将 Inspector 中可编辑的 List 转为运行时高速查询结构。
        /// </summary>
        private void BuildTileCaches()
        {
            iceSet = new HashSet<TileBase>();
            if (iceTiles != null)
            {
                for (int i = 0; i < iceTiles.Count; i++)
                {
                    var t = iceTiles[i];
                    if (t != null) iceSet.Add(t);
                }
            }

            buttonSet = new HashSet<TileBase>();
            if (buttonTiles != null)
            {
                for (int i = 0; i < buttonTiles.Count; i++)
                {
                    var t = buttonTiles[i];
                    if (t != null) buttonSet.Add(t);
                }
            }

            exitSet = new HashSet<TileBase>();
            if (exitTiles != null)
            {
                for (int i = 0; i < exitTiles.Count; i++)
                {
                    var t = exitTiles[i];
                    if (t != null) exitSet.Add(t);
                }
            }

            maskMap = new Dictionary<TileBase, FactionColor>();
            if (maskTiles != null)
            {
                for (int i = 0; i < maskTiles.Count; i++)
                {
                    var def = maskTiles[i];
                    if (def == null || def.tile == null) continue;

                    // 若同一个 tile 在列表中重复出现：后者覆盖前者
                    maskMap[def.tile] = def.color;
                }
            }
        }

        public bool InBounds(Vector2Int cell)
        {
            if (groundTilemap == null) return true;
            return groundTilemap.HasTile((Vector3Int)cell);
        }

        public bool IsWall(Vector2Int cell)
        {
            if (wallTilemap == null) return false;
            return wallTilemap.HasTile((Vector3Int)cell);
        }

        public bool IsIce(Vector2Int cell)
        {
            if (surfaceTilemap == null) return false;
            var tile = surfaceTilemap.GetTile((Vector3Int)cell);
            return tile != null && iceSet != null && iceSet.Contains(tile);
        }

        public bool IsOccupied(Vector2Int cell) => occupancy.ContainsKey(cell);

        public BaseActor GetOccupant(Vector2Int cell)
            => occupancy.TryGetValue(cell, out var a) ? a : null;

        public bool IsMaskCell(Vector2Int cell, out FactionColor color)
        {
            color = FactionColor.White;
            if (markerTilemap == null) return false;

            var tile = markerTilemap.GetTile((Vector3Int)cell);
            if (tile == null) return false;

            return maskMap != null && maskMap.TryGetValue(tile, out color);
        }

        public bool IsButtonCell(Vector2Int cell)
        {
            if (markerTilemap == null) return false;
            var tile = markerTilemap.GetTile((Vector3Int)cell);
            return tile != null && buttonSet != null && buttonSet.Contains(tile);
        }

        public bool IsExitCell(Vector2Int cell)
        {
            if (markerTilemap == null) return false;
            var tile = markerTilemap.GetTile((Vector3Int)cell);
            return tile != null && exitSet != null && exitSet.Contains(tile);
        }

        public void MoveActor(BaseActor actor, Vector2Int toCell)
        {
            if (actor == null) return;

            if (actorCell.TryGetValue(actor, out var from))
                occupancy.Remove(from);

            occupancy[toCell] = actor;
            actorCell[actor] = toCell;
             Debug.Log($"[MoveActor] {actor.name} {from} -> {toCell}");
            // 同步世界坐标（用 groundTilemap 或 Grid）
            var worldPos = groundTilemap.GetCellCenterWorld((Vector3Int)toCell);
            actor.transform.position = worldPos;
        }

        public Vector2Int GetActorCell(BaseActor actor)
            => actor != null && actorCell.TryGetValue(actor, out var c) ? c : Vector2Int.zero;

        public void RegisterActor(BaseActor actor, Vector2Int cell)
        {
            if (actor == null) return;
            actorCell[actor] = cell;
            occupancy[cell] = actor;
            actor.OnKilled += HandleActorKilled;
        }

        public void UnregisterActor(BaseActor actor)
        {
            if (actor == null) return;
            if (!actorCell.TryGetValue(actor, out var cell)) return;
            occupancy.Remove(cell);
            actorCell.Remove(actor);
            actor.OnKilled -= HandleActorKilled;
        }

        private void HandleActorKilled(BaseActor actor)
        {
            UnregisterActor(actor);
        }
    }
}
