using System;
using UnityEngine;

namespace CUBIE
{
    public abstract class BaseActor : MonoBehaviour
    {
        private bool isAlive = true;
        private bool isSliding = false;
        private MoveDir slideDir = MoveDir.None;

        private  ActorType actorType;
        
        public bool IsAlive => isAlive;
        public virtual ActorType ActorType
        {
            get => actorType;
            set => actorType = value;
        }

        public event Action<BaseActor> OnKilled;

        public bool IsSliding => isSliding;
        public MoveDir SlideDir => slideDir;

        public void StartSlide(MoveDir dir)
        {
            if (dir == MoveDir.None) return;
            isSliding = true;
            slideDir = dir;
        }

        public void StopSlide()
        {
            isSliding = false;
            slideDir = MoveDir.None;
        }


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
            StopSlide();
            OnKilled?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
