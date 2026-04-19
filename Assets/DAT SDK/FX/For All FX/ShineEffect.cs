using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DAT.UI.FX
{
    [ExecuteInEditMode]
    [AddComponentMenu("DAT SDK/UI/ShineEffect")]
    public class ShineEffect : MonoBehaviour
    {
        [Header("Shine Settings")]
        [Tooltip("Sprite được dùng để tạo hiệu ứng lấp lánh.")]
        public Sprite shineSprite;
        [Tooltip("Object con chứa Image hiển thị hiệu ứng lấp lánh.")]
        public RectTransform shineObject;
        [Tooltip("Tự động bắt đầu khi vật thể xuất hiện (OnEnable).")]
        public bool autoStart = true;
        [Tooltip("Thời gian cho toàn bộ 1 chu kỳ.")]
        public float duration = 0.6f;

        [Header("Tween Settings")]
        [Tooltip("Khoảng thời gian giữa các chu kì.")]
        public float delayBetween = 0f;
        [Tooltip("Lặp lại hiệu ứng.")]
        public bool loop = true;

        private Image _image;
        private Tween _shineTween;

        private void OnValidate()
        {
            // Đảm bảo có component Mask
            Mask mask = GetComponent<Mask>();
            if (mask == null)
            {
                mask = gameObject.AddComponent<Mask>();
                mask.showMaskGraphic = true;
            }

            // Đảm bảo có object con ShineObject
            if (shineObject == null)
            {
                Transform existing = transform.Find("ShineObject");
                if (existing != null)
                {
                    shineObject = existing.GetComponent<RectTransform>();
                    return;
                }

                // Tạo object con mới
                GameObject child = new GameObject("ShineObject", typeof(RectTransform), typeof(Image));
                child.transform.SetParent(transform, false);

                RectTransform rect = child.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                Image img = child.GetComponent<Image>();
                img.sprite = shineSprite;
                img.raycastTarget = false;

                shineObject = rect;
            }
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (autoStart)
                StartShine();
        }

        private void OnDisable()
        {
            StopShine();
        }

        public void StartShine()
        {
            StopShine();

            float width = _image.rectTransform.sizeDelta.x;
            shineObject.anchoredPosition = new Vector2(-width, 0);

            Sequence shineSeq = DOTween.Sequence().SetUpdate(true);
            shineSeq.Append(shineObject.DOAnchorPos(new Vector2(-width, 0), 0));
            shineSeq.Append(shineObject.DOAnchorPos(new Vector2(width, 0), duration));
            shineSeq.AppendInterval(delayBetween);

            if (loop)
                shineSeq.SetLoops(-1);

            _shineTween = shineSeq;
        }

        public void StopShine()
        {
            _shineTween?.Kill();
            _shineTween = null;
        }
    }
}
