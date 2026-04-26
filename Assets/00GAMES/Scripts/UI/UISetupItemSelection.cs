using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISetupItemSelection : MonoBehaviour
{
    Button thisBtn;
    public Sprite itemIconSprite;
    public Item item;
    public InteractableObject selectOwner;
    public Image imageDisplay;
    public Image chosenImage;
    [SerializeField]
    public TextMeshProUGUI itemNameTxt;
    public bool isChosen = false;
    void Awake(){
        thisBtn = GetComponent<Button>();
    }
    void Start(){
        imageDisplay = GetComponent<Image>();
        imageDisplay.sprite = itemIconSprite;
        RefreshSelectionFromOwner();
    }
    
    public void Select(){
        ResetAllSelectionInContainer();
        SetSelectionState(true);

        if(selectOwner.GetComponent<Grabber>()){
            Grabber grabber = selectOwner.GetComponent<Grabber>();
            if(grabber.isSmartGrabber){
                grabber.allowToGrabObject = item.itemName;
            }
        }

        if (selectOwner.GetComponent<Combiner>())
        {
            Combiner combiner = selectOwner.GetComponent<Combiner>();
            combiner.itemSelected = item;
        }
    }

    private void ResetAllSelectionInContainer()
    {
        Transform setupContainer = transform.parent;
        if (setupContainer == null)
        {
            return;
        }

        foreach (Transform child in setupContainer)
        {
            UISetupItemSelection selection = child.GetComponent<UISetupItemSelection>();
            if (selection == null)
            {
                continue;
            }

            selection.SetSelectionState(false);
        }
    }

    private void SetSelectionState(bool chosen)
    {
        isChosen = chosen;

        if (thisBtn != null)
        {
            thisBtn.enabled = !chosen;
        }

        if (chosenImage != null)
        {
            chosenImage.gameObject.SetActive(chosen);
        }
    }

    public void RefreshSelectionFromOwner()
    {
        bool shouldChoose = false;

        if (selectOwner != null && item != null)
        {
            Grabber grabber = selectOwner.GetComponent<Grabber>();
            if (grabber != null && grabber.isSmartGrabber)
            {
                shouldChoose = !string.IsNullOrEmpty(grabber.allowToGrabObject) && grabber.allowToGrabObject == item.itemName;
            }

            Combiner combiner = selectOwner.GetComponent<Combiner>();
            if (combiner != null && combiner.itemSelected == item)
            {
                shouldChoose = true;
            }
        }

        SetSelectionState(shouldChoose);
    }
}
