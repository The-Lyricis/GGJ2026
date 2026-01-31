using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public class TurnSystem : MonoBehaviour
    {
        [SerializeField] private GridWorldBehaviour world;
        [SerializeField] private List<BaseActor> allActors = new();
        [SerializeField] private PlayerActor player;

        [SerializeField] private bool autoCollectOnAwake = true;

        private void Reset()
        {
            CollectSceneReferences();
        }

        private void OnValidate()
        {
            if (world == null || player == null || allActors == null || allActors.Count == 0)
                CollectSceneReferences();
        }

        private void Awake()
        {
            if (autoCollectOnAwake)
                CollectSceneReferences();
        }

        private void OnEnable()
        {
            CollectSceneReferences();

            if (player != null)
                player.OnKilled += HandlePlayerKilled;
        }

        private void OnDisable()
        {
            if (player != null)
                player.OnKilled -= HandlePlayerKilled;
        }

        private void HandlePlayerKilled(BaseActor actor)
        {
            if (player == null || actor != player) return;
            Debug.LogWarning("Game Over");
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
            if (world == null)
                world = FindFirstObjectByType<GridWorldBehaviour>();

            var actors = FindObjectsOfType<BaseActor>();

            allActors.Clear();
            for (int i = 0; i < actors.Length; i++)
                allActors.Add(actors[i]);

            player = null;
            for (int i = 0; i < allActors.Count; i++)
            {
                if (allActors[i] is PlayerActor p)
                {
                    player = p;
                    break;
                }
            }

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

            // 0) snapshot for blocking
            ctx.BuildSnapshot(allActors, world);

            // 1) player intent
            MoveIntent playerIntent = player.ReadIntent();

            // 2) broadcast intents
            SymbiosisController.Broadcast(playerIntent, allActors, ctx);

            // 3) solve movement
            ResolveMovement(ctx);

            // 3.1) detect player blocked (player has priority)
            var playerFrom = world.GetActorCell(player);
            bool playerBlocked = false;

            if (playerIntent.dir != MoveDir.None)
            {
                if (!ctx.TryGetPlannedMove(player, out var playerTo))
                    playerBlocked = true;
                else
                    playerBlocked = (playerTo == playerFrom);
            }

            // 3.2) if blocked, stop same-color NPCs and re-solve
            if (playerBlocked)
            {
                for (int i = 0; i < allActors.Count; i++)
                {
                    var a = allActors[i];
                    if (a == null || !a.IsAlive || a is PlayerActor) continue;

                    if (a.ControlColor == ctx.playerControlColor)
                        ctx.SetIntent(a, MoveIntent.None);
                }

                ctx.ClearPlannedMoves();
                ResolveMovement(ctx);
            }

            // 4) apply moves
            ctx.ApplyMoves(world);

            // 5) triggers (Latch-only button triggers happen here)
            ctx.ResolveTriggers(world, allActors);

            // 6) combat
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
                    if (a == null || !a.IsAlive) continue;

                    var intent = ctx.GetIntent(a);
                    if (intent.dir != dir) continue;

                    list.Add(a);
                }

                // front-to-back sorting to avoid ice exit conflicts
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
