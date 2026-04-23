using UnityEngine;
using UnityEngine.UI;

public class UISetupItemSelection : MonoBehaviour
{
    public Sprite itemIconSprite;
    public string itemName;
    public InteractableObject selectOwner;
    public Image imageDisplay;
    void Start(){
        imageDisplay = GetComponent<Image>();
        imageDisplay.sprite = itemIconSprite;
    }
    
    public void Select(){

        if(selectOwner.GetComponent<Grabber>()){
            Grabber grabber = selectOwner.GetComponent<Grabber>();
            if(grabber.isSmartGrabber){
                grabber.allowToGrabObject = itemName;
            }
        }
    }
}
