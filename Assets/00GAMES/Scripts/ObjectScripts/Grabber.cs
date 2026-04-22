using UnityEngine;

public class Grabber : InteractableObject
{
    public override void LevelUp()
    {
        level++;
        canSetup = true;
        canUpgrade = false;
        sellPrice = sellPrice+100;
        
    }
}
