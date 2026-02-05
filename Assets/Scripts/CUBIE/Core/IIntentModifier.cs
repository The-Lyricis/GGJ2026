namespace CUBIE
{
    public interface IIntentModifier
    {   
        void OnIntent(BaseActor actor, MoveIntent intent, TurnContext ctx);
    }
}
