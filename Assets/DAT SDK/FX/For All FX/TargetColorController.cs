using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/TargetColorController")]
    public class TargetColorController : MonoBehaviour
    {
        [Header("Target UI Images (set in order)")]
        public List<Image> targetImages = new List<Image>();

        [Header("Color Settings")]
        public Color normalColor = Color.white;
        public Color highlightColor = Color.yellow;
        public float transitionTime = 0.25f;

        [Header("Auto Reset Settings")]
        [Tooltip("Tự động trở về màu gốc sau khi highlight")]
        public bool autoReset = false;

        [Tooltip("Thời gian chờ trước khi trở về màu gốc")]
        public float resetDelay = 0.5f;

        private void Start()
        {
            ResetAll();
        }

        public void HighlightAtIndex(int index)
        {
            if (index < 0 || index >= targetImages.Count || targetImages[index] == null)
                return;

            Debug.Log($"Highlighting index: {index}");

            for (int i = 0; i < targetImages.Count; i++)
            {
                Image img = targetImages[i];
                if (img == null) continue;

                if (i == index)
                {
                    img.DOColor(highlightColor, transitionTime).SetUpdate(true);

                    if (autoReset)
                    {
                        // Reset lại về màu cũ sau delay
                        img.DOColor(normalColor, transitionTime)
                           .SetDelay(resetDelay)
                           .SetEase(Ease.InOutSine)
                           .SetUpdate(true);
                    }
                }
                else
                {
                    img.DOColor(normalColor, transitionTime).SetUpdate(true);
                }
            }
        }

        public void ResetAll()
        {
            foreach (var img in targetImages)
            {
                if (img != null)
                {
                    img.color = normalColor;
                }
            }
        }
    }
}
