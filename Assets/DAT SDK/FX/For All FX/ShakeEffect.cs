using UnityEngine;
using DG.Tweening;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/ShakeEffect")]
    public class ShakeEffect : MonoBehaviour
    {
        [Header("Shake Settings")]
        [Tooltip("Cường độ rung lắc theo trục X,Y")]
        public Vector2 strength = new Vector2(10f, 10f);
        [Tooltip("Tần suất rung (số lần rung trong 1 chu kì)")]
        public int vibrato = 10;
        [Tooltip("Mức ngẫu nhiên hoá góc rung (0 = hướng cố định)")]
        [Range(0f, 180f)]
        public float randomness = 90f;
        [Tooltip("Thời gian cho toàn bộ 1 chu kỳ")]
        public float duration = 0.5f;
        [Tooltip("Tự động bắt đầu khi vật thể xuất hiện (OnEnable).")]
        public bool autoStart = true;
        [Tooltip("Thời gian chờ trước khi bắt đầu")]
        public float startDelay = 0f;

        [Header("Tween Settings")]
        [Tooltip("Lựa chọn hiệu ứng.")]
        public Ease ease = Ease.Linear;
        [Tooltip("Lặp lại hiệu ứng.")]
        public bool loop = true;
        [Tooltip("Khoảng thời gian giữa các chu kì.")]
        public float delayBetween = 0f;

        private Tween _shakeTween;
        private Vector3 _originalPosition;

        void OnEnable()
        {
            _originalPosition = transform.localPosition;
            if (autoStart)
            {
                if (startDelay > 0)
                    Invoke(nameof(StartShake), startDelay);
                else
                    StartShake();
            }
        }

        void OnDisable()
        {
            StopShake();
        }

        public void StartShake()
        {
            StopShake();
            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(transform.DOShakePosition(duration, strength, vibrato, randomness).SetEase(ease));
            seq.AppendInterval(delayBetween);
            if (loop)
            {
                seq.SetLoops(-1);
            }
            _shakeTween = seq;
        }

        public void StopShake()
        {
            _shakeTween?.Kill();
            transform.localPosition = _originalPosition;
        }
    }
}
