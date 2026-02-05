using System;
using UnityEngine;

namespace CUBIE
{
    public abstract class BaseActor : MonoBehaviour
    {
        private bool isAlive = true;

        private  ActorType actorType;
        
        public bool IsAlive => isAlive;
        public virtual ActorType ActorType
        {
            get => actorType;
            set => actorType = value;
        }

        public event Action<BaseActor> OnKilled;


        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public virtual void Kill()
        {
            if (!isAlive) return;

            isAlive = false;
            OnKilled?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}