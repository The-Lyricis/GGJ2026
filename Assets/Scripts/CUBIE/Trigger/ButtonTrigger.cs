using System;
using UnityEngine;

namespace CUBIE
{
    public class ButtonTrigger : MonoBehaviour, ITrigger
    {
        [SerializeField] private int priority = 0;
        [SerializeField] private bool killOnTrigger = true;

        public int Priority => priority;

        public bool Matches(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            throw new NotImplementedException();
        }

        public void Execute(BaseActor actor, TurnContext ctx, IGridWorld world)
        {
            
        }
    }
}
