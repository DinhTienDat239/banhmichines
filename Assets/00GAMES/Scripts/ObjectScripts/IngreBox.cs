using UnityEngine;

public class IngreBox : InteractableObject
{
    [SerializeField] public Item ownItem;

    public override void LevelUp()
    {
        level++;
    }
}
