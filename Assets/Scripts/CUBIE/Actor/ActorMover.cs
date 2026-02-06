using UnityEngine;
using DG.Tweening;

namespace CUBIE
{
    public class ActorMover : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        [Header("Animator Params")]
        [SerializeField] private string moveXParam = "MoveX";
        [SerializeField] private string moveYParam = "MoveY";
        [SerializeField] private string isMovingParam = "IsMoving";
        [SerializeField] private string bumpTrigger = "Bump";
        [SerializeField] private string interactTrigger = "Interact";

        private Tween moveTween;

        public float MoveTo(Vector3 target, Vector2Int dir, float duration)
        {
            if (duration <= 0f)
            {
                transform.position = target;
                SetMoving(false, dir);
                return 0f;
            }

            moveTween?.Kill();
            SetMoving(true, dir);
            moveTween = transform.DOMove(target, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => SetMoving(false, dir));

            return duration;
        }

        public void PlayBump(Vector2Int dir)
        {
            SetDir(dir);
            if (!string.IsNullOrEmpty(bumpTrigger) && animator != null)
                animator.SetTrigger(bumpTrigger);
        }

        public void PlayInteract()
        {
            if (!string.IsNullOrEmpty(interactTrigger) && animator != null)
                animator.SetTrigger(interactTrigger);
        }

        // Animator + coroutine version (kept for reference)
        // private IEnumerator MoveRoutine(Vector3 target, Vector2Int dir, float duration)
        // {
        //     SetMoving(true, dir);
        //     var start = transform.position;
        //     float t = 0f;
        //     while (t < duration)
        //     {
        //         t += Time.deltaTime;
        //         float k = Mathf.Clamp01(t / duration);
        //         transform.position = Vector3.Lerp(start, target, k);
        //         yield return null;
        //     }
        //
        //     transform.position = target;
        //     SetMoving(false, dir);
        // }

        // Animator hook: movement state + facing direction.
        private void SetMoving(bool moving, Vector2Int dir)
        {
            SetDir(dir);
            if (animator != null && !string.IsNullOrEmpty(isMovingParam))
                animator.SetBool(isMovingParam, moving);
        }

        // Animator hook: set facing direction (MoveX/MoveY).
        private void SetDir(Vector2Int dir)
        {
            if (animator == null) return;
            if (!string.IsNullOrEmpty(moveXParam)) animator.SetFloat(moveXParam, dir.x);
            if (!string.IsNullOrEmpty(moveYParam)) animator.SetFloat(moveYParam, dir.y);
        }
    }
}
