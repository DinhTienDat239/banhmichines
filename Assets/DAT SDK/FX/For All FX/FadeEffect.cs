using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DAT.Core.Enums;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/FadeEffect")]
    [RequireComponent(typeof(Image))]
    public class FadeEffect : MonoBehaviour
    {
        [Header("Fade Mode")]
        public FadeMode fadeMode = FadeMode.FadeReturn;

        [Header("Fade Settings")]
        [Range(0f, 1f)] public float startAlpha = 0.3f;
        [Range(0f, 1f)] public float endAlpha = 1f;
        [Tooltip("Thời gian cho toàn bộ 1 chu kỳ pulse (1 nửa khi snap)")]
        public float duration = 0.6f;
        [Tooltip("Tự động bắt đầu khi vật thể xuất hiện (OnEnable)")]
        public bool autoStart = true;
        [Tooltip("Thời gian chờ trước khi bắt đầu")]
        public float startDelay = 0f;

        [Header("Tween Settings")]
        public Ease ease = Ease.InOutSine;
        [Tooltip("Lặp lại hiệu ứng")]
        public bool loop = true;
        [Tooltip("Thời gian chờ giữa các chu kỳ")]
        public float delayBetween = 0f;

        private Image _image;
        private float _originalAlpha;
        private Tween _fadeTween;

        void Awake()
        {
            _image = GetComponent<Image>();
            _originalAlpha = _image.color.a;
        }

        void OnEnable()
        {
            if (autoStart)
            {
                if (startDelay > 0)
                    Invoke(nameof(StartFade), startDelay);
                else
                    StartFade();
            }
        }

        void OnDisable()
        {
            StopFade();
        }

        public void StartFade()
        {
            StopFade();

            float stepDuration = duration / 2;
            Color baseColor = _image.color;
            baseColor.a = startAlpha;
            _image.color = baseColor;
            Sequence fadeSeq = DOTween.Sequence().SetUpdate(true);

            switch (fadeMode)
            {
                case FadeMode.FadeReturn:
                    fadeSeq.Append(_image.DOFade(endAlpha, stepDuration).SetEase(ease));
                    fadeSeq.Append(_image.DOFade(startAlpha, stepDuration).SetEase(ease));
                    fadeSeq.AppendInterval(delayBetween);
                    if (loop)
                        fadeSeq.SetLoops(-1);
                    break;


                case FadeMode.FadeSnap:
                    fadeSeq.Append(_image.DOFade(endAlpha, stepDuration).SetEase(ease));
                    fadeSeq.AppendCallback(() =>
                    {
                        Color c = _image.color;
                        c.a = startAlpha;
                        _image.color = c;
                    });
                    fadeSeq.AppendInterval(delayBetween);
                    if (loop) 
                        fadeSeq.SetLoops(-1);
                    break;
                case FadeMode.Fade:
                    fadeSeq.Append(_image.DOFade(endAlpha, stepDuration).SetEase(ease));
                    fadeSeq.AppendInterval(delayBetween);

                    if (loop)
                    {
                        fadeSeq.AppendCallback(() =>
                        {
                            Color c = _image.color;
                            c.a = startAlpha;
                            _image.color = c;
                        });
                        fadeSeq.SetLoops(-1);
                    }
                    break;
            }
            _fadeTween = fadeSeq;
        }

        public void StopFade()
        {
            _fadeTween?.Kill();
            if (_image != null)
            {
                Color c = _image.color;
                c.a = _originalAlpha;
                _image.color = c;
            }
        }
    }
}
