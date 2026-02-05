using System.Collections.Generic;

namespace CUBIE
{
    public interface ITriggerResolver
    {
        void Resolve(IGridWorld world, IReadOnlyList<BaseActor> actors, TurnContext ctx);
    }
}
