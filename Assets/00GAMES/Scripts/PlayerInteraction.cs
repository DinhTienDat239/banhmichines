using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask interactableLayerMask = ~0;
    [SerializeField] private float rayDistance = 300f;

    [Header("Drag")]
    [SerializeField] private float dragPlaneHeight = 0f;
    [SerializeField, Min(0.001f)] private float gridSnapSize = 1f;
    [SerializeField] private bool keepObjectOriginalHeight = true;
    [SerializeField, Min(0f)] private float dragStartThresholdPixels = 8f;

    [Header("Runtime (Read Only)")]
    [SerializeField] private InteractableObject selectedInteractable;
    [SerializeField] private InteractableObject draggingInteractable;

    private Behaviour selectedOutline;
    private Plane dragPlane;
    private Vector3 dragStartPosition;
    private float selectedObjectHeight;
    private bool isPointerDownOnInteractable;
    private bool dragStarted;
    private Vector3 pointerDownScreenPosition;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    public InteractableObject SelectedInteractable => selectedInteractable;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        dragPlane = new Plane(Vector3.up, new Vector3(0f, dragPlaneHeight, 0f));
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandlePointerDown();
        }

        if (Input.GetMouseButton(0))
        {
            HandlePointerHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandlePointerUp();
        }
    }

    private void HandlePointerDown()
    {
        if (IsPointerOverUI())
        {
            isPointerDownOnInteractable = false;
            dragStarted = false;
            draggingInteractable = null;
            return;
        }

        if (!TryGetInteractableFromMouse(out InteractableObject interactable))
        {
            ClearSelection();
            return;
        }

        SelectInteractable(interactable);

        isPointerDownOnInteractable = true;
        dragStarted = false;
        pointerDownScreenPosition = Input.mousePosition;
        draggingInteractable = interactable;
    }

    private bool IsPointerOverUI()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return false;
        }

        if (eventSystem.IsPointerOverGameObject())
        {
            return true;
        }

        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        uiRaycastResults.Clear();
        eventSystem.RaycastAll(pointerEventData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }

    private void SelectInteractable(InteractableObject interactable)
    {
        if (selectedInteractable == interactable)
        {
            return;
        }

        SetOutline(selectedOutline, false);

        selectedInteractable = interactable;
        selectedOutline = GetQuickOutline(interactable);
        SetOutline(selectedOutline, true);
    }

    public void ClearSelection()
    {
        SetOutline(selectedOutline, false);
        selectedOutline = null;
        selectedInteractable = null;
        draggingInteractable = null;
        isPointerDownOnInteractable = false;
        dragStarted = false;
    }

    private void StartDrag(InteractableObject interactable)
    {
        draggingInteractable = interactable;
        dragStartPosition = interactable.transform.position;
        selectedObjectHeight = interactable.transform.position.y;
    }

    private void HandlePointerHold()
    {
        if (!isPointerDownOnInteractable || draggingInteractable == null)
        {
            return;
        }

        if (!dragStarted)
        {
            if (!HasExceededDragThreshold())
            {
                return;
            }

            StartDrag(draggingInteractable);
            dragStarted = true;
        }

        HandleDrag();
    }

    private void HandleDrag()
    {
        if (!TryGetMousePointOnDragPlane(out Vector3 mouseWorldPoint))
        {
            return;
        }

        Vector3 snapped = SnapPositionByOffset(mouseWorldPoint);

        if (keepObjectOriginalHeight)
        {
            snapped.y = selectedObjectHeight;
        }
        else
        {
            snapped.y = draggingInteractable.transform.position.y;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            draggingInteractable.transform.position = snapped;
            return;
        }

        if (gameManager.TryGetNearestValidGridPosition(snapped, draggingInteractable, out Vector3 validGridPosition))
        {
            draggingInteractable.transform.position = new Vector3(validGridPosition.x, snapped.y, validGridPosition.z);
        }
    }

    private void HandleDrop()
    {
        if (!dragStarted || draggingInteractable == null)
        {
            draggingInteractable = null;
            isPointerDownOnInteractable = false;
            dragStarted = false;
            return;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            draggingInteractable = null;
            isPointerDownOnInteractable = false;
            dragStarted = false;
            return;
        }

        Vector3 droppedPosition = draggingInteractable.transform.position;
        Vector3 validatePosition = new Vector3(droppedPosition.x, 0f, droppedPosition.z);

        if (!gameManager.IsPositionAvailable(validatePosition, draggingInteractable))
        {
            draggingInteractable.transform.position = dragStartPosition;
        }

        draggingInteractable = null;
        isPointerDownOnInteractable = false;
        dragStarted = false;
    }

    private void HandlePointerUp()
    {
        if (!isPointerDownOnInteractable)
        {
            return;
        }

        HandleDrop();
    }

    private bool TryGetInteractableFromMouse(out InteractableObject interactable)
    {
        interactable = null;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayerMask))
        {
            return false;
        }

        interactable = hit.collider.GetComponentInParent<InteractableObject>();
        return interactable != null;
    }

    private bool TryGetMousePointOnDragPlane(out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float enter))
        {
            return false;
        }

        worldPoint = ray.GetPoint(enter);
        return true;
    }

    private Vector3 SnapPositionByOffset(Vector3 worldPosition)
    {
        float step = gridSnapSize;
        step = Mathf.Max(0.001f, step);

        float x = Mathf.Round(worldPosition.x / step) * step;
        float z = Mathf.Round(worldPosition.z / step) * step;
        return new Vector3(x, worldPosition.y, z);
    }

    private bool HasExceededDragThreshold()
    {
        Vector3 delta = Input.mousePosition - pointerDownScreenPosition;
        return delta.sqrMagnitude >= dragStartThresholdPixels * dragStartThresholdPixels;
    }

    private Behaviour GetQuickOutline(InteractableObject interactable)
    {
        if (interactable == null)
        {
            return null;
        }

        return interactable.GetComponent("QuickOutline") as Behaviour;
    }

    private void SetOutline(Behaviour outlineComponent, bool isEnabled)
    {
        if (outlineComponent == null)
        {
            return;
        }

        outlineComponent.enabled = isEnabled;
    }
}
