using UnityEngine;
using DG.Tweening;
using DAT.Core.Enums;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/PulseEffect")]
    public class PulseEffect : MonoBehaviour
    {
        [Header("Pulse Mode")]
        public PulseMode pulseMode = PulseMode.PulseIn;

        [Header("Pulse Settings")]
        [Tooltip("Độ phóng lớn nhất (PulseIn) hoặc nhỏ nhất (PulseOut)")]
        public float pulseScale = 1.1f;
        [Tooltip("Thời gian cho toàn bộ 1 chu kỳ (1 nửa snap)")]
        public float duration = 0.6f;
        [Tooltip("Tự động bắt đầu khi vật thể xuất hiện (OnEnable)")]
        public bool autoStart = true;
        [Tooltip("Thời gian chờ trước khi bắt đầu")]
        public float startDelay = 0f;

        [Header("Tween Settings")]
        [Tooltip("Lựa chọn hiệu ứng")]
        public Ease ease = Ease.InOutSine;
        [Tooltip("Lặp lại hiệu ứng")]
        public bool loop = true;
        [Tooltip("Giật về trạng thái ban đầu thay vì chuyển động đảo ngược")]
        public bool snap = false;
        [Tooltip("Thời gian chờ giữa các chu kỳ")]
        public float delayBetween = 0f;

        private Vector3 _originalScale;
        public Vector3 GetOriginalScale() {  return _originalScale; }
        private Tween _pulseTween;

        void Awake()
        {
            _originalScale = transform.localScale;
        }

        void OnEnable()
        {
            if (autoStart)
            {
                if (startDelay > 0)
                    Invoke(nameof(StartPulse), startDelay);
                else
                    StartPulse();
            }
        }

        void OnDisable()
        {
            StopPulse();
        }

        public void StartPulse()
        {
            StopPulse();

            switch (pulseMode)
            {
                case PulseMode.PulseIn:
                    BuildPulseTween(
                        toScale: _originalScale * pulseScale,
                        stepCount: 2
                    );
                    break;

                case PulseMode.PulseOut:
                    BuildPulseTween(
                        toScale: _originalScale * (1f / pulseScale),
                        stepCount: 2
                    );
                    break;

                case PulseMode.PulseInThenOut:
                    BuildPulseTween(
                        toScaleA: _originalScale * pulseScale,
                        toScaleB: _originalScale * (1f / pulseScale),
                        stepCount: 3
                    );
                    break;
            }
        }

        private void BuildPulseTween(Vector3 toScale, int stepCount)
        {
            float stepDuration = duration / stepCount;
            Sequence seq = DOTween.Sequence().SetUpdate(true);

            seq.Append(transform.DOScale(toScale, stepDuration).SetEase(ease));

            if (snap)
            {
                seq.AppendCallback(() => transform.localScale = _originalScale);
            }
            else
            {
                seq.Append(transform.DOScale(_originalScale, stepDuration).SetEase(ease));
            }

            seq.AppendInterval(delayBetween);
            if(loop)
            {
                seq.SetLoops(-1);
            }
            _pulseTween = seq;
        }

        private void BuildPulseTween(Vector3 toScaleA, Vector3 toScaleB, int stepCount)
        {
            float stepDuration = duration / stepCount;
            Sequence seq = DOTween.Sequence().SetUpdate(true);

            seq.Append(transform.DOScale(toScaleA, stepDuration).SetEase(ease));
            seq.Append(transform.DOScale(toScaleB, stepDuration).SetEase(ease));

            if (snap)
            {
                seq.AppendCallback(() => transform.localScale = _originalScale);
            }
            else
            {
                seq.Append(transform.DOScale(_originalScale, stepDuration).SetEase(ease));
            }

            seq.AppendInterval(delayBetween);
            if (loop)
            {
                seq.SetLoops(-1);
            }
            _pulseTween = seq;
        }

        public void StopPulse()
        {
            _pulseTween?.Kill();
            transform.localScale = _originalScale;
        }
    }
}
