using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GGJ2026
{
    public class TurnSystem : MonoBehaviour
    {
        [SerializeField] private GridWorldBehaviour world;
        [SerializeField] private List<BaseActor> allActors = new();
        [SerializeField] private PlayerActor player;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D))
            {
                StepTurn();
            }
        }

        public void StepTurn()
        {
            Debug.Log("=== StepTurn ===");
            if (world == null || player == null) return;

            var ctx = new TurnContext
            {
                playerControlColor = player.ControlColor
            };

            // 0) 回合快照（阻挡判定使用快照）
            ctx.BuildSnapshot(allActors, world);

            // 1) 读取玩家输入（Mind 只产出意图）
            MoveIntent playerIntent = player.ReadIntent();
            Debug.Log($"playerIntent = {playerIntent.dir}");


            var playerFrom = world.GetActorCell(player);
            var playerTo = ctx.ResolveMovement(player, playerFrom, playerIntent.dir, world);
            bool playerBlocked = (playerIntent.dir != MoveDir.None && playerTo == playerFrom);


            // 2) 共生广播 -> 为每个 actor 写入本回合 intent
            SymbiosisController.Broadcast(playerIntent, allActors, ctx);

            // Player blocked -> all controlled NPCs do nothing
            if (playerBlocked)
            {
                for (int i = 0; i < allActors.Count; i++)
                {
                    var a = allActors[i];
                    if (!a.IsAlive || a is PlayerActor) continue;
                    if (a.ControlColor == ctx.playerControlColor)
                        ctx.SetIntent(a, MoveIntent.None);
                }
            }

            // 3) Trait 执行（通常由 GridMovementTrait 计算 to 并 QueueMove）
            for (int i = 0; i < allActors.Count; i++)
            {
                var a = allActors[i];
                if (!a.IsAlive) continue;

                var intent = ctx.GetIntent(a);
                a.DispatchIntent(intent, ctx);
            }

            // 4) 同时应用位置
            ctx.ApplyMoves(world);

            // 5) 触发与拾取
            ctx.ResolveTriggers(world, allActors);

            // 6) 战斗结算（整体一次性）
            CombatSystem.Resolve(allActors, world);
        }
    }

}
