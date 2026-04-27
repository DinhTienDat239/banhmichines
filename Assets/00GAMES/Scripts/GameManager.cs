using UnityEngine;
using DAT.Core.DesignPatterns;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class GameManager : Singleton<GameManager>
{
    private const float GridCellSpacing = 2f;
    private const float GridY = 0f;
    private static readonly Vector3 GridCameraEuler = new Vector3(45f, -15f, 0f);

    [Header("Level Settings")]
    [SerializeField] int levelNum;
    [SerializeField] public int startMoney = 1000;
    [SerializeField] public Item[] allowSetUpSmartGrabItemList;
    [SerializeField] public Item[] allowSetUpCombineItemList;
    [SerializeField] public GameObject[] allowBuyObjectList;
    [SerializeField] public Item[] winItemList;
    [SerializeField, Min(0.001f)] private float positionTolerance = 0.05f;
    private Coroutine winCoroutine;

    #region Tutorial

    [Header("Tutorial")]
    [SerializeField]
    List<string> tutorialTextList;
    [SerializeField]
    TextMeshProUGUI tutorialTextTxt;
    [SerializeField]
    GameObject clickToContinue;
    [SerializeField]
    GameObject tutorialPanel;
    [SerializeField]
    private Transform buyObjectPanelContent;
    private GameObject tutoStage6BuyButtonClone;
    public GameObject oven;
    public bool isTutorial=false;
    public bool isTalking = false;
    public bool allowInteract = false;
    public int tutoStage = 0;
    public bool isTyping;
    [SerializeField]
    GameObject recipeIconTuto;
    [SerializeField]
    GameObject tip;

    public void TypeText(string text){
        clickToContinue.SetActive(false);
        isTyping = true;
        tutorialTextTxt.text = "";
        StartCoroutine(TypeTextCoroutine(text));
    }
    private IEnumerator TypeTextCoroutine(string text){
        for(int i = 0; i < text.Length; i++){
            tutorialTextTxt.text += text[i];
            yield return new WaitForSeconds(0.02f);
        }
        isTyping = false;
        
        clickToContinue.SetActive(true);
    }
    public void MoveToNextStage(){
        
       
        if(tutoStage == 8){
            Destroy(tutoStage6BuyButtonClone);
            tutorialPanel.SetActive(false);
            allowInteract = true;
            tutoStage++;
            return;
        }
        

        if(tutoStage == 11){
            tutorialPanel.SetActive(false);
            allowInteract = true;
            tutoStage++;
            return;
        }


        tutorialPanel.SetActive(true);
        allowInteract = false;
         tutoStage++;
         if(tutoStage == 17){
            isTutorial = false;
            tutorialPanel.SetActive(false);
        }
        TypeText(tutorialTextList[tutoStage]);
        if(tutoStage == 16){
            recipeIconTuto.SetActive(true);
            tip.SetActive(true);
        }
        if(tutoStage == 13){
            InteractableObject interactableObject = oven.GetComponent<InteractableObject>();
            UIBuyItemSelection button = Instantiate(ObjectUIManager.Instance.UIBuyItemSelectionPref,buyObjectPanelContent).GetComponent<UIBuyItemSelection>();
            button.objectIcon.sprite = interactableObject.objectIcon;
            button.objectName.text = interactableObject.objectName;
            button.objectDescription = interactableObject.objectDescription;
            button.objectPrice = interactableObject.sellPrice;
            button.objectPriceTxt.text = interactableObject.sellPrice.ToString() + " $";
            button.objectBuyPref = oven;
        }
        if(tutoStage == 6){
            if (buyObjectPanelContent == null || tutorialPanel == null)
            {
                return;
            }

            if (buyObjectPanelContent.childCount <= 0)
            {
                return;
            }

            if (tutoStage6BuyButtonClone != null)
            {
                Destroy(tutoStage6BuyButtonClone);
                tutoStage6BuyButtonClone = null;
            }

            Transform first = buyObjectPanelContent.GetChild(0);
            RectTransform firstRect = first as RectTransform;
            tutoStage6BuyButtonClone = Instantiate(first.gameObject);
            RectTransform cloneRect = tutoStage6BuyButtonClone.transform as RectTransform;

            if (firstRect != null && cloneRect != null)
            {
                cloneRect.position = firstRect.position;
                cloneRect.rotation = firstRect.rotation;
            }
            
            tutoStage6BuyButtonClone.transform.SetParent(tutorialPanel.transform, true);
            tutoStage6BuyButtonClone.transform.SetAsLastSibling();
            cloneRect.localScale = Vector3.one;
            Button btn = tutoStage6BuyButtonClone.GetComponent<Button>();
            if (btn != null)
            {
                btn.enabled = false;
                btn.interactable = false;
            }
        }

    }
   void Awake(){
        base.Awake();
        if(isTutorial){
            tutorialPanel.SetActive(true);
            TypeText(tutorialTextList[tutoStage]);
        }
    }
    void Update(){
        
    }
    #endregion


    [Header("Time of Game Settings")]
    public float ingreBoxCoolDown = 1f;
    [Header("Runtime boolean")]
    [SerializeField] public bool isRunning = false;
    [SerializeField] public InteractableObject[] interactableObjects;
    [SerializeField] private Vector3[] gridPositions;
    [Header("Tool")]
    [SerializeField] private Transform gridParent;
    [SerializeField]
    Vector2 mapSize = new Vector2(10, 10);
    [SerializeField]
    public GameObject floorPref;
    [SerializeField] private Vector2 cameraOffsetXZ = new Vector2(0.5f, -0.75f);
    [SerializeField, Min(0.01f)] private float cameraFixedY = 7.5f;
    [SerializeField, Min(0.01f)] private float cameraMinOrthoSize = 6.5f;
    [SerializeField, Min(0f)] private float cameraOrthoPadding = 0.25f;

    public void Run(){
        isRunning = true;
    }
    public void Pause()
    {
        isRunning = false;

        if (interactableObjects == null)
        {
            return;
        }

        for (int i = 0; i < interactableObjects.Length; i++)
        {
            InteractableObject interactable = interactableObjects[i];
            if (interactable == null)
            {
                continue;
            }
            foreach(GameObject obj in interactable.disableWhenRunObjects){
                if(!obj.activeSelf)
                    obj.SetActive(true);
            }
            if (interactable.itemHolding != null)
            {
                Destroy(interactable.itemHolding.gameObject);
                interactable.itemHolding = null;
                interactable.itemRestingAtSlot = false;
            }

            if (interactable is Combiner combiner)
            {
                combiner.ResetCombinerOnPause();
            }
            if(interactable is IngreBox ingreBox){
                
            }
            if (interactable is Chopper chopper)
            {
                chopper.ForceStopAndClearFx();
            }

            if (interactable is Oven oven)
            {
                oven.ForceStopAndClearFx();
            }

            if (interactable is Hob hob)
            {
                hob.ForceStopAndClearFx();
            }

            if (interactable is RoboMixer roboMixer)
            {
                roboMixer.ForceStopAndClearFx();
            }

            if (interactable is XaxiuPot xaxiuPot)
            {
                xaxiuPot.ForceStopAndClearFx();
            }
        }
    }
    public void RestartLevel(){
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
    public void CheckWin(){
        Dictionary<string, bool> winItemKeys = new Dictionary<string, bool>();
        foreach(Item item in winItemList){
                    winItemKeys[item.itemName] = false;
                }
        foreach(InteractableObject interactable in interactableObjects){
            if(interactable is Dish dish){
                if(dish.itemHolding == null){
                    continue;
                }
                foreach(Item item in winItemList){
                    
                    if(dish.itemHolding.itemName == item.itemName){
                        winItemKeys[item.itemName] = true;
                        ObjectUIManager.Instance.UpdateMissionItem(item);
                    }
                }
            }
        }
        foreach(KeyValuePair<string, bool> winItemKey in winItemKeys){
            if(!winItemKey.Value){
                return;
            }
        }
        Win();
    }
    public void Win(){
        ObjectUIManager.Instance.ShowWinUI();
        if (winCoroutine == null)
        {
            winCoroutine = StartCoroutine(WinIE());
        }
    }
    IEnumerator WinIE(){
        yield return new WaitForSecondsRealtime(1.5f);

        if (levelNum == 8)
        {
            ObjectUIManager.Instance.ShowEnd();
            winCoroutine = null;
            yield break;
        }
        string nextSceneName = "Level" + (levelNum + 1);

        SceneLoader.Instance.LoadScene(nextSceneName);
    }
    [ContextMenu("Spawn Grid Floors")]
    public void SpawnGridFloors()
    {
        if (gridParent == null || floorPref == null)
        {
            return;
        }

        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            Transform child = gridParent.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        int width = Mathf.Max(1, Mathf.RoundToInt(mapSize.x));
        int height = Mathf.Max(1, Mathf.RoundToInt(mapSize.y));

        float startX = gridParent.position.x;
        float startZ = gridParent.position.z;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 spawnPos = new Vector3(
                    startX + x * GridCellSpacing,
                    GridY,
                    startZ + z * GridCellSpacing);

                Quaternion rotation = floorPref.transform.rotation;
                GameObject floorObj = Instantiate(floorPref, spawnPos, rotation, gridParent);
                floorObj.transform.position = spawnPos;
            }
        }

        LoadGridPositions();
        AlignCameraToGrid(width, height, startX, startZ);
    }

    public void LoadGridPositions()
    {
        if (gridParent == null)
        {
            gridPositions = new Vector3[0];
            return;
        }

        List<Vector3> positions = new List<Vector3>(gridParent.childCount);

        for (int i = 0; i < gridParent.childCount; i++)
        {
            Transform child = gridParent.GetChild(i);
            positions.Add(child.position);
        }

        gridPositions = positions.ToArray();
    }

    private void AlignCameraToGrid(int width, int height, float startX, float startZ)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        float gridWidth = (width - 1) * GridCellSpacing;
        float gridHeight = (height - 1) * GridCellSpacing;
        Vector3 center = new Vector3(
            startX + gridWidth * 0.5f + cameraOffsetXZ.x,
            cameraFixedY,
            startZ + gridHeight * 0.5f + cameraOffsetXZ.y);

        cam.transform.SetPositionAndRotation(center, Quaternion.Euler(GridCameraEuler));

        if (!cam.orthographic)
        {
            return;
        }

        float halfCell = GridCellSpacing * 0.5f;
        float minX = startX - halfCell;
        float maxX = startX + gridWidth + halfCell;
        float minZ = startZ - halfCell;
        float maxZ = startZ + gridHeight + halfCell;

        Vector3[] corners = new Vector3[]
        {
            new Vector3(minX, GridY, minZ),
            new Vector3(maxX, GridY, minZ),
            new Vector3(minX, GridY, maxZ),
            new Vector3(maxX, GridY, maxZ)
        };

        float maxLocalX = 0f;
        float maxLocalY = 0f;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 local = cam.transform.InverseTransformPoint(corners[i]);
            maxLocalX = Mathf.Max(maxLocalX, Mathf.Abs(local.x));
            maxLocalY = Mathf.Max(maxLocalY, Mathf.Abs(local.y));
        }

        float sizeFromHeight = maxLocalY;
        float sizeFromWidth = maxLocalX / Mathf.Max(0.01f, cam.aspect);
        cam.orthographicSize = Mathf.Max(cameraMinOrthoSize, Mathf.Max(sizeFromHeight, sizeFromWidth) + cameraOrthoPadding);
    }

    public bool TryGetNearestValidGridPosition(Vector3 worldPosition, InteractableObject movingObject, out Vector3 validPosition)
    {
        validPosition = worldPosition;

        if (gridPositions == null || gridPositions.Length == 0)
        {
            return false;
        }

        float closestDistance = float.MaxValue;
        bool found = false;

        for (int i = 0; i < gridPositions.Length; i++)
        {
            Vector3 candidate = gridPositions[i];

            if (!IsPositionAvailable(candidate, movingObject))
            {
                continue;
            }

            float distance = Vector2.Distance(
                new Vector2(worldPosition.x, worldPosition.z),
                new Vector2(candidate.x, candidate.z));

            if (distance < closestDistance)
            {
                closestDistance = distance;
                validPosition = candidate;
                found = true;
            }
        }

        return found;
    }

    public bool IsOnGridPosition(Vector3 worldPosition)
    {
        if (gridPositions == null)
        {
            return false;
        }

        for (int i = 0; i < gridPositions.Length; i++)
        {
            if (ApproximatelyXZ(worldPosition, gridPositions[i]))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPositionAvailable(Vector3 worldPosition, InteractableObject ignoreObject = null)
    {
        if (!IsOnGridPosition(worldPosition))
        {
            return false;
        }

        if (interactableObjects == null)
        {
            return true;
        }

        for (int i = 0; i < interactableObjects.Length; i++)
        {
            InteractableObject current = interactableObjects[i];

            if (current == null || current == ignoreObject)
            {
                continue;
            }

            if (ApproximatelyXZ(current.transform.position, worldPosition))
            {
                return false;
            }
        }

        return true;
    }

    private bool ApproximatelyXZ(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) <= positionTolerance && Mathf.Abs(a.z - b.z) <= positionTolerance;
    }

    public bool TryGetNearestGridWorldPosition(Vector3 worldPosition, out Vector3 snappedGridWorld)
    {
        snappedGridWorld = worldPosition;

        if (gridPositions == null || gridPositions.Length == 0)
        {
            return false;
        }

        float bestSqr = float.MaxValue;

        for (int i = 0; i < gridPositions.Length; i++)
        {
            Vector3 candidate = gridPositions[i];
            float sqr = Vector2.SqrMagnitude(
                new Vector2(worldPosition.x - candidate.x, worldPosition.z - candidate.z));

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                snappedGridWorld = candidate;
            }
        }

        return true;
    }

    public bool TryGetAdjacentGridWorldPosition(Vector3 fromWorld, Vector3 directionXZ, out Vector3 neighborGridWorld)
    {
        neighborGridWorld = fromWorld;

        if (gridPositions == null || gridPositions.Length == 0)
        {
            return false;
        }

        if (!TryGetNearestGridWorldPosition(fromWorld, out Vector3 originGrid))
        {
            return false;
        }

        directionXZ.y = 0f;

        if (directionXZ.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        directionXZ.Normalize();

        float bestDistance = float.MaxValue;
        bool found = false;

        for (int i = 0; i < gridPositions.Length; i++)
        {
            Vector3 candidate = gridPositions[i];

            if (ApproximatelyXZ(candidate, originGrid))
            {
                continue;
            }

            Vector3 delta = candidate - originGrid;
            delta.y = 0f;
            float magnitude = delta.magnitude;

            if (magnitude < 0.0001f)
            {
                continue;
            }

            if (Vector3.Dot(delta / magnitude, directionXZ) < 0.85f)
            {
                continue;
            }

            if (magnitude < bestDistance)
            {
                bestDistance = magnitude;
                neighborGridWorld = candidate;
                found = true;
            }
        }

        return found;
    }

    public InteractableObject GetInteractableAtWorldPosition(Vector3 worldPosition, InteractableObject ignore)
    {
        if (interactableObjects == null)
        {
            return null;
        }

        for (int i = 0; i < interactableObjects.Length; i++)
        {
            InteractableObject current = interactableObjects[i];

            if (current == null || current == ignore)
            {
                continue;
            }

            if (ApproximatelyXZ(current.transform.position, worldPosition))
            {
                return current;
            }
        }

        return null;
    }

    public bool TryGetFirstEmptyGridPosition(out Vector3 emptyGridPosition)
    {
        emptyGridPosition = Vector3.zero;

        if (gridPositions == null || gridPositions.Length == 0)
        {
            LoadGridPositions();
        }

        if (gridPositions == null || gridPositions.Length == 0)
        {
            return false;
        }

        InteractableObject[] currentInteractables = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);

        for (int i = 0; i < gridPositions.Length; i++)
        {
            Vector3 grid = gridPositions[i];
            bool occupied = false;

            for (int j = 0; j < currentInteractables.Length; j++)
            {
                InteractableObject interactable = currentInteractables[j];
                if (interactable == null)
                {
                    continue;
                }

                if (ApproximatelyXZ(interactable.transform.position, grid))
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                emptyGridPosition = grid;
                return true;
            }
        }

        return false;
    }

    public bool TryGetMiddlestEmptyGridPosition(out Vector3 emptyGridPosition)
    {
        emptyGridPosition = Vector3.zero;

        if (gridPositions == null || gridPositions.Length == 0)
        {
            LoadGridPositions();
        }

        if (gridPositions == null || gridPositions.Length == 0)
        {
            return false;
        }

        Vector3 center = Vector3.zero;
        for (int i = 0; i < gridPositions.Length; i++)
        {
            center += gridPositions[i];
        }
        center /= gridPositions.Length;
        center.y = 0f;

        InteractableObject[] currentInteractables = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);

        bool found = false;
        float bestSqrDistance = float.MaxValue;

        for (int i = 0; i < gridPositions.Length; i++)
        {
            Vector3 grid = gridPositions[i];
            bool occupied = false;

            for (int j = 0; j < currentInteractables.Length; j++)
            {
                InteractableObject interactable = currentInteractables[j];
                if (interactable == null)
                {
                    continue;
                }

                if (ApproximatelyXZ(interactable.transform.position, grid))
                {
                    occupied = true;
                    break;
                }
            }

            if (occupied)
            {
                continue;
            }

            Vector3 gridXZ = new Vector3(grid.x, 0f, grid.z);
            float sqrDistance = (gridXZ - center).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                emptyGridPosition = grid;
                found = true;
            }
        }

        return found;
    }

    public void RegisterInteractableObject(InteractableObject interactable)
    {
        if (interactable == null)
        {
            return;
        }

        if (interactableObjects == null)
        {
            interactableObjects = new InteractableObject[] { interactable };
            return;
        }

        for (int i = 0; i < interactableObjects.Length; i++)
        {
            if (interactableObjects[i] == interactable)
            {
                return;
            }
        }

        InteractableObject[] newArray = new InteractableObject[interactableObjects.Length + 1];
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            newArray[i] = interactableObjects[i];
        }

        newArray[newArray.Length - 1] = interactable;
        interactableObjects = newArray;
    }

    public void RemoveInteractableObject(InteractableObject interactable)
    {
        if (interactable == null || interactableObjects == null || interactableObjects.Length == 0)
        {
            return;
        }

        int removeIndex = -1;
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            if (interactableObjects[i] == interactable)
            {
                removeIndex = i;
                break;
            }
        }

        if (removeIndex < 0)
        {
            return;
        }

        if (interactableObjects.Length == 1)
        {
            interactableObjects = new InteractableObject[0];
            return;
        }

        InteractableObject[] newArray = new InteractableObject[interactableObjects.Length - 1];
        int newIndex = 0;
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            if (i == removeIndex)
            {
                continue;
            }

            newArray[newIndex] = interactableObjects[i];
            newIndex++;
        }

        interactableObjects = newArray;
    }

}
