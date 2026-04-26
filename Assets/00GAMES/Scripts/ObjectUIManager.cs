using DAT.Core.DesignPatterns;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjectUIManager : Singleton<ObjectUIManager>
{   [Header("GameUI")]
    [SerializeField]
    TextMeshProUGUI levelTxt;
    [SerializeField]
    Image goalPanel;
    [SerializeField]
    Button runBtn;
    [SerializeField]
    Button pauseBtn;
    [SerializeField]
    Button restartBtn;
    [SerializeField]
    RectTransform buyPanel;
    [SerializeField]
    Transform buyObjectPanelContent;
    [SerializeField]
    GameObject detailObjectBuyHoverPanel;
    [SerializeField]
    TextMeshProUGUI detailObjectBuyHoverTxt;
    [SerializeField]
    public GameObject UIBuyItemSelectionPref;
    [SerializeField]
    TextMeshProUGUI moneyTxt;
    [Header("Buy Hover Detail")]
    [SerializeField, Min(0f)] private float buyHoverDelaySeconds = 2f;
    [Header("Buy Panel Tween")]
    [SerializeField] private bool isBuyPanelShowing = true;
    [SerializeField] private float buyPanelTweenDuration = 0.25f;
    [SerializeField] private float buyPanelShowX = 50f;
    [SerializeField] private float buyPanelHideX = -500f;
    [Header("Mission Panel")]
    [SerializeField] private GameObject UIMissionItemPref;
    [SerializeField] private RectTransform missionPanelContent;
    [Header("Recipe Book")]
    [SerializeField] private GameObject recipePanel;
    [Header("Win Screen")]
    [SerializeField] private Image winScreen;
    [SerializeField] private TextMeshProUGUI winTxt;
    [SerializeField, Min(0.01f)] private float winFadeDuration = 0.25f;
    [SerializeField, Min(0f)] private float winTextDelay = 0.2f;
    [SerializeField, Min(0f)] private float winTextMoveDistance = 20f;
    [Header("End Screen")]
    [SerializeField] private Image endScreen;
    
    [SerializeField] private Image endText;
    [SerializeField] private TextMeshProUGUI endTxt;
    [Header("Object Panel")]
    [SerializeField] public Image fullPanel;
    [SerializeField] public Image buttonsPanel;
    [SerializeField] public Button rotateBtn;
    [SerializeField] public Button sellBtn;
    [SerializeField] public TextMeshProUGUI sellPriceTxt;
    [SerializeField] public Button upgradeBtn;
    [SerializeField] public TextMeshProUGUI upgradePriceTxt;
    [SerializeField] public Image setupPanel;
    [SerializeField] public RectTransform setupContainer;
    [SerializeField] public GameObject UISetupItemSelectionPref;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 fullPanelWorldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

    private readonly Dictionary<GameObject, Coroutine> fadeCoroutines = new Dictionary<GameObject, Coroutine>();
    private InteractableObject displayedInteractable;
    private RectTransform fullPanelRectTransform;
    private RectTransform rootCanvasRectTransform;
    private Canvas rootCanvas;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
    private Vector3 lastMousePosition;
    private float hoverStillTime;
    private UIBuyItemSelection currentHoverBuyItem;
    private bool isHoverDetailVisible;
    private RectTransform detailHoverRectTransform;
    private Sequence winSequence;

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
        InitBuyObjectPanel();
        InitMissionPanel();

        isBuyPanelShowing = true;
        if (buyPanel != null)
        {
            Vector2 anchored = buyPanel.anchoredPosition;
            anchored.x = buyPanelShowX;
            buyPanel.anchoredPosition = anchored;
            buyPanel.gameObject.SetActive(true);
        }

        lastMousePosition = Input.mousePosition;
        hoverStillTime = 0f;
        currentHoverBuyItem = null;
        detailHoverRectTransform = detailObjectBuyHoverPanel != null
            ? detailObjectBuyHoverPanel.transform as RectTransform
            : null;
        SetBuyHoverDetailVisible(false);
    }

    private void Start()
    {
        HideUIImmediate();
    }

    private void LateUpdate()
    {
        SyncSelectionFromPlayerInteraction();
        moneyTxt.text = GameManager.Instance.startMoney.ToString() + " $";
        SyncRunPauseButtons();
        UpdateBuyHoverDetail();
    }

    private void OnDisable()
    {
        if (winSequence != null)
        {
            winSequence.Kill(complete: false);
            winSequence = null;
        }

        foreach (Coroutine coroutine in fadeCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        fadeCoroutines.Clear();
        SetBuyHoverDetailVisible(false);
    }
    public void ShowWinUI(){
        if (winScreen == null || winTxt == null)
        {
            return;
        }

        if (winSequence != null)
        {
            winSequence.Kill(complete: false);
            winSequence = null;
        }

        winScreen.gameObject.SetActive(true);
        winTxt.gameObject.SetActive(true);

        Color screenColor = winScreen.color;
        screenColor.a = 0f;
        winScreen.color = screenColor;

        Color textColor = winTxt.color;
        textColor.a = 0f;
        winTxt.color = textColor;

        RectTransform textRect = winTxt.rectTransform;
        Vector2 endPos = textRect.anchoredPosition;
        textRect.anchoredPosition = endPos - new Vector2(0f, winTextMoveDistance);

        winSequence = DOTween.Sequence()
            .SetUpdate(isIndependentUpdate: true);

        winSequence.Append(winScreen.DOFade(1f, winFadeDuration).SetEase(Ease.Linear));
        winSequence.Insert(winTextDelay, winTxt.DOFade(1f, winFadeDuration).SetEase(Ease.Linear));
        winSequence.Insert(winTextDelay, textRect.DOAnchorPos(endPos, winFadeDuration).SetEase(Ease.OutQuad));
    }
    public void ShowEnd(){
        if (endScreen == null || endTxt == null)
        {
            return;
        }

        if (winSequence != null)
        {
            winSequence.Kill(complete: false);
            winSequence = null;
        }

        endScreen.gameObject.SetActive(true);
        endTxt.gameObject.SetActive(true);

        Color screenColor = endScreen.color;
        screenColor.a = 0f;
        endScreen.color = screenColor;

        Color textColor = endTxt.color;
        textColor.a = 0f;
        endTxt.color = textColor;

        RectTransform textRect = endTxt.rectTransform;
        Vector2 endPos = textRect.anchoredPosition;
        textRect.anchoredPosition = endPos - new Vector2(0f, winTextMoveDistance);

        winSequence = DOTween.Sequence()
            .SetUpdate(isIndependentUpdate: true);

        winSequence.Append(endScreen.DOFade(1f, winFadeDuration).SetEase(Ease.Linear));
        winSequence.Insert(winTextDelay, endTxt.DOFade(1f, winFadeDuration).SetEase(Ease.Linear));
        winSequence.Insert(winTextDelay, textRect.DOAnchorPos(endPos, winFadeDuration).SetEase(Ease.OutQuad));
    
    }
    private void InitMissionPanel()
    {
        foreach(Item item in GameManager.Instance.winItemList){
            UIMissionItem button = Instantiate(UIMissionItemPref,missionPanelContent).GetComponent<UIMissionItem>();
            button.item = item;
            button.gameObject.GetComponent<Image>().sprite = item.itemIcon;
            button.itemNameTxt.text = item.itemName;
            button.UpdateMissionItem(false);
            button.gameObject.SetActive(true);
        }
    }
    public void UpdateMissionItem(Item item)
    {
        foreach(Transform child in missionPanelContent){
            if(child.GetComponent<UIMissionItem>() == null){
                continue;
            }
            if(child.GetComponent<UIMissionItem>().item == item){
                child.GetComponent<UIMissionItem>().UpdateMissionItem(true);
                continue;
            }
            child.GetComponent<UIMissionItem>().UpdateMissionItem(false);
        }
    }
    public void ShowRecipeBook(){
        recipePanel.GetComponent<RecipeBook>().Init();
        recipePanel.SetActive(true);
    }
    public void HideRecipeBook(){
        recipePanel.SetActive(false);
    }
    private void UpdateBuyHoverDetail()
    {
        if (detailObjectBuyHoverPanel == null || detailObjectBuyHoverTxt == null)
        {
            return;
        }

        if (Input.mousePosition != lastMousePosition)
        {
            lastMousePosition = Input.mousePosition;
            hoverStillTime = 0f;
            currentHoverBuyItem = null;
            SetBuyHoverDetailVisible(false);
            return;
        }

        UIBuyItemSelection hoveredBuyItem = TryGetHoveredBuyItemSelection();
        if (hoveredBuyItem == null)
        {
            hoverStillTime = 0f;
            currentHoverBuyItem = null;
            SetBuyHoverDetailVisible(false);
            return;
        }

        if (hoveredBuyItem != currentHoverBuyItem)
        {
            currentHoverBuyItem = hoveredBuyItem;
            hoverStillTime = 0f;
            SetBuyHoverDetailVisible(false);
            return;
        }

        hoverStillTime += Time.unscaledDeltaTime;

        if (hoverStillTime >= buyHoverDelaySeconds)
        {
            detailObjectBuyHoverTxt.text = currentHoverBuyItem.objectDescription;
            SetBuyHoverDetailVisible(true);
        }
    }

    private UIBuyItemSelection TryGetHoveredBuyItemSelection()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return null;
        }

        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        uiRaycastResults.Clear();
        eventSystem.RaycastAll(pointerEventData, uiRaycastResults);

        for (int i = 0; i < uiRaycastResults.Count; i++)
        {
            GameObject hit = uiRaycastResults[i].gameObject;
            if (hit == null)
            {
                continue;
            }

            UIBuyItemSelection selection = hit.GetComponentInParent<UIBuyItemSelection>();
            if (selection != null)
            {
                return selection;
            }
        }

        return null;
    }

    private void SetBuyHoverDetailVisible(bool visible)
    {
        if(GameManager.Instance.isTutorial && !GameManager.Instance.allowInteract){
            return;
        }
        if (detailObjectBuyHoverPanel == null)
        {
            return;
        }

        if (isHoverDetailVisible == visible)
        {
            return;
        }

        isHoverDetailVisible = visible;
        detailObjectBuyHoverPanel.SetActive(visible);

        if (visible)
        {
            PositionBuyHoverDetailAtMouse();
        }
    }

    private void PositionBuyHoverDetailAtMouse()
    {
        if (detailHoverRectTransform == null || rootCanvasRectTransform == null)
        {
            return;
        }

        Camera uiCamera = rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas != null ? rootCanvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvasRectTransform,
                Input.mousePosition,
                uiCamera,
                out Vector2 localPoint))
        {
            detailHoverRectTransform.anchoredPosition = localPoint;
        }
    }
    public void InitBuyObjectPanel(){
        foreach(GameObject objectBuy in GameManager.Instance.allowBuyObjectList){
            InteractableObject interactableObject = objectBuy.GetComponent<InteractableObject>();
            UIBuyItemSelection button = Instantiate(UIBuyItemSelectionPref,buyObjectPanelContent).GetComponent<UIBuyItemSelection>();
            button.objectIcon.sprite = interactableObject.objectIcon;
            button.objectName.text = interactableObject.objectName;
            button.objectDescription = interactableObject.objectDescription;
            button.objectPrice = interactableObject.sellPrice;
            button.objectPriceTxt.text = interactableObject.sellPrice.ToString() + " $";
            button.objectBuyPref = objectBuy;
        }
    }
    public void UIRun(){
        if (buyPanel != null)
        {
            buyPanel.gameObject.SetActive(false);
        }
        GameManager.Instance.Run();
        PlayerInteraction.Instance.ClearSelection();
        SyncRunPauseButtons(); 
    }   

    public void UIPause()
    {   
        if (buyPanel != null)
        {
            buyPanel.gameObject.SetActive(true);
        }
        GameManager.Instance.Pause();
        SyncRunPauseButtons();
    }

    public void HideOrShowBuyPanel()
    {
        isBuyPanelShowing = !isBuyPanelShowing;
        TweenBuyPanel(isBuyPanelShowing);
    }

    private void TweenBuyPanel(bool show)
    {
        if (buyPanel == null)
        {
            return;
        }

        buyPanel.gameObject.SetActive(true);
        buyPanel.DOKill();

        float targetX = show ? buyPanelShowX : buyPanelHideX;
        buyPanel.DOAnchorPosX(targetX, Mathf.Max(0.01f, buyPanelTweenDuration))
            .SetEase(Ease.OutQuad);
    }

    private void SyncRunPauseButtons()
    {
        if (runBtn == null || pauseBtn == null || GameManager.Instance == null)
        {
            return;
        }

        bool running = GameManager.Instance.isRunning;
        runBtn.gameObject.SetActive(!running);
        pauseBtn.gameObject.SetActive(running);
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
        sellPriceTxt.text = newSelectedInteractable.sellPrice.ToString() + " $";
        ShowOrHideWithFade(upgradeBtn.gameObject, newSelectedInteractable.canUpgrade);
        upgradePriceTxt.text = newSelectedInteractable.upgradePrice.ToString() + " $";
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
                sellBtn.onClick.AddListener(this.HideUI);
                sellBtn.onClick.AddListener(playerInteraction.ClearSelection);
            }
        }

        if (upgradeBtn != null)
        {
            upgradeBtn.onClick.RemoveAllListeners();
            if (interactableObject != null)
            {
                upgradeBtn.onClick.AddListener(interactableObject.LevelUp);
                upgradeBtn.onClick.AddListener(this.HideUI);
                upgradeBtn.onClick.AddListener(playerInteraction.ClearSelection);
            }
        }
    }

    private void ShowOrHideWithFade(GameObject target, bool shouldShow)
    {
        if (shouldShow)
        {
            if(target == setupPanel.gameObject){
                foreach(Transform transform in setupContainer){
                    Destroy(transform.gameObject);
                }
                if(displayedInteractable.GetComponent<Grabber>()){
                    Grabber grabber = displayedInteractable.GetComponent<Grabber>();
                    if(grabber.isSmartGrabber){
                        foreach(Item item in GameManager.Instance.allowSetUpSmartGrabItemList){
                            UISetupItemSelection button = Instantiate(UISetupItemSelectionPref,setupContainer).GetComponent<UISetupItemSelection>();
                            button.itemIconSprite = item.itemIcon;
                            button.gameObject.SetActive(true);
                            button.item = item;
                            button.selectOwner = displayedInteractable;
                            button.itemNameTxt.text = item.itemName;
                            button.RefreshSelectionFromOwner();
                        }
                    }
                }
                if(displayedInteractable.GetComponent<Combiner>()){
                    foreach(Item item in GameManager.Instance.allowSetUpCombineItemList){
                        UISetupItemSelection button = Instantiate(UISetupItemSelectionPref,setupContainer).GetComponent<UISetupItemSelection>();
                        button.itemIconSprite = item.itemIcon;
                        button.gameObject.SetActive(true);
                        button.item = item;
                        button.selectOwner = displayedInteractable;
                        button.itemNameTxt.text = item.itemName;
                        button.RefreshSelectionFromOwner();
                    }
                }
            }
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
