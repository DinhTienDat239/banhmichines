using System;
using System.Collections;
using System.Collections.Generic;
using DAT.Core.DesignPatterns;
using UnityEngine;
using UnityEngine.Events;

public class SEPManager : Singleton<SEPManager>
{
    [SerializeField]
    UnityEvent awakeEvents;
    [SerializeField]
    UnityEvent startEvents;

    protected override void Awake()
    {
        base.Awake();
        awakeEvents?.Invoke();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        startEvents?.Invoke();
    }
}
