using UnityEngine;
using DAT.Core.DesignPatterns;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private InteractableObject[] interactableObjects;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Vector3[] gridPositions;
    [SerializeField, Min(0.001f)] private float positionTolerance = 0.05f;
    [SerializeField] public bool isRunning = false;
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

}
