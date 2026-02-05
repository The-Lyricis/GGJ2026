namespace CUBIE
{
    public interface ITrigger
    {
        // 是否在当前回合触发（例如：actor 站在触发格）
        bool Matches(BaseActor actor, TurnContext ctx, IGridWorld world);

        // 触发效果
        void Execute(BaseActor actor, TurnContext ctx, IGridWorld world);

        // 触发优先级（小的先执行）
        int Priority { get; }
    }
}
