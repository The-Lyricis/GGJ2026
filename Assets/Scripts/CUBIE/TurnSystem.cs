using System.Collections.Generic;
using UnityEngine;

namespace CUBIE
{
    public class TurnSystem : MonoBehaviour
    {
        [Header("World")]
        [SerializeField] private MonoBehaviour worldComponent;
        private IGridWorld world;

        [Header("Actors")]
        [SerializeField] private List<BaseActor> allActors = new();
        [SerializeField] private BaseActor player;
        [SerializeField] private bool autoCollectOnAwake = true;
        
        //[Header("Input")]
        //private float inputLockTimer = 0f;
        
        private MovementResolver movementResolver = new MovementResolver();
        
        private void Awake()
        {
            if (autoCollectOnAwake)
                CollectSceneReferences();

            world = worldComponent as IGridWorld;
        }

        private void Update()
        {
            // if (inputLockTimer > 0f)
            // {
            //     inputLockTimer -= Time.deltaTime;
            //     return;
            // }
            StepTurn();
        }
        public void StepTurn()
        {
            if (world == null || player == null || !player.IsAlive) return;

            var input = player.GetComponent<IInputSource>();
            if(input == null)
            {
                return;
            } 

            MoveIntent playerIntent = input.ReadMoveIntent();

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
            ctx.ApplyMoves(world);

            // 5) 触发
            ResolveTriggers(ctx);

            // 6) 回合效果结算
            ResolveActorEffects(ctx);
        }

        private void BroadcastIntents(MoveIntent playerIntent, TurnContext ctx)
        {
            if (ctx == null) return;
            if (player != null)
                ctx.SetIntent(player, playerIntent);

            for (int i = 0; i < allActors.Count; i++)
            {
                var actor = allActors[i];
                if (actor == null || !actor.IsAlive || actor == player) continue;

                if (player is PlayerActor pa && actor.ActorType == pa.ControlType)
                {
                    ctx.SetIntent(actor, playerIntent);
                    continue;
                }

                var input = actor.GetComponent<IInputSource>();
                if (input != null)
                    ctx.SetIntent(actor, input.ReadMoveIntent());
                else
                    ctx.SetIntent(actor, MoveIntent.None);
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
        }
    }
}
