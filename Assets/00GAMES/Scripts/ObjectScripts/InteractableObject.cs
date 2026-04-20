using DG.Tweening;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    [Header("Interactable Object Settings")]
    [SerializeField]
    public bool canRotate = true;
    [SerializeField]
    public bool canSell = true;
    [SerializeField]
    public bool canUpgrade = true;
    [SerializeField]
    public bool canSetup = false;
    [SerializeField]
    public bool pushable = false;
    [SerializeField]
    public bool grabable = false;
    [Header("Interactable Object values")]
    [SerializeField]
    public int level = 1;
    [SerializeField]
    public int sellPrice = 100;
    [SerializeField]
    public int upgradePrice = 100;
    [Header("Direction Settings")]
    [SerializeField] private Direction defaultDirection = Direction.Up;
    [SerializeField] private float directionGizmoLength = 1.2f;
    [SerializeField] private Color directionGizmoColor = Color.cyan;
    [Header("Runtime (Read Only)")]
    [SerializeField] public Item itemHolding = null;
    [SerializeField] private Direction currentDirection;

    private bool isRotating;

    public Direction CurrentDirection => currentDirection;
    public Direction DefaultDirection => defaultDirection;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ApplyDefaultDirection();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Rotate()
    {
        if (isRotating || !canRotate)
        {
            return;
        }

        Vector3 currentEuler = transform.eulerAngles;
        float targetY = Mathf.Round((currentEuler.y + 90f) / 90f) * 90f;
        Direction targetDirection = GetNextDirection(currentDirection);

        isRotating = true;
        transform.DORotate(new Vector3(currentEuler.x, targetY, currentEuler.z), 0.25f, RotateMode.Fast)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                currentDirection = targetDirection;
                isRotating = false;
            });
    }
    public void Sell()
    {
        Destroy(gameObject);
    }
    public void Upgrade()
    {
        level++;
    }

    private void ApplyDefaultDirection()
    {
        currentDirection = defaultDirection;
    }

    private Direction GetDirectionFromYaw(float yRotation)
    {
        int step = Mathf.RoundToInt(yRotation / 90f);
        step = ((step % 4) + 4) % 4;
        return (Direction)step;
    }

    private Direction GetNextDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Right,
            Direction.Right => Direction.Down,
            Direction.Down => Direction.Left,
            Direction.Left => Direction.Up,
            _ => Direction.Up
        };
    }

    private Vector3 GetDirectionVector(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector3.forward,
            Direction.Right => Vector3.right,
            Direction.Down => Vector3.back,
            Direction.Left => Vector3.left,
            _ => Vector3.forward
        };
    }

    private void OnValidate()
    {
        if (directionGizmoLength < 0.1f)
        {
            directionGizmoLength = 0.1f;
        }

        if (!Application.isPlaying)
        {
            currentDirection = defaultDirection;
        }
    }

    [ContextMenu("Sync Default Direction From Current Rotation")]
    private void SyncDefaultDirectionFromCurrentRotation()
    {
        defaultDirection = GetDirectionFromYaw(transform.eulerAngles.y);
        currentDirection = defaultDirection;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = directionGizmoColor;

        Vector3 from = transform.position + Vector3.up * 0.1f;
        Vector3 direction = GetDirectionVector(currentDirection);
        Vector3 to = from + direction * directionGizmoLength;

        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(to, 0.08f);
    }
}
