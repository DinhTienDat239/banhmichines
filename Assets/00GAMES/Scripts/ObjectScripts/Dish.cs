using UnityEngine;

public class Dish : InteractableObject
{
    public override void LevelUp()
    {
        
    }
    void Awake(){
        GameManager.Instance.RegisterInteractableObject(this);
    }
}
