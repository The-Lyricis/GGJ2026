using System.Collections.Generic;

namespace GGJ2026
{
    public static class SymbiosisController
    {
        public static void Broadcast(MoveIntent playerIntent, List<BaseActor> actors, TurnContext ctx)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (!a.IsAlive) continue;

                if (a is PlayerActor)
                {
                    ctx.SetIntent(a, playerIntent);
                    continue;
                }

                // 同控制色的 NPC 跟随玩家意图移动，否则无意图
                if (a.ControlColor == ctx.playerControlColor)
                    ctx.SetIntent(a, playerIntent);
                else
                    ctx.SetIntent(a, MoveIntent.None);
            }
        }
    }
}
