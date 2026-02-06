using System.Collections.Generic;
using UnityEngine;

namespace CUBIE
{
    public class TurnSystem : MonoBehaviour
    {
        [Header("World")]
        [SerializeField] private MonoBehaviour worldComponent;
        private IGridWorld world;

        [Header("Systems")]
        [SerializeField] private ButtonSystem buttonSystem;

        [Header("Actors")]
        [SerializeField] private List<BaseActor> allActors = new();
        [SerializeField] private BaseActor player;

        [Header("Move Interval Timing")]
        [SerializeField] private float stepInterval = 0.12f;
        [SerializeField] private float holdInitialDelay = 0.0f;
        [SerializeField] private float holdRepeatInterval = 0.12f;

        private MoveDir heldDir = MoveDir.None;
        private float holdTimer = 0f;
        private float repeatTimer = 0f;

        //[Header("Input")]
        private bool turnLocked = false;
        private float lockTimer = 0f;
        private MoveIntent bufferedIntent = MoveIntent.None;

        private MovementResolver movementResolver = new MovementResolver();

        private void Awake()
        {
            CollectSceneReferences();
        }

        private void Update()
        {
            if (turnLocked)
            {
                lockTimer -= Time.deltaTime;
                if (lockTimer <= 0f) turnLocked = false;
            }

            if (!turnLocked && AnyActorSliding())
            {
                bufferedIntent = MoveIntent.None;
                StepTurn(MoveIntent.None, true);
                return;
            }

            var intent = ReadPlayerIntentBuffered();
            if (intent.dir != MoveDir.None)
                bufferedIntent = intent;

            if (turnLocked) return;
            if (bufferedIntent.dir != MoveDir.None)
            {
                var use = bufferedIntent;
                bufferedIntent = MoveIntent.None;
                StepTurn(use, false);
                return;
            }

            // no input and no sliding: do nothing
        }

        private MoveIntent ReadPlayerIntentBuffered()
        {
            if (player == null || !player.IsAlive) return MoveIntent.None;

            var input = player.GetComponent<IInputSource>();
            if (input == null) return MoveIntent.None;

            if (player.IsSliding)
            {
                bufferedIntent = MoveIntent.None;
                return new MoveIntent { dir = player.SlideDir };
            }

            var down = input.ReadDownDir();
            if (down != MoveDir.None)
            {
                heldDir = down;
                holdTimer = 0f;
                repeatTimer = 0f;
                return new MoveIntent { dir = down };
            }

            var hold = input.ReadHoldDir();
            if (hold == MoveDir.None)
            {
                heldDir = MoveDir.None;
                return MoveIntent.None;
            }

            if (hold != heldDir)
            {
                heldDir = hold;
                holdTimer = 0f;
                repeatTimer = 0f;
                return new MoveIntent { dir = hold };
            }

            holdTimer += Time.deltaTime;
            if (holdTimer < holdInitialDelay) return MoveIntent.None;

            repeatTimer += Time.deltaTime;
            if (repeatTimer >= holdRepeatInterval)
            {
                repeatTimer = 0f;
                return new MoveIntent { dir = heldDir };
            }

            return MoveIntent.None;
        }

        public void StepTurn(MoveIntent playerIntent, bool autoStep)
        {
            float animTime = 0f;
            if (world == null) return;
            if (!autoStep && (player == null || !player.IsAlive)) return;
            if (playerIntent.dir == MoveDir.None && !autoStep) return;

            var ctx = new TurnContext();
            ctx.BuildSnapshot(allActors, world);

            // 1) 广播意图
            BroadcastIntents(playerIntent, ctx);

            // 2) 移动解析
            movementResolver.ResolveAll(ctx, world, allActors);

            // 3) 玩家阻挡判定
            if (IsPlayerBlocked(playerIntent, ctx))
            {
                for (int i = 0; i < allActors.Count; i++)
                {
                    var actor = allActors[i];
                    if (actor == null || !actor.IsAlive || actor == player) continue;
                    if (player is PlayerActor pa && actor.ActorType == pa.ControlType)
                        ctx.SetIntent(actor, MoveIntent.None);
                }
                ctx.ClearPlannedMoves();
                movementResolver.ResolveAll(ctx, world, allActors);
            }

            // 4) 应用移动
            animTime += ctx.ApplyMoves(world, stepInterval);

            // 5) 触发
            ResolveTriggers(ctx);

            // 6) 回合效果结算
            ResolveActorEffects(ctx);

            LockTurn(Mathf.Max(stepInterval, animTime));
        }

