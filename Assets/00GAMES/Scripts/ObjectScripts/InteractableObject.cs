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
    public bool allowPushIn = false;
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
    [Header("Item Settings")]
    [SerializeField]
    Transform itemPosition;
    [SerializeField, Min(0.01f)] private float grabItemMoveDuration = 0.35f;

    private bool isRotating;
    private bool itemRestingAtSlot;
    private float nextIngreBoxGrabTime;

    public Direction CurrentDirection => currentDirection;
    public Direction DefaultDirection => defaultDirection;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ApplyDefaultDirection();

        if (itemHolding != null)
        {
            SnapItemToSlot();
        }
    }

    void Update()
    {
        if (grabable)
        {
            Grab();
        }

        if (pushable)
        {
            Push();
        }
    }
    public void Grab()
    {
        if (!grabable || itemHolding != null || itemPosition == null)
        {
            return;
        }

        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            return;
        }

        Direction oppositeDirection = GetOppositeDirection(currentDirection);
        Vector3 dir = GetDirectionVector(oppositeDirection);

        if (!gameManager.TryGetAdjacentGridWorldPosition(transform.position, dir, out Vector3 oppositeGridWorld))
        {
            return;
        }

        InteractableObject opposite = gameManager.GetInteractableAtWorldPosition(oppositeGridWorld, this);

        if (opposite == null)
        {
            return;
        }

        if (opposite is IngreBox box && box.ownItem != null)
        {
            if (Time.time < nextIngreBoxGrabTime)
            {
                return;
            }

            Item spawned = Instantiate(box.ownItem, itemPosition.position, itemPosition.rotation, itemPosition);
            spawned.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            itemHolding = spawned;
            itemRestingAtSlot = true;
            nextIngreBoxGrabTime = Time.time + Mathf.Max(0f, gameManager.ingreBoxCoolDown);
            return;
        }

        if (opposite.itemHolding == null || !opposite.isItemInPosition())
        {
            return;
        }

        Item item = opposite.itemHolding;
        opposite.itemHolding = null;
        opposite.itemRestingAtSlot = false;

        itemHolding = item;
        itemRestingAtSlot = false;

        item.transform.DOKill();
        item.transform.SetParent(itemPosition, true);
        item.transform.DOLocalMove(Vector3.zero, grabItemMoveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (item == null || itemHolding != item || itemPosition == null)
                {
                    return;
                }

                item.transform.SetParent(itemPosition, false);
                item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                itemRestingAtSlot = true;
            });
    }

    public void Push()
    {
        if (!pushable || itemHolding == null || itemPosition == null || !isItemInPosition())
        {
            return;
        }

        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            return;
        }

        Vector3 dir = GetDirectionVector(currentDirection);

        if (!gameManager.TryGetAdjacentGridWorldPosition(transform.position, dir, out Vector3 targetGridWorld))
        {
            return;
        }

        InteractableObject target = gameManager.GetInteractableAtWorldPosition(targetGridWorld, this);

        if (target == null || !target.allowPushIn || target.itemHolding != null || target.itemPosition == null)
        {
            return;
        }

        Item item = itemHolding;
        itemHolding = null;
        itemRestingAtSlot = false;

        target.itemHolding = item;
        target.itemRestingAtSlot = false;

        item.transform.DOKill();
        item.transform.SetParent(target.itemPosition, true);
        item.transform.DOLocalMove(Vector3.zero, grabItemMoveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (item == null || target == null || target.itemHolding != item || target.itemPosition == null)
                {
                    return;
                }

                item.transform.SetParent(target.itemPosition, false);
                item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                target.itemRestingAtSlot = true;
            });
    }

    public bool isItemInPosition()
    {
        return itemHolding != null
            && itemPosition != null
            && itemRestingAtSlot;
    }

    private void SnapItemToSlot()
    {
        if (itemHolding == null || itemPosition == null)
        {
            itemRestingAtSlot = false;
            return;
        }

        itemHolding.transform.DOKill();
        itemHolding.transform.SetParent(itemPosition, false);
        itemHolding.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        itemRestingAtSlot = true;
    }

    private static Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Right => Direction.Left,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            _ => Direction.Down
        };
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
    public virtual void LevelUp()
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

    public void RotateDesignLevel90()
    {
        Direction nextDirection = GetNextDirection(defaultDirection);
        defaultDirection = nextDirection;
        currentDirection = nextDirection;

        transform.Rotate(0f, 90f, 0f, Space.Self);
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
