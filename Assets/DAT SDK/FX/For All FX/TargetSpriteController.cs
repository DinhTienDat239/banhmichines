using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/TargetSpriteController")]
    public class TargetSpriteController : MonoBehaviour
    {
        [Header("Target UI Images (set in order)")]
        public List<Image> targetImages = new List<Image>();

        [Header("Sprites (the same order as targets)")]
        public List<Sprite> newSprites = new List<Sprite>();

        [Header("Transition Settings")]
        [Tooltip("Làm mờ trước khi đổi sprite và hiện lại")]
        public bool enableFade = true;
        public float fadeDuration = 0.3f;

        [Tooltip("Hiệu ứng scale khi đổi sprite")]
        public bool enableScaleEffect = false;
        public float scaleFactor = 1.2f;
        public float scaleDuration = 0.2f;

        [Header("Auto Reset Settings")]
        [Tooltip("Tự động đổi lại sprite gốc sau một khoảng thời gian")]
        public bool autoReset = false;
        public float resetDelay = 1.5f;

        private List<Sprite> originalSprites = new List<Sprite>();

        private void Awake()
        {
            CacheOriginalSprites();
        }

        private void CacheOriginalSprites()
        {
            originalSprites.Clear();
            foreach (var img in targetImages)
            {
                originalSprites.Add(img != null ? img.sprite : null);
            }
        }

        public void ChangeSpriteAtIndex(int index)
        {
            if (index < 0 || index >= targetImages.Count || index >= newSprites.Count)
                return;

            Image img = targetImages[index];
            Sprite newSprite = newSprites[index];
            Sprite oldSprite = (index < originalSprites.Count) ? originalSprites[index] : null;

            if (img == null || newSprite == null) return;

            Sequence seq = DOTween.Sequence().SetUpdate(true);

            if (enableFade)
            {
                seq.Append(img.DOFade(0f, fadeDuration / 2f));
                seq.AppendCallback(() =>
                {
                    img.sprite = newSprite;
                });
                seq.Append(img.DOFade(1f, fadeDuration / 2f));
            }
            else
            {
                img.sprite = newSprite;
            }

            if (enableScaleEffect)
            {
                Vector3 original = img.rectTransform.localScale;

                seq.Join(img.rectTransform.DOScale(original * scaleFactor, scaleDuration / 2f)
                    .SetEase(Ease.OutBack)
                    .SetLoops(2, LoopType.Yoyo));
            }

            // ✅ Trả về sprite gốc sau delay
            if (autoReset && oldSprite != null)
            {
                seq.AppendInterval(resetDelay);
                seq.AppendCallback(() =>
                {
                    img.sprite = oldSprite;
                });
            }
        }

        public void ResetAllSprites(Sprite defaultSprite)
        {
            for (int i = 0; i < targetImages.Count; i++)
            {
                if (targetImages[i] != null)
                    targetImages[i].sprite = defaultSprite;
            }
        }
    }
}