        private void BroadcastIntents(MoveIntent playerIntent, TurnContext ctx)
        {
            if (ctx == null) return;
            if (player != null)
            {
                if (player.IsSliding)
                    ctx.SetIntent(player, new MoveIntent { dir = player.SlideDir });
                else
                    ctx.SetIntent(player, playerIntent);
            }

            for (int i = 0; i < allActors.Count; i++)
            {
                var actor = allActors[i];
                if (actor == null || !actor.IsAlive || actor == player) continue;

                if (actor.IsSliding)
                {
                    ctx.SetIntent(actor, new MoveIntent { dir = actor.SlideDir });
                    continue;
                }

                if (player is PlayerActor pa && actor.ActorType == pa.ControlType)
                {
                    ctx.SetIntent(actor, playerIntent);
                    continue;
                }

                var input = actor.GetComponent<IInputSource>();
                if (input != null)
                {
                    var dir = input.ReadDownDir();
                    if (dir == MoveDir.None) dir = input.ReadHoldDir();
                    ctx.SetIntent(actor, new MoveIntent { dir = dir });
                }
                else
                {
                    ctx.SetIntent(actor, MoveIntent.None);
                }
            }

            // Allow traits to modify intents
            for (int i = 0; i < allActors.Count; i++)
            {
                var actor = allActors[i];
                if (actor == null || !actor.IsAlive) continue;

                var monoBehaviours = actor.GetComponents<MonoBehaviour>();
                for (int j = 0; j < monoBehaviours.Length; j++)
                {
                    if (monoBehaviours[j] is IIntentModifier modifier)
                        modifier.OnIntent(actor, ctx.GetIntent(actor), ctx);
                }
            }
        }

        private bool IsPlayerBlocked(MoveIntent playerIntent, TurnContext ctx)
        {
            if (player == null || ctx == null) return false;
            if (playerIntent.dir == MoveDir.None) return false;
            if (ctx.IsBlocked(player)) return true;
            if (ctx.TryGetPlannedMove(player, out var planned))
                return planned == world.GetActorCell(player);
            return false;
        }


        private void ResolveTriggers(TurnContext ctx)
        {
            if (ctx == null || world == null) return;

            var triggerBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            var triggers = new List<ITrigger>();
            for (int i = 0; i < triggerBehaviours.Length; i++)
            {
                if (triggerBehaviours[i] is ITrigger trigger)
                    triggers.Add(trigger);
            }

            if (triggers.Count == 0) return;

            triggers.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            for (int i = 0; i < allActors.Count; i++)
            {
                var actor = allActors[i];
                if (actor == null || !actor.IsAlive) continue;

                for (int t = 0; t < triggers.Count; t++)
                {
                    var trigger = triggers[t];
                    if (trigger.Matches(actor, ctx, world))
                        trigger.Execute(actor, ctx, world);
                }
            }

            buttonSystem?.ResolveAndClear();
        }

        private void ResolveActorEffects(TurnContext ctx)
        {
            if (ctx == null) return;
            for (int i = 0; i < allActors.Count; i++)
            {
                var actor = allActors[i];
                if (actor == null || !actor.IsAlive) continue;

                var monoBehaviours = actor.GetComponents<MonoBehaviour>();
                for (int j = 0; j < monoBehaviours.Length; j++)
                {
                    if (monoBehaviours[j] is IActorEffect effect)
                        effect.OnResolve(actor, ctx, world);
                }
            }
        }


        public void CollectSceneReferences()
        {
            var actors = FindObjectsByType<BaseActor>(FindObjectsSortMode.None);
            allActors = new List<BaseActor>(actors);

            if (player == null)
            {
                for (int i = 0; i < allActors.Count; i++)
                {
                    if (allActors[i] is PlayerActor)
                    {
                        player = allActors[i];
                        break;
                    }
                }
            }

            if (worldComponent == null)
            {
                var worldBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                for (int i = 0; i < worldBehaviours.Length; i++)
                {
                    if (worldBehaviours[i] is IGridWorld)
                    {
                        worldComponent = worldBehaviours[i];
                        break;
                    }
                }
            }

            world = worldComponent as IGridWorld;
            if (world != null)
            {
                for (int i = 0; i < allActors.Count; i++)
                {
                    var actor = allActors[i];
                    if (actor == null) continue;
                    world.RegisterActor(actor, GridUtil.WorldToCell(actor.transform.position));
                }
            }

            if (buttonSystem == null)
            {
                var systems = FindObjectsByType<ButtonSystem>(FindObjectsSortMode.None);
                if (systems.Length > 0) buttonSystem = systems[0];
            }
        }

        private void LockTurn(float duration)
        {
            if (duration <= 0f) return;
            turnLocked = true;
            lockTimer = Mathf.Max(lockTimer, duration);
        }

        private bool AnyActorSliding()
        {
            for (int i = 0; i < allActors.Count; i++)
            {
                var actor = allActors[i];
                if (actor == null || !actor.IsAlive) continue;
                if (actor.IsSliding) return true;
            }
            return false;
        }

    }
}
