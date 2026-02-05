namespace CUBIE
{
    public interface IActorEffect
    {
        void OnResolve(BaseActor actor, TurnContext ctx, IGridWorld world);
    }
}
