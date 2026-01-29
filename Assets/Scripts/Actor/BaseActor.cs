using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026
{
    public abstract class BaseActor : MonoBehaviour
    {
        [Tooltip("The base color of the character (appearance / grouping / ability affiliation)")] [SerializeField]
        private FactionColor bodyColor = FactionColor.White;

        [Tooltip("Is Live")] [SerializeField] private bool isAlive = true;

        /// <summary>
        /// The base color of the character (appearance / grouping / ability affiliation)
        /// </summary>
        public virtual FactionColor ControlColor 
        {
            get => bodyColor;
            set => bodyColor = value;
        }

        /// <summary>
        /// Battle determination color (used for the strength of kills, immunity of the same color, etc.)
        /// </summary>
        public virtual FactionColor CombatColor => bodyColor;

        /// <summary>
        /// Is Live
        /// </summary>
        public bool IsAlive => isAlive;


        [Tooltip("Current mind component that controls this actor")] [SerializeField]
        private MonoBehaviour mindComponent = null;

        private IMind mind = null;

        /// <summary>
        /// Ability/Rule Executor. Movement, firing, taking damage, and passive effects are all extended through Traits.
        /// </summary>
        private readonly List<IActorTrait> traits = new();

        /// <summary>
        /// Event when the actor is killed  
        /// </summary>
        public event Action<BaseActor> OnKilled;

        protected virtual void Awake()
        {
            if (mindComponent != null && mindComponent is IMind m)
            {
                mind = m;
            }

            //AddTrait 
            foreach (var t in GetComponents<MonoBehaviour>())
            {
                if (t is IActorTrait trait) AddTrait(trait);
            }
        }

        public void SetMind(IMind newMind)
        {
            mind = newMind;
        }

        public IMind GetMind() => mind;

        /// <summary>
        /// Add a trait to the actor
        /// </summary>
        public void AddTrait(IActorTrait trait)
        {
            if (trait == null) return;

            if (!traits.Contains(trait))
            {
                traits.Add(trait);
                trait.OnAdded(this);
            }
        }

        /// <summary>
        /// Remove a trait from the actor
        /// </summary>
        public void RemoveTrait(IActorTrait trait)
        {
            if (trait == null) return;

            if (traits.Remove(trait))
            {
                trait.OnRemoved(this);
            }
        }
        
        public MoveIntent ReadIntent()
        {
            if (!isAlive || mind == null) return MoveIntent.None;
            return mind.ReadMoveIntent();
        }
        
        public void DispatchIntent(MoveIntent intent, TurnContext ctx)
        {
            for (int i = 0; i < traits.Count; i++)
                traits[i].OnIntent(this, intent, ctx);
        }
        
        public virtual void Kill()
        {
            isAlive = false;
            OnKilled?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}


