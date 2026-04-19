using DG.Tweening;
using UnityEngine;
using DAT.Core.Enums;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/RotationEffect")]
    public class RotationEffect : MonoBehaviour
    {
        [Header("Rotation Mode")]
        public RotationMode rotationMode = RotationMode.LeftRight;

        [Header("Rotation Settings")]
        [Tooltip("Góc cần xoay (theo trục Z)")]
        public float rotationAngle = 30f;
        [Tooltip("Thời gian cho toàn bộ 1 chu kỳ (1 nửa hoặc 1/3 khi snap)")]
        public float duration = 0.6f;
        [Tooltip("Tự động bắt đầu khi vật thể xuất hiện (OnEnable)")]
        public bool autoStart = true;
        [Tooltip("Thời gian chờ trước khi bắt đầu")]
        public float startDelay = 0f;

        [Header("Tween Settings")]
        public Ease ease = Ease.Linear;
        [Tooltip("Lặp lại hiệu ứng")]
        public bool loop = true;
        [Tooltip("Giật về trạng thái ban đầu thay vì chuyển động đảo ngược")]
        public bool snap = false;
        [Tooltip("Thời gian chờ giữa các chu kì")]
        public float delayBetween = 0f;

        private Tween _rotationTween;
        private Quaternion _originalRotation;
        public Quaternion GetOriginalRotation() {  return _originalRotation; }

        void Awake()
        {
            _originalRotation = transform.rotation;
        }

        void OnEnable()
        {
            if (autoStart)
            {
                if (startDelay > 0)
                    Invoke(nameof(StartRotation), startDelay);
                else
                    StartRotation();
            }
        }

        void OnDisable()
        {
            StopRotation();
        }

        public void StartRotation()
        {
            StopRotation();

            Vector3 baseEuler = transform.eulerAngles;
            Vector3 leftTarget = baseEuler + new Vector3(0, 0, Mathf.Abs(rotationAngle));
            Vector3 rightTarget = baseEuler - new Vector3(0, 0, Mathf.Abs(rotationAngle));

            //Tính toán stepDuration
            float stepDuration = duration; // default
            switch (rotationMode)
            {
                case RotationMode.Left:
                case RotationMode.Right:
                    stepDuration = duration / 2f;
                    break;

                case RotationMode.RightLeft:
                case RotationMode.LeftRight:
                    stepDuration = duration / 3f;
                    break;

                case RotationMode.ContinuousLeft:
                case RotationMode.ContinuousRight:
                    stepDuration = duration;
                    break;
            }

            //Tạo Rotation dựa trên Mode
            switch (rotationMode)
            {
                case RotationMode.Left:
                    _rotationTween = CreateRotation(leftTarget, stepDuration);
                    break;

                case RotationMode.Right:
                    _rotationTween = CreateRotation(rightTarget, stepDuration);
                    break;

                case RotationMode.LeftRight:
                    _rotationTween = CreateRotation(leftTarget ,rightTarget, stepDuration);
                    break;

                case RotationMode.RightLeft:
                    _rotationTween = CreateRotation(rightTarget, leftTarget, stepDuration);
                    break;

                case RotationMode.ContinuousLeft:
                    _rotationTween = transform.DORotate(new Vector3(0, 0, 360f), stepDuration, RotateMode.FastBeyond360)
                        .SetEase(ease)
                        .SetLoops(loop ? -1 : 0)
                        .SetDelay(delayBetween)
                        .SetUpdate(true);
                    break;

                case RotationMode.ContinuousRight:
                    _rotationTween = transform.DORotate(new Vector3(0, 0, -360f), stepDuration, RotateMode.FastBeyond360)
                        .SetEase(ease)
                        .SetLoops(loop ? -1 : 0)
                        .SetDelay(delayBetween)
                        .SetUpdate(true);
                    break;
            }
        }

        private Tween CreateRotation(Vector3 target, Vector3 secondTarget, float stepDuration)
        {
            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(transform.DORotate(target, stepDuration).SetEase(ease));
            seq.Append(transform.DORotate(secondTarget, stepDuration).SetEase(ease));

            if (snap)
            {
                seq.AppendCallback(() => transform.rotation = _originalRotation);
            }
            else
            {
                seq.Append(transform.DORotate(_originalRotation.eulerAngles, stepDuration).SetEase(ease));
            }

            seq.AppendInterval(delayBetween);
            if (loop) seq.SetLoops(-1);

            return seq;
        }
        private Tween CreateRotation(Vector3 target, float stepDuration)
        {
            if (snap)
            {
                Sequence seq = DOTween.Sequence().SetUpdate(true);
                seq.Append(transform.DORotate(target, stepDuration).SetEase(ease));
                seq.AppendCallback(() => transform.rotation = _originalRotation);
                seq.AppendInterval(delayBetween);
                if (loop) 
                    seq.SetLoops(-1);
                return seq;
            }
            else
            {
                Sequence seq = DOTween.Sequence().SetUpdate(true);
                seq.Append(transform.DORotate(target, stepDuration).SetEase(ease));
                seq.Append(transform.DORotate(_originalRotation.eulerAngles, stepDuration).SetEase(ease));
                seq.AppendInterval(delayBetween);
                if (loop)
                    seq.SetLoops(-1);
                return seq;
            }
        }

        public void StopRotation()
        {
            _rotationTween?.Kill();
            transform.rotation = _originalRotation;
        }
    }
}
