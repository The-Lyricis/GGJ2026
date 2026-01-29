
namespace GGJ2026
{   
    public interface IActorTrait
    {
        void OnAdded(BaseActor actor);
        void OnRemoved(BaseActor actor);
        
        void OnIntent(BaseActor actor, MoveIntent intent, TurnContext ctx);
    }
}