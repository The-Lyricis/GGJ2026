using UnityEngine;

namespace CUBIE
{
    public class DoorReceiver : ButtonReceiverBase
    {
        [SerializeField] private bool startClosed = true;
        [SerializeField] private BlockType closedBlockType = BlockType.Wall;

        private IGridWorld world;
        private Vector2Int cell;

        private void Awake()
        {
            world = FindWorld();
            if (world == null) return;

            cell = GridUtil.WorldToCell(transform.position);

            if (startClosed)
                world.SetBlock(cell, closedBlockType);
            else
                world.SetBlock(cell, BlockType.None);
        }

        protected override void ApplyInternal(bool active)
        {
            if (world == null) world = FindWorld();
            if (world == null) return;

            if (active)
                world.SetBlock(cell, BlockType.None);
            else
                world.SetBlock(cell, closedBlockType);
        }

        private static IGridWorld FindWorld()
        {
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IGridWorld w)
                    return w;
            }
            return null;
        }
    }
}
