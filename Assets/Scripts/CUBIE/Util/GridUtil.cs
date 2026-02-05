using UnityEngine;
namespace CUBIE
{
    public static class GridUtil
    {
        public static Vector2Int WorldToCell(Vector3 worldPos)
        {
            return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
        }

        public static Vector3 CellToWorldCenter(Vector2Int cell)
        {
            return new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
        }
    }
}
