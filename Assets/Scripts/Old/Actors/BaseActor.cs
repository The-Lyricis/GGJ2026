using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace GGJ2026
{
    public abstract class BaseActor : MonoBehaviour
    {
        [Tooltip("The base color of the character (appearance / grouping / ability affiliation)")] [SerializeField]
        private FactionColor bodyColor;

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

        public SpriteRenderer spriteRenderer;
        public List<Sprite> spriteList; //1 = up, 2= down, 3= left, 4= right

        public MoveDir currentDir = MoveDir.Right;

        [Header("Blocked Shake")]
        [SerializeField] private Transform blockedShakeTarget;
        [SerializeField] private float blockedShakeDuration = 0.08f;
        [SerializeField] private float blockedShakeStrength = 0.06f;
        [SerializeField] private int blockedShakeVibrato = 8;
        [SerializeField] private float blockedShakeRandomness = 90f;
        [SerializeField] private bool blockedShakeFadeOut = true;

        private Tween blockedShakeTween;

        protected virtual void Awake()
        {
             var p = transform.position;

            // 防止 tileSize 为 0
            float sx = 1;
            float sy = 1;

            // “向下取整到格子起点”再“+半格到中心”
            float cx = Mathf.Floor(p.x / sx) * sx + sx * 0.5f;
            float cy = Mathf.Floor(p.y / sy) * sy + sy * 0.5f;

            transform.position = new Vector3(cx, cy, 0f);

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
            Debug.Log($"[ReadIntent] {name} mind={(mind == null ? "null" : mind.GetType().Name)}");
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
            if (!isAlive) return;
            isAlive = false;
            AudioManager.Instance?.PlayDeathSFX();
            OnKilled?.Invoke(this);
            gameObject.SetActive(false);
        }

        public virtual float PlayMoveAnimation(Vector3 from, Vector3 to, int tiles)
        {
            // 默认无动画
            return 0f;
        }
        public virtual void PlayBlockedAnimation()
        {
            if (!isAlive) return;

            var target = blockedShakeTarget != null ? blockedShakeTarget : transform;
            if (target == null) return;

            if (blockedShakeTween != null && blockedShakeTween.IsActive())
                blockedShakeTween.Kill();

            var origin = target.localPosition;
            blockedShakeTween = target
                .DOShakePosition(blockedShakeDuration, blockedShakeStrength, blockedShakeVibrato, blockedShakeRandomness, false, blockedShakeFadeOut)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (target != null)
                        target.localPosition = origin;
                });
        }

        public virtual void SetSpriteDirection(MoveDir d)
        {
            currentDir = d;
            if(spriteRenderer == null || spriteList == null || spriteList.Count < 4) return;
            if(MoveDir.Up == d)
            {
                spriteRenderer.sprite = spriteList[0];
                transform.localScale = new Vector3(1,1,1);
            } 
            else if(MoveDir.Down == d)
            {
                spriteRenderer.sprite = spriteList[1];
                transform.localScale = new Vector3(1,1,1);
            }
            else if(MoveDir.Left == d)
            {
                spriteRenderer.sprite = spriteList[2];
                transform.localScale = new Vector3(-1,1,1);
            }
            else if(MoveDir.Right == d)
            {
                spriteRenderer.sprite = spriteList[3];
                transform.localScale = new Vector3(1,1,1);
            }
        }
    }
}


