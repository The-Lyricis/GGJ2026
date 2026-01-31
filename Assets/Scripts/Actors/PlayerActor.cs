using DG.Tweening;
using UnityEngine;
namespace GGJ2026
{
    public class PlayerActor : BaseActor
    {
        [Header("Mask/Control")]
        [SerializeField] private FactionColor controlColor = FactionColor.White;

        [Header("Minds")]
        [SerializeField] private PlayerMind playerMind;
        [SerializeField] private SymbiosisMind symbiosisMind;

        public override FactionColor ControlColor => controlColor;
        public override FactionColor CombatColor => controlColor;

        public void EquipMask(FactionColor color)
        {
            controlColor = color;

            // 切到共生 Mind
            if (symbiosisMind != null)
                SetMind(symbiosisMind);
        }

        public void UnequipMask()
        {
            controlColor = FactionColor.White;

            // 切回玩家 Mind
            if (playerMind != null)
                SetMind(playerMind);
        }
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