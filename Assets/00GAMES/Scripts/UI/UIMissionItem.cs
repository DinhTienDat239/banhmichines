using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMissionItem : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI itemNameTxt;
    [SerializeField]
    public Image doneMark;
    public Item item;

    public void UpdateMissionItem(bool isDone){
        doneMark.gameObject.SetActive(isDone);
    }
}
