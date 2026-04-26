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
    public bool canMix;
    [SerializeField]
    Item afterMix;
    [SerializeField]
    public bool canOven;
    [SerializeField]
    Item afterOvenItem;
    [SerializeField]
    public bool canXaxiu;
    [SerializeField]
    Item afterXaxiuItem;
    [SerializeField]
    public bool canCombine;
    [SerializeField]
    public Item[] combinedFromList;

    public Item AfterChopItem => afterChopItem;
    public Item AfterHobItem => afterHobItem;
    public Item AfterMixItem => afterMix;
    public Item AfterOvenItem => afterOvenItem;
    public Item AfterXaxiuItem => afterXaxiuItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
