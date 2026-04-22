using DAT.Core.DesignPatterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectUIManager : Singleton<ObjectUIManager>
{
    [SerializeField] public Image fullPanel;
    [SerializeField] public Image buttonsPanel;
    [SerializeField] public Button rotateBtn;
    [SerializeField] public Button sellBtn;
    [SerializeField] public Button upgradeBtn;
    [SerializeField] public Image setupPanel;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 fullPanelWorldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

    private readonly Dictionary<GameObject, Coroutine> fadeCoroutines = new Dictionary<GameObject, Coroutine>();
    private InteractableObject displayedInteractable;
    private RectTransform fullPanelRectTransform;
    private RectTransform rootCanvasRectTransform;
    private Canvas rootCanvas;

    protected override void Awake()
    {
        base.Awake();

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        if (fullPanel == null)
        {
            return;
        }

        fullPanelRectTransform = fullPanel.rectTransform;
        rootCanvas = fullPanel.GetComponentInParent<Canvas>();
        rootCanvasRectTransform = rootCanvas != null ? rootCanvas.transform as RectTransform : null;
    }

    private void Start()
    {
        HideUIImmediate();
    }

    private void LateUpdate()
    {
        SyncSelectionFromPlayerInteraction();
    }

    private void OnDisable()
    {
        foreach (Coroutine coroutine in fadeCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        fadeCoroutines.Clear();
    }

    private void SyncSelectionFromPlayerInteraction()
    {
        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        InteractableObject selectedInteractable = playerInteraction != null
            ? playerInteraction.SelectedInteractable
            : null;

        if (selectedInteractable != displayedInteractable)
        {
            HandleSelectionChanged(selectedInteractable);
        }

        if (selectedInteractable != null)
        {
            UpdateFullPanelPosition(selectedInteractable);
        }
    }

    private void HandleSelectionChanged(InteractableObject newSelectedInteractable)
    {
        displayedInteractable = newSelectedInteractable;
        BindButtons(newSelectedInteractable);

        if (newSelectedInteractable == null)
        {
            HideUI();
            return;
        }

        UpdateFullPanelPosition(newSelectedInteractable);

        FadeIn(fullPanel.gameObject);
        FadeIn(buttonsPanel.gameObject);

        ShowOrHideWithFade(setupPanel.gameObject, newSelectedInteractable.canSetup);
        ShowOrHideWithFade(rotateBtn.gameObject, newSelectedInteractable.canRotate);
        ShowOrHideWithFade(sellBtn.gameObject, newSelectedInteractable.canSell);
        ShowOrHideWithFade(upgradeBtn.gameObject, newSelectedInteractable.canUpgrade);
    }

    public void HideUI()
    {
        BindButtons(null);

        FadeOut(fullPanel.gameObject);
        FadeOut(buttonsPanel.gameObject);
        FadeOut(setupPanel.gameObject);
        FadeOut(rotateBtn.gameObject);
        FadeOut(sellBtn.gameObject);
        FadeOut(upgradeBtn.gameObject);
    }

    private void HideUIImmediate()
    {
        displayedInteractable = null;
        BindButtons(null);

        SetHiddenImmediately(fullPanel.gameObject);
        SetHiddenImmediately(buttonsPanel.gameObject);
        SetHiddenImmediately(setupPanel.gameObject);
        SetHiddenImmediately(rotateBtn.gameObject);
        SetHiddenImmediately(sellBtn.gameObject);
        SetHiddenImmediately(upgradeBtn.gameObject);
    }

    private void UpdateFullPanelPosition(InteractableObject interactableObject)
    {
        if (interactableObject == null || fullPanelRectTransform == null)
        {
            return;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
            if (worldCamera == null)
            {
                return;
            }
        }

        Vector3 targetWorldPosition = interactableObject.transform.position + fullPanelWorldOffset;
        Vector3 screenPoint = worldCamera.WorldToScreenPoint(targetWorldPosition);

        if (rootCanvasRectTransform == null)
        {
            fullPanelRectTransform.position = screenPoint;
            return;
        }

        Camera uiCamera = rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas != null ? rootCanvas.worldCamera : worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            return;
        }

        fullPanelRectTransform.anchoredPosition = localPoint;
    }

    private void BindButtons(InteractableObject interactableObject)
    {
        if (rotateBtn != null)
        {
            rotateBtn.onClick.RemoveAllListeners();
            if (interactableObject != null)
            {
                rotateBtn.onClick.AddListener(interactableObject.Rotate);
            }
        }

        if (sellBtn != null)
        {
            sellBtn.onClick.RemoveAllListeners();
            if (interactableObject != null)
            {
                sellBtn.onClick.AddListener(interactableObject.Sell);
            }
        }

        if (upgradeBtn != null)
        {
            upgradeBtn.onClick.RemoveAllListeners();
            if (interactableObject != null)
            {
                upgradeBtn.onClick.AddListener(interactableObject.LevelUp);
            }
        }
    }

    private void ShowOrHideWithFade(GameObject target, bool shouldShow)
    {
        if (shouldShow)
        {
            FadeIn(target);
        }
        else
        {
            FadeOut(target);
        }
    }

    private void FadeIn(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (fadeCoroutines.TryGetValue(target, out Coroutine runningCoroutine) && runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }

        fadeCoroutines[target] = StartCoroutine(FadeRoutine(target, 1f, deactivateOnFinish: false));
    }

    private void FadeOut(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (fadeCoroutines.TryGetValue(target, out Coroutine runningCoroutine) && runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }

        fadeCoroutines[target] = StartCoroutine(FadeRoutine(target, 0f, deactivateOnFinish: true));
    }

    private IEnumerator FadeRoutine(GameObject target, float targetAlpha, bool deactivateOnFinish)
    {
        CanvasGroup canvasGroup = GetOrAddCanvasGroup(target);

        if (targetAlpha > 0f && !target.activeSelf)
        {
            target.SetActive(true);
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        if (fadeDuration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
        }
        else
        {
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        if (deactivateOnFinish && Mathf.Approximately(targetAlpha, 0f))
        {
            target.SetActive(false);
        }

        if (target.activeSelf && targetAlpha > 0f)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        fadeCoroutines[target] = null;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    private void SetHiddenImmediately(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        CanvasGroup canvasGroup = GetOrAddCanvasGroup(target);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        target.SetActive(false);
    }
}
