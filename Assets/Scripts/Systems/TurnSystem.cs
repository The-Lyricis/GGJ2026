using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public class TurnSystem : MonoBehaviour
    {
        [SerializeField] private GridWorldBehaviour world;
        [SerializeField] private List<BaseActor> allActors = new();
        [SerializeField] private PlayerActor player;


        private bool autoCollectOnAwake = true;
        private bool includeInactiveActors = true;

        private void Reset()
        {
            // 当组件第一次被添加到物体上时调用
            CollectSceneReferences();
        }

        private void OnValidate()
        {
            // Inspector 修改时自动刷新
            // 避免每次都刷太频繁：只在缺引用时补齐
            if (world == null || player == null || allActors == null || allActors.Count == 0)
                CollectSceneReferences();
        }

        private void Awake()
        {
            if (autoCollectOnAwake)
                CollectSceneReferences();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D))
            {
                StepTurn();
            }
        }

        [ContextMenu("Collect Scene References")]
        public void CollectSceneReferences()
        {
            // 1) world
            if (world == null)
                world = FindFirstObjectByType<GridWorldBehaviour>();

            // 2) allActors
#if UNITY_2023_1_OR_NEWER
            var actors = FindObjectsByType<BaseActor>(
                includeInactiveActors ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );
#else
            // 旧版本 Unity：FindObjectsOfType 默认不包含 inactive
            var actors = FindObjectsOfType<BaseActor>();
#endif
            allActors.Clear();
            for (int i = 0; i < actors.Length; i++)
            {
                // 若你希望把某些 actor 排除，可在这里加过滤条件
                allActors.Add(actors[i]);
            }

            // 3) player：优先从 allActors 中找（避免再全场扫描）
            player = null;
            for (int i = 0; i < allActors.Count; i++)
            {
                if (allActors[i] is PlayerActor p)
                {
                    player = p;
                    break;
                }
            }

            // 如果没找到，再兜底扫描一次
            if (player == null)
                player = FindFirstObjectByType<PlayerActor>();
        }



        public void StepTurn()
        {
            if (world == null || player == null) return;

            var ctx = new TurnContext
            {
                playerControlColor = player.ControlColor
            };

            // 0) Build snapshot for blocking
            ctx.BuildSnapshot(allActors, world);

            // 1) Read player intent
            MoveIntent playerIntent = player.ReadIntent();

            // 2) Broadcast intents to controlled actors
            SymbiosisController.Broadcast(playerIntent, allActors, ctx);

            // 3) Solve movement (front-to-back by direction)
            this.ResolveMovement(ctx);

            // 3.1) Decide if player is blocked using planned moves
            var playerFrom = world.GetActorCell(player);
            bool playerBlocked = false;

            if (playerIntent.dir != MoveDir.None)
            {
                if (!ctx.TryGetPlannedMove(player, out var playerTo))
                    playerBlocked = true; // 没有计划移动，视作被挡
                else
                    playerBlocked = (playerTo == playerFrom);
            }

            // 3.2) If blocked, stop same-color NPCs and re-solve
            if (playerBlocked)
            {
                for (int i = 0; i < allActors.Count; i++)
                {
                    var a = allActors[i];
                    if (!a.IsAlive || a is PlayerActor) continue;

                    if (a.ControlColor == ctx.playerControlColor)
                        ctx.SetIntent(a, MoveIntent.None);
                }

                ctx.ClearPlannedMoves();
                // 3) Solve movement (front-to-back by direction)
                this.ResolveMovement(ctx);
            }

            // 4) Apply moves
            ctx.ApplyMoves(world);

            // 5) Resolve triggers
            ctx.ResolveTriggers(world, allActors);

            // 6) Resolve combat
            CombatSystem.Resolve(allActors, world);

        }

        private void ResolveMovement(TurnContext ctx)
        {
            MoveDir[] dirs = { MoveDir.Up, MoveDir.Down, MoveDir.Left, MoveDir.Right };

            for (int d = 0; d < dirs.Length; d++)
            {
                var dir = dirs[d];
                var delta = MoveUtil.DirToDelta(dir);

                var list = new List<BaseActor>();
                for (int i = 0; i < allActors.Count; i++)
                {
                    var a = allActors[i];
                    if (!a.IsAlive) continue;

                    var intent = ctx.GetIntent(a);
                    if (intent.dir != dir) continue;

                    list.Add(a);
                }

                list.Sort((a, b) =>
                {
                    var ca = world.GetActorCell(a);
                    var cb = world.GetActorCell(b);
                    return Projection(cb, delta).CompareTo(Projection(ca, delta));
                });

                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    var intent = ctx.GetIntent(a);
                    a.DispatchIntent(intent, ctx);
                }
            }
        }

        private static int Projection(Vector2Int cell, Vector2Int delta)
        {
            return cell.x * delta.x + cell.y * delta.y;
        }
    }

}
