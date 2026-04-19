using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/TargetShakeController")]
    public class TargetShakeController : MonoBehaviour
    {
        [Header("Target Objects (Transform or RectTransform) (set in order)")]
        [Tooltip("Danh sách các object sẽ được shake theo index")]
        public List<Transform> targetObjects = new List<Transform>();

        [Header("Shake Settings")]
        [Tooltip("Là UI element (RectTransform) hay Transform thường")]
        public bool isUIElement = false;

        [Tooltip("Cường độ shake theo X, Y")]
        public Vector2 strength = new Vector2(10f, 10f);

        [Tooltip("Thời gian mỗi lần shake")]
        public float duration = 0.5f;

        [Tooltip("Số lần lặp lại (-1 = vô hạn)")]
        public int loopCount = 0;

        [Tooltip("Khoảng thời gian giữa các lần shake")]
        public float delayBetween = 1f;

        [Tooltip("Độ trễ trước khi bắt đầu shake (giây)")]
        public float startDelay = 0f;

        [Tooltip("Tần suất shake (số lần rung trong thời gian duration)")]
        public int vibrato = 10;

        [Tooltip("Mức ngẫu nhiên hoá góc shake (0 = hướng cố định)")]
        [Range(0f, 180f)]
        public float randomness = 90f;

        private Dictionary<int, Tween> activeTweens = new Dictionary<int, Tween>();
        private Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();

        void Awake()
        {
            for (int i = 0; i < targetObjects.Count; i++)
            {
                if (targetObjects[i] != null)
                    originalPositions[i] = isUIElement
                        ? (Vector3)((RectTransform)targetObjects[i]).anchoredPosition
                        : targetObjects[i].localPosition;
            }
        }

        public void ShakeAtIndex(int index)
        {
            if (index < 0 || index >= targetObjects.Count || targetObjects[index] == null)
                return;

            Transform target = targetObjects[index];

            if (activeTweens.ContainsKey(index))
                activeTweens[index].Kill();

            void StartNow()
            {
                Sequence CreateShakeSequence()
                {
                    var seq = DOTween.Sequence();
                    Tween shakeTween = isUIElement
                        ? ((RectTransform)target).DOShakeAnchorPos(duration, strength, vibrato, randomness).SetUpdate(true)
                        : target.DOShakePosition(duration, strength, vibrato, randomness).SetUpdate(true);

                    shakeTween.OnComplete(() =>
                    {
                        if (isUIElement)
                            ((RectTransform)target).anchoredPosition = originalPositions[index];
                        else
                            target.localPosition = originalPositions[index];
                    });

                    seq.Append(shakeTween);
                    return seq;
                }

                if (loopCount == -1)
                {
                    activeTweens[index] = DOTween.Sequence()
                        .Append(CreateShakeSequence())
                        .AppendInterval(delayBetween)
                        .SetLoops(-1)
                        .SetUpdate(true);
                }
                else if (loopCount > 0)
                {
                    Sequence loopSeq = DOTween.Sequence();
                    for (int i = 0; i < loopCount; i++)
                    {
                        loopSeq.Append(CreateShakeSequence());
                        loopSeq.AppendInterval(delayBetween);
                    }
                    loopSeq.SetUpdate(true);
                    activeTweens[index] = loopSeq;
                }
                else
                {
                    activeTweens[index] = CreateShakeSequence();
                }
            }

            if (startDelay > 0f)
            {
                DOVirtual.DelayedCall(startDelay, StartNow, ignoreTimeScale: true);
            }
            else
            {
                StartNow();
            }
        }

        public void ResetAll()
        {
            foreach (var pair in activeTweens)
            {
                pair.Value.Kill();
            }

            activeTweens.Clear();

            foreach (var posPair in originalPositions)
            {
                int idx = posPair.Key;
                if (targetObjects[idx] != null)
                {
                    if (isUIElement)
                        ((RectTransform)targetObjects[idx]).anchoredPosition = posPair.Value;
                    else
                        targetObjects[idx].localPosition = posPair.Value;
                }
            }
        }
    }
}
