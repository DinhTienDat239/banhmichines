using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DAT.Core.Enums;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/TargetPulseController")]
    public class TargetPulseController : MonoBehaviour
    {
        [Header("Target Objects (Transform or RectTransform) (set in order)")]
        [Tooltip("Danh sách các object sẽ được scale pulse theo index")]
        public List<Transform> targetObjects = new List<Transform>();

        [Header("Pulse Mode")]
        [Tooltip("Chọn kiểu pulse: In / Out / In-Then-Out")]
        public DAT.Core.Enums.PulseMode pulseMode = DAT.Core.Enums.PulseMode.PulseIn;

        [Header("Pulse Settings")]
        [Tooltip("Độ lớn của scale khi pulse")]
        public float pulseScale = 1.2f;

        [Tooltip("Thời gian cho toàn bộ 1 chu kỳ pulse")]
        public float duration = 0.3f;

        [Tooltip("Ease tween scale")]
        public Ease ease = Ease.OutBack;

        [Tooltip("Snap về scale gốc sau pulse")]
        public bool snap = false;

        public void PulseAtIndex(int index)
        {
            if (index < 0 || index >= targetObjects.Count || targetObjects[index] == null)
                return;

            Transform target = targetObjects[index];
            Vector3 originalScale = target.localScale;

            Sequence seq = DOTween.Sequence().SetUpdate(true);

            switch (pulseMode)
            {
                case DAT.Core.Enums.PulseMode.PulseIn:
                    float stepInDuration = duration / 2f;

                    seq.Append(target.DOScale(originalScale * pulseScale, stepInDuration).SetEase(ease));

                    if (snap)
                        seq.AppendCallback(() => target.localScale = originalScale);
                    else
                        seq.Append(target.DOScale(originalScale, stepInDuration).SetEase(ease));
                    break;

                case DAT.Core.Enums.PulseMode.PulseOut:
                    float stepOutDuration = duration / 2f;

                    seq.Append(target.DOScale(originalScale * (1f / pulseScale), stepOutDuration).SetEase(ease));

                    if (snap)
                        seq.AppendCallback(() => target.localScale = originalScale);
                    else
                        seq.Append(target.DOScale(originalScale, stepOutDuration).SetEase(ease));
                    break;

                case DAT.Core.Enums.PulseMode.PulseInThenOut:
                    float stepPulseDuration = duration / 3f;
                    Vector3 scaleIn = originalScale * pulseScale;
                    Vector3 scaleOut = originalScale * (1f / pulseScale);

                    seq.Append(target.DOScale(scaleIn, stepPulseDuration).SetEase(ease));
                    seq.Append(target.DOScale(scaleOut, stepPulseDuration).SetEase(ease));

                    if (snap)
                        seq.AppendCallback(() => target.localScale = originalScale);
                    else
                        seq.Append(target.DOScale(originalScale, stepPulseDuration).SetEase(Ease.InOutQuad));
                    break;
            }
        }

        public void ResetAll()
        {
            foreach (var obj in targetObjects)
            {
                if (obj != null)
                    obj.localScale = Vector3.one;
            }
        }
    }
}
