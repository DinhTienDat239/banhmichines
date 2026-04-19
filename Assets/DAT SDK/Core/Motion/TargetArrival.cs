
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class TargetArrival : MonoBehaviour
{
    public List<Transform> targetObjects = new List<Transform>();
    [Header("Events")]
    public UnityEvent OnArrivals;
    public List<UnityEvent> OnArrivalBySingleTarget;
    public void ArrivalAtIndex(int index)
    {
        if (index < 0 || index >= targetObjects.Count || targetObjects[index] == null)
            return;

        OnArrivals.Invoke();

        if (OnArrivalBySingleTarget[index] != null)
        {
            OnArrivalBySingleTarget[index].Invoke();
        }
      
    }

}
