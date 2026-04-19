using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float minZoomSize = 3f;
    [SerializeField] private float zoomStep = 2f;
    [SerializeField] private float zoomSmoothTime = 0.12f;
    [SerializeField] private float moveSmoothTime = 0.12f;
    [SerializeField] private float groundPlaneY = 0f;

    [Header("Init / Max (Readonly At Runtime)")]
    [SerializeField] private Vector3 initialPosition;
    [SerializeField] private float initialOrthographicSize;
    [SerializeField] private float fixedCameraY;

    [Header("Runtime Info")]
    [SerializeField] private float currentOrthographicSize;
    [SerializeField] private float targetOrthographicSize;
    [SerializeField] private Vector3 currentPosition;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float scrollInput;
    [SerializeField] private Vector3 mouseWorldOnGround;

    private Camera _cam;
    private float _zoomVelocity;
    private Vector3 _moveVelocity;

    private void Start()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null)
        {
            Debug.LogError("CameraController requires a Camera component on the same GameObject.");
            enabled = false;
            return;
        }

        if (!_cam.orthographic)
        {
            Debug.LogWarning("CameraController is designed for Orthographic camera zoom.");
        }

        initialPosition = transform.position;
        fixedCameraY = initialPosition.y;
        initialOrthographicSize = _cam.orthographicSize;

        targetPosition = initialPosition;
        targetOrthographicSize = initialOrthographicSize;

        UpdateInspectorRuntimeInfo();
    }

    private void Update()
    {
        HandleZoomInput();
        ApplySmoothMovement();
        UpdateInspectorRuntimeInfo();
    }

    private void HandleZoomInput()
    {
        scrollInput = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollInput) < Mathf.Epsilon)
        {
            return;
        }

        float clampedMinZoom = Mathf.Clamp(minZoomSize, 0.01f, initialOrthographicSize);
        float proposedSize = targetOrthographicSize - (scrollInput * zoomStep);
        float newTargetSize = Mathf.Clamp(proposedSize, clampedMinZoom, initialOrthographicSize);

        // Zoom in: keep mouse world point stable by shifting camera X/Z only.
        if (scrollInput > 0f)
        {
            Vector3 beforeZoomMouseWorld = GetMouseWorldPointOnGround(targetOrthographicSize, targetPosition);
            Vector3 afterZoomMouseWorld = GetMouseWorldPointOnGround(newTargetSize, targetPosition);

            Vector3 delta = beforeZoomMouseWorld - afterZoomMouseWorld;
            delta.y = 0f;

            targetPosition += delta;
            targetPosition.y = fixedCameraY;
        }
        else
        {
            // Zoom out: move back to initial max position while size returns to max.
            targetPosition = new Vector3(initialPosition.x, fixedCameraY, initialPosition.z);
        }

        targetOrthographicSize = newTargetSize;
    }

    private void ApplySmoothMovement()
    {
        float smoothSize = Mathf.SmoothDamp(
            _cam.orthographicSize,
            targetOrthographicSize,
            ref _zoomVelocity,
            Mathf.Max(0.001f, zoomSmoothTime)
        );
        _cam.orthographicSize = smoothSize;

        Vector3 desiredPos = new Vector3(targetPosition.x, fixedCameraY, targetPosition.z);
        Vector3 smoothPos = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _moveVelocity,
            Mathf.Max(0.001f, moveSmoothTime)
        );
        smoothPos.y = fixedCameraY;
        transform.position = smoothPos;
    }

    private Vector3 GetMouseWorldPointOnGround(float cameraSize, Vector3 cameraPosition)
    {
        Vector3 backupPosition = transform.position;
        float backupSize = _cam.orthographicSize;

        transform.position = new Vector3(cameraPosition.x, fixedCameraY, cameraPosition.z);
        _cam.orthographicSize = cameraSize;

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundPlaneY, 0f));
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        Vector3 point = transform.position;

        if (groundPlane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
        }

        transform.position = backupPosition;
        _cam.orthographicSize = backupSize;
        return point;
    }

    private void UpdateInspectorRuntimeInfo()
    {
        currentOrthographicSize = _cam != null ? _cam.orthographicSize : 0f;
        currentPosition = transform.position;
        mouseWorldOnGround = _cam != null
            ? GetMouseWorldPointOnGround(currentOrthographicSize, currentPosition)
            : Vector3.zero;
    }
}
