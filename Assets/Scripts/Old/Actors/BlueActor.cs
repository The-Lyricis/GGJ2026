using DG.Tweening;
using UnityEngine;

namespace GGJ2026
{
    public class BlueActor : BaseActor
    {
        [SerializeField] private FactionColor controlColor = FactionColor.Blue;

        public override FactionColor ControlColor
        {
            get => controlColor;
            set => controlColor = value;
        }

        public override FactionColor CombatColor => FactionColor.Blue;

        [SerializeField] private Transform visual; // 建议是子物体
        [SerializeField] private float baseMoveTime = 0.2f;
        public override float PlayMoveAnimation(Vector3 from, Vector3 to, int tiles)
        {
            float duration = baseMoveTime * Mathf.Max(1, tiles);
            transform.position = from;
            transform.DOMove(to, duration).SetEase(Ease.InOutSine);

            Sequence seq = DOTween.Sequence();
            seq.Append(visual.DOScale(new Vector3(1.2f, 0.8f, 1f), duration * 0.2f));
            seq.Append(visual.DOScale(new Vector3(0.9f, 1.1f, 1f), duration * 0.6f));
            seq.Append(visual.DOScale(Vector3.one, duration * 0.2f));

            return duration;
        }
    }
}