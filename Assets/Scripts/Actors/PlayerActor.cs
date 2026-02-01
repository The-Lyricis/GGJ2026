using System.Collections.Generic;
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

        [SerializeField] List<Sprite> maskSpritesRed;
        [SerializeField] List<Sprite> maskSpritesBlue;
        [SerializeField] List<Sprite> maskSpritesStone;
        [SerializeField] SpriteRenderer maskRenderer;

        public override FactionColor ControlColor => controlColor;
        public override FactionColor CombatColor => controlColor;

        public void EquipMask(FactionColor color)
        {
            Debug.Log("Equip Mask: " + color);

            
            controlColor = color;

            SetCurrentMask();

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
        public override void Kill()
        {
            LevelManager.Instance?.RestartCurrentLevel();
            base.Kill();
        }
        public void SetCurrentMask()
        {
            Debug.Log("Set Current Mask Sprite");
            List<Sprite> currentSpriteList = new List<Sprite>();
            if(controlColor == FactionColor.Red)
                    currentSpriteList = maskSpritesRed;
                else if(controlColor == FactionColor.Blue)
                    currentSpriteList = maskSpritesBlue;
                else if(controlColor == FactionColor.Green)
                    currentSpriteList = maskSpritesStone;
                    else
                    return;
            if(currentDir == MoveDir.Up)
            {
                maskRenderer.sprite = currentSpriteList[0];
                transform.localScale = new Vector3(1,1,1);
            } 
            else if(currentDir == MoveDir.Down)
            {
                maskRenderer.sprite = currentSpriteList[1];
                transform.localScale = new Vector3(1,1,1);
            }
            else if(currentDir == MoveDir.Left)
            {
                maskRenderer.sprite = currentSpriteList[2];
                transform.localScale = new Vector3(-1,1,1);
            }
            else if(currentDir == MoveDir.Right)
            {
                maskRenderer.sprite = currentSpriteList[3];
                transform.localScale = new Vector3(1,1,1);
            }
            
        }
        public override void  SetSpriteDirection(MoveDir d)
        {
            base.SetSpriteDirection(d);
            
            List<Sprite> currentSpriteList = new List<Sprite>();
            if(controlColor == FactionColor.Red)
                    currentSpriteList = maskSpritesRed;
                else if(controlColor == FactionColor.Blue)
                    currentSpriteList = maskSpritesBlue;
                else if(controlColor == FactionColor.Green)
                    currentSpriteList = maskSpritesStone;
                    else
                    return;

            if(MoveDir.Up == d)
            {
                maskRenderer.sprite = currentSpriteList[0];
                transform.localScale = new Vector3(1,1,1);
            } 
            else if(MoveDir.Down == d)
            {
                maskRenderer.sprite = currentSpriteList[1];
                transform.localScale = new Vector3(1,1,1);
            }
            else if(MoveDir.Left == d)
            {
                maskRenderer.sprite = currentSpriteList[2];
                transform.localScale = new Vector3(-1,1,1);
            }
            else if(MoveDir.Right == d)
            {
                maskRenderer.sprite = currentSpriteList[3];
                transform.localScale = new Vector3(1,1,1);
            }
        }


    }

}