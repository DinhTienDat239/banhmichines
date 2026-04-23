using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField]
    public string itemName;
    [SerializeField]
    public Sprite itemIcon;
    [SerializeField]
    public bool canChop;
    [SerializeField]
    public Item afterChopItem;
    [SerializeField]
    public bool canHob;
    [SerializeField]
    Item afterHobItem;
    [SerializeField]
    public bool canHandMachine;
    [SerializeField]
    Item afterHandMachineItem;
    [SerializeField]
    public bool canOven;
    [SerializeField]
    Item afterOvenItem;
    [SerializeField]
    public bool canCombine;

    public Item AfterChopItem => afterChopItem;
    public Item AfterOvenItem => afterOvenItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
