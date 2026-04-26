using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBuyItemSelection : MonoBehaviour
{
    public GameObject objectBuyPref;
    [SerializeField]
    public Image objectIcon;
    public TextMeshProUGUI objectName;
    public string objectDescription;
    public int objectPrice;
    public TextMeshProUGUI objectPriceTxt;
    public void BuyItem(){
        if (objectBuyPref == null || GameManager.Instance == null)
        {
            return;
        }

        GameManager gameManager = GameManager.Instance;

        if (gameManager.startMoney < objectPrice)
        {
            return;
        }

        if (!gameManager.TryGetMiddlestEmptyGridPosition(out Vector3 emptyGridPosition))
        {
            return;
        }

        Vector3 spawnPosition = new Vector3(emptyGridPosition.x, 0.25f, emptyGridPosition.z);
        GameObject obj = Instantiate(objectBuyPref, spawnPosition, objectBuyPref.transform.rotation);
        obj.SetActive(true);
        gameManager.RegisterInteractableObject(obj.GetComponent<InteractableObject>());
        gameManager.startMoney -= objectPrice;
        if(obj.GetComponent<InteractableObject>().objectName == "Grabber" && GameManager.Instance.isTutorial){
            GameManager.Instance.MoveToNextStage();
        }
    }
}
