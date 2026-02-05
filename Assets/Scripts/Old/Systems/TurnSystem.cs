using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GGJ2026
{
    public class TurnSystem : MonoBehaviour
    {
        [SerializeField] private GridWorldBehaviour world;
        [SerializeField] private List<BaseActor> allActors = new();
        [SerializeField] private PlayerActor player;

        [SerializeField] private bool autoCollectOnAwake = true;
        [SerializeField] private Transform blockShakeTarget;
        [SerializeField] private float blockShakeDuration = 0.08f;
        [SerializeField] private float blockShakeStrength = 0.08f;
        [SerializeField] private int blockShakeVibrato = 8;
        [SerializeField] private float blockShakeRandomness = 90f;
        [SerializeField] private bool blockShakeFadeOut = true;

        private Tween blockShakeTween;
        private Vector3 blockShakeOriginLocal;

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

            if (blockShakeTarget == null && Camera.main != null)
                blockShakeTarget = Camera.main.transform;

            if (blockShakeTarget != null)
                blockShakeOriginLocal = blockShakeTarget.localPosition;
        }

        private void OnEnable()
        {
            CollectSceneReferences();

            if (player != null)
                player.OnKilled += HandlePlayerKilled;

            if (blockShakeTarget == null && Camera.main != null)
                blockShakeTarget = Camera.main.transform;

            if (blockShakeTarget != null)
                blockShakeOriginLocal = blockShakeTarget.localPosition;
        }

        private void OnDisable()
        {
            if (player != null)
                player.OnKilled -= HandlePlayerKilled;

            if (blockShakeTween != null && blockShakeTween.IsActive())
                blockShakeTween.Kill();
        }

        private void HandlePlayerKilled(BaseActor actor)
        {
            if (player == null || actor != player) return;
            Debug.LogWarning("Game Over");
        }
        private float inputLockTimer = 0.2f;
        private void Update()
        {
            if (inputLockTimer > 0f)
            {
                inputLockTimer -= Time.deltaTime;
                return;
            }
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
                TriggerBlockShake();
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

            for (int i = 0; i < allActors.Count; i++)
            {
                var a = allActors[i];
                if (a == null || !a.IsAlive) continue;

                var intent = ctx.GetIntent(a);
                if (intent.dir == MoveDir.None) continue;

                var from = world.GetActorCell(a);
                if (!ctx.TryGetPlannedMove(a, out var to) || to == from)
                {
                    a.PlayBlockedAnimation();
                }
            }

            bool playerMoved = false;
            bool playerWasBlocked = false;
            var playerIntentNow = ctx.GetIntent(player);
            if (playerIntentNow.dir != MoveDir.None)
            {
                var playerFromCell = world.GetActorCell(player);
                if (!ctx.TryGetPlannedMove(player, out var playerToCell) || playerToCell == playerFromCell)
                    playerWasBlocked = true;
                else
                    playerMoved = true;
            }

            // 4) apply moves
            float animTime = ctx.ApplyMoves(world);
            inputLockTimer = animTime*0.9f;
            if (playerWasBlocked) AudioManager.Instance?.PlayBlockedSFX();
            if (playerMoved) AudioManager.Instance?.PlayMoveSFX();

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

        private void TriggerBlockShake()
        {
            if (blockShakeTarget == null) return;

            if (blockShakeTween != null && blockShakeTween.IsActive())
                blockShakeTween.Kill();

            blockShakeTarget.localPosition = blockShakeOriginLocal;
            blockShakeTween = blockShakeTarget
                .DOShakePosition(blockShakeDuration, blockShakeStrength, blockShakeVibrato, blockShakeRandomness, false, blockShakeFadeOut)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (blockShakeTarget != null)
                        blockShakeTarget.localPosition = blockShakeOriginLocal;
                });
        }
    }
}
