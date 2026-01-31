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
        [SerializeField] private List<TileBase> killTiles = new();
    

        [Header("Marker Tiles")]
        [SerializeField] private List<ButtonDef> buttonDefs = new();
        

        [SerializeField] private List<TileBase> exitTiles = new();
        [SerializeField] private List<MaskTileDef> maskTiles = new();

        // ===== Runtime caches (O(1) lookup) =====
        private HashSet<TileBase> iceSet;
        private HashSet<TileBase> killSet;

        private Dictionary<TileBase, ButtonDef> buttonMap;
        private HashSet<TileBase> exitSet;
        private Dictionary<TileBase, FactionColor> maskMap;

        // Runtime blocking cells
        private readonly HashSet<Vector2Int> blockCells = new();

        private readonly Dictionary<BaseActor, Vector2Int> actorCell = new();
        private readonly Dictionary<Vector2Int, BaseActor> occupancy = new();

        public  Tilemap GroundTilemap {get => groundTilemap;}
        
        private void Awake()
        {
            BuildTileCaches();
            BuildBlockCells();
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
            //ice
            iceSet = new HashSet<TileBase>();
            if (iceTiles != null)
            {
                for (int i = 0; i < iceTiles.Count; i++)
                {
                    var t = iceTiles[i];
                    if (t != null) iceSet.Add(t);
                }
            }
            // kill
            killSet = new HashSet<TileBase>();
            if (killTiles != null)
            {
                for (int i = 0; i < killTiles.Count; i++)
                {
                    var t = killTiles[i];
                    if (t != null) killSet.Add(t);
                }
            }


            //Buttons
            buttonMap = new Dictionary<TileBase, ButtonDef>();
            var idSet = new HashSet<string>();

            if (buttonDefs != null)
            {
                for (int i = 0; i < buttonDefs.Count; i++)
                {
                    var def = buttonDefs[i];
                    if (def == null || def.tile == null) continue;

                    // runtime cache
                    def.BuildRuntimeCache();

                    // tile -> def
                    buttonMap[def.tile] = def;

                    // id sanity check
                    if (string.IsNullOrWhiteSpace(def.id))
                    {
                        Debug.LogWarning($"[GridWorld] ButtonDef has empty id (tile={def.tile.name}).", this);
                    }
                    else if (!idSet.Add(def.id))
                    {
                        Debug.LogWarning($"[GridWorld] Duplicate ButtonDef id: {def.id}", this);
                    }
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

        private void BuildBlockCells()
        {
            blockCells.Clear();

            if (wallTilemap != null)
            {
                var bounds = wallTilemap.cellBounds;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        var cell = new Vector3Int(x, y, 0);
                        if (wallTilemap.HasTile(cell))
                            blockCells.Add((Vector2Int)cell);
                    }
                }
            }

            var doors = FindObjectsOfType<DoorMarker>();
            for (int i = 0; i < doors.Length; i++)
            {
                var door = doors[i];
                if (door == null) continue;
                if (!door.startClosed) continue;
                
                blockCells.Add(door.cell);
            }
        }

        public bool InBounds(Vector2Int cell)
        {
            if (groundTilemap == null) return true;
            return groundTilemap.HasTile((Vector3Int)cell);
        }

        public bool IsWall(Vector2Int cell)
        {
            return blockCells.Contains(cell);
        }
        public bool IsBlocked(Vector2Int cell) => blockCells.Contains(cell);

        public bool IsIce(Vector2Int cell)
        {
            if (surfaceTilemap == null) return false;
            var tile = surfaceTilemap.GetTile((Vector3Int)cell);
            return tile != null && iceSet != null && iceSet.Contains(tile);
        }

        public bool IsKillFloor(Vector2Int cell)
        {
            if (surfaceTilemap == null) return false;
            var tile = surfaceTilemap.GetTile((Vector3Int)cell);
            return tile != null && killSet != null && killSet.Contains(tile);
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
        
        public bool TryGetButton(Vector2Int cell, out ButtonDef def)
        {
            def = null;
            if (markerTilemap == null) return false;

            var tile = markerTilemap.GetTile((Vector3Int)cell);
            if (tile == null) return false;

            return buttonMap != null && buttonMap.TryGetValue(tile, out def);
        }

        public bool IsButtonCell(Vector2Int cell) => TryGetButton(cell, out _);

        public bool IsExitCell(Vector2Int cell)
        {
            if (markerTilemap == null) return false;
            var tile = markerTilemap.GetTile((Vector3Int)cell);
            return tile != null && exitSet != null && exitSet.Contains(tile);
        }

        // public void MoveActor(BaseActor actor, Vector2Int toCell)
        // {
        //     if (actor == null) return;

        //     var fromCell = actorCell.TryGetValue(actor, out var f) ? f : toCell;
        //     var fromWorld = actor.transform.position;
        //     var toWorld = groundTilemap.GetCellCenterWorld((Vector3Int)toCell);

        //     occupancy.Remove(fromCell);
        //     occupancy[toCell] = actor;
        //     actorCell[actor] = toCell;

        //     int tiles = Mathf.Abs(toCell.x - fromCell.x) + Mathf.Abs(toCell.y - fromCell.y);
        //     actor.PlayMoveAnimation(fromWorld, toWorld, tiles);
        // }

        public float MoveActor(BaseActor actor, Vector2Int toCell)
        {
            if (actor == null) return 0f;

            var fromCell = actorCell.TryGetValue(actor, out var f) ? f : toCell;
            occupancy.Remove(fromCell);
            occupancy[toCell] = actor;
            actorCell[actor] = toCell;

            var fromWorld = actor.transform.position;
            var toWorld = groundTilemap.GetCellCenterWorld((Vector3Int)toCell);

            int tiles = Mathf.Abs(toCell.x - fromCell.x) + Mathf.Abs(toCell.y - fromCell.y);
            return actor.PlayMoveAnimation(fromWorld, toWorld, tiles);
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

        public bool TryConsumeMask(Vector2Int cell, out FactionColor color)
        {
            color = FactionColor.White;
            if (!IsMaskCell(cell, out color)) return false;

            markerTilemap.SetTile((Vector3Int)cell, null); // 清掉面具图块
            return true;
        }

        
        public void AddBlocks(Vector2Int[] cells)
        {
            if (cells == null) return;
            for (int i = 0; i < cells.Length; i++)
                blockCells.Add(cells[i]);
        }

        public void RemoveBlocks(Vector2Int[] cells)
        {
            if (cells == null) return;
            for (int i = 0; i < cells.Length; i++)
                blockCells.Remove(cells[i]);
        }
        public void AddBlocks(Vector2Int cell)
        {
            blockCells.Add(cell);
        }
        
        
        public void RemoveBlocks(Vector2Int cell)
        {
            blockCells.Remove(cell);
        }

    }
}
