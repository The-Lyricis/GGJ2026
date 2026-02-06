using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CUBIE
{
    
    [Serializable]
    public struct BlockTileDef
    {
        public TileBase tile;
        public BlockType type;
    }

    [Serializable]
    public struct SurfaceTileDef
    {
        public TileBase tile;
        public SurfaceType type;
    }

    public class CubieGridWorld : MonoBehaviour, IGridWorld
    {
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap blockTilemap;
        [SerializeField] private Tilemap surfaceTilemap;
        
        
        [SerializeField] private List<BlockTileDef> blockTiles = new();
        [SerializeField] private List<SurfaceTileDef> surfaceTiles = new();


        private Dictionary<TileBase, BlockType> blockMap;

        private Dictionary<TileBase, SurfaceType> surfaceMap;

        private readonly Dictionary<BaseActor, Vector2Int> actorToCell = new();
        private readonly Dictionary<Vector2Int, BaseActor> cellToActor = new();

        private readonly Dictionary<Vector2Int, BlockType> dynamicBlocks = new();
        
        //
        
        private void Awake()
        {
            RebuildMaps();
        }

        private void OnValidate()
        {
            RebuildMaps();
        }

        private void Start()
        {
            RebuildMaps();
        }

        private void RebuildMaps()
        {
            blockMap = BuildMap(blockTiles);
            surfaceMap = BuildMap(surfaceTiles);
        }

        private static Dictionary<TileBase, BlockType> BuildMap(List<BlockTileDef> defs)
        {
            var map = new Dictionary<TileBase, BlockType>();
            for (int i = 0; i < defs.Count; i++)
            {
                var d = defs[i];
                if (d.tile == null) continue;
                map[d.tile] = d.type;
            }
            return map;
        }

        private static Dictionary<TileBase, SurfaceType> BuildMap(List<SurfaceTileDef> defs)
        {
            var map = new Dictionary<TileBase, SurfaceType>();
            for (int i = 0; i < defs.Count; i++)
            {
                var d = defs[i];
                if (d.tile == null) continue;
                map[d.tile] = d.type;
            }
            return map;
        }


        // At present, bounds are defined by ground tilemap
        // InBounds = HasGround
        bool InBounds(Vector2Int cell)
        {
            if (groundTilemap == null) return true;
            return groundTilemap.HasTile((Vector3Int)cell);
        }
        
        public bool HasGround(Vector2Int cell)
        {
            if (groundTilemap == null) return true;
            return groundTilemap.HasTile((Vector3Int)cell);
        }

        public BlockType GetBlock(Vector2Int cell)
        {
            if (dynamicBlocks.TryGetValue(cell, out var d)) return d;
            if (blockTilemap == null) return BlockType.None;
            var tile = blockTilemap.GetTile((Vector3Int)cell);
            if (tile == null || blockMap == null) return BlockType.None;
            return blockMap.TryGetValue(tile, out var t) ? t : BlockType.None;
        }

        public SurfaceType GetSurface(Vector2Int cell)
        {
            if (surfaceTilemap == null) return SurfaceType.None;
            var tile = surfaceTilemap.GetTile((Vector3Int)cell);
            if (tile == null || surfaceMap == null) return SurfaceType.None;
            return surfaceMap.TryGetValue(tile, out var t) ? t : SurfaceType.None;
        }

        public void SetBlock(Vector2Int cell, BlockType type)
        {
            if (type == BlockType.None)
                dynamicBlocks.Remove(cell);
            else
                dynamicBlocks[cell] = type;

            return;
        }

        public void SetSurface(Vector2Int cell, SurfaceType type)
        {
            if (surfaceTilemap == null) return;
            if (type == SurfaceType.None)
            {
                surfaceTilemap.SetTile((Vector3Int)cell, null);
                return;
            }

            var tile = FindTile(surfaceTiles, type);
            surfaceTilemap.SetTile((Vector3Int)cell, tile);
        }


        public Vector2Int GetActorCell(BaseActor actor)
        {
            if (actor == null) return Vector2Int.zero;
            if (actorToCell.TryGetValue(actor, out var cell)) return cell;

            var cellFromPos = GridUtil.WorldToCell(actor.transform.position);
            actorToCell[actor] = cellFromPos;
            cellToActor[cellFromPos] = actor;
            return cellFromPos;
        }

        private void SetActorCell(BaseActor actor, Vector2Int toCell, bool updateTransform)
        {
            if (actor == null) return;

            if (actorToCell.TryGetValue(actor, out var fromCell))
            {
                if (cellToActor.TryGetValue(fromCell, out var occ) && occ == actor)
                    cellToActor.Remove(fromCell);
            }

            actorToCell[actor] = toCell;
            cellToActor[toCell] = actor;
            if (updateTransform)
                actor.transform.position = GridUtil.CellToWorldCenter(toCell);
        }

        private void HandleActorKilled(BaseActor actor) => UnregisterActor(actor);
        public void RegisterActor(BaseActor actor, Vector2Int cell)
        {
            if (actor == null) return;
            SetActorCell(actor, cell, true);
            actor.OnKilled += HandleActorKilled;
        }
        
        public void UnregisterActor(BaseActor actor)
        {
            if (actor == null) return;
            if (actorToCell.TryGetValue(actor, out var cell))
            {
                if (cellToActor.TryGetValue(cell, out var occ) && occ == actor)
                    cellToActor.Remove(cell);
                actorToCell.Remove(actor);
            }
            actor.OnKilled -= HandleActorKilled;
        }

        public float MoveActor(BaseActor actor, Vector2Int toCell, float duration)
        {
            if (actor == null || !actor.IsAlive) return 0f;
            if (!InBounds(toCell)) return 0f;
            if (!HasGround(toCell)) return 0f;

            if (cellToActor.TryGetValue(toCell, out var occ) && occ != null && occ != actor)
                return 0f;

            var fromCell = actorToCell.TryGetValue(actor, out var f) ? f : GridUtil.WorldToCell(actor.transform.position);
            SetActorCell(actor, toCell, false);

            var mover = actor.GetComponent<ActorMover>();
            if (mover != null)
            {
                var dir = new Vector2Int(
                    Mathf.Clamp(toCell.x - fromCell.x, -1, 1),
                    Mathf.Clamp(toCell.y - fromCell.y, -1, 1)
                );
                return mover.MoveTo(GridUtil.CellToWorldCenter(toCell), dir, duration * 0.65f);
            }

            actor.transform.position = GridUtil.CellToWorldCenter(toCell);
            return duration;
        }

        private static TileBase FindTile(List<BlockTileDef> defs, BlockType type)
        {
            for (int i = 0; i < defs.Count; i++)
            {
                var d = defs[i];
                if (d.type == type) return d.tile;
            }
            return null;
        }

        private static TileBase FindTile(List<SurfaceTileDef> defs, SurfaceType type)
        {
            for (int i = 0; i < defs.Count; i++)
            {
                var d = defs[i];
                if (d.type == type) return d.tile;
            }
            return null;
        }

    }
}
