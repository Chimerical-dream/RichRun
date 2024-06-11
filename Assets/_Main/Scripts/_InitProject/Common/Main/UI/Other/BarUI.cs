using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.UI {
	[AddComponentMenu("Common/UI/Bar")]
    public class BarUI : MonoBehaviour {
        public Ease ChangeEase = Ease.InOutSine;

        /// <summary>
        /// [0..1] Target bar fill amount (not current, if change anim is on)
        /// </summary>
        public float FillAmount => _fillAmount;

        [SerializeField] protected Image _fillImage;
        
        protected float _fillAmount;
        
        public virtual void Init(float fillAmount) {
            _fillAmount = fillAmount;
            _fillImage.fillAmount = fillAmount;
        }

        /// <param name="fillAmount">[0..1]</param>
        public virtual void ChangeFill(float fillAmount, float changeTime = 0f) {
            fillAmount = Mathf.Clamp01(fillAmount);
            _fillAmount = fillAmount;

            if (changeTime > 0f) {
                DoSmoothFillChange(fillAmount, changeTime);
            }
            else {
                if (fillChangeTween != null && fillChangeTween.active) {
                    fillChangeTween.Kill(true);
                }
                _fillImage.fillAmount = fillAmount;
            }
        }

        public virtual void StopFillChange(bool reachTarget) {
            if (fillChangeTween != null && fillChangeTween.active) {
                fillChangeTween.Kill(reachTarget);
                _fillAmount = _fillImage.fillAmount;
            }
        }

        private Tween fillChangeTween;
        private void DoSmoothFillChange(float targetFill, float changeTime) {
            if (fillChangeTween != null && fillChangeTween.active) {
                fillChangeTween.Kill(true);
            }

            fillChangeTween = DOTween.To(() => _fillImage.fillAmount, x => _fillImage.fillAmount = x, targetFill, changeTime)
                .SetEase(ChangeEase)
                .OnComplete(() => {
                    _fillImage.fillAmount = targetFill;
                });
        }

        private void OnDestroy() {
            StopFillChange(false);
        }
    }
}