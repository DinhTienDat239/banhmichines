using UnityEngine;
using DAT.Core.DesignPatterns;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private InteractableObject[] interactableObjects;
    [SerializeField] public Item[] allowSetUpSmartGrabItemList;
    [SerializeField] public Item[] allowSetUpCombineItemList;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Vector3[] gridPositions;
    [SerializeField, Min(0.001f)] private float positionTolerance = 0.05f;
    [SerializeField] public bool isRunning = false;

    [Header("Time of Game Settings")]
    public float ingreBoxCoolDown = 1f;
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

}
