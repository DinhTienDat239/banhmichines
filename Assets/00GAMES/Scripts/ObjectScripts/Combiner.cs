using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Combiner : InteractableObject
{
    [SerializeField]
    float combineTime = 2f;
    [SerializeField]
    GameObject combineFx;
    public List<Item> itemCollectedList;
    public Item itemSelected;
    bool isCombining = false;
    private Sequence combineSequence;

    public void CheckCombine(){
        int fitItemCount = 0;
        foreach(Item i in itemCollectedList){
            foreach(Item i2 in itemSelected.combinedFromList){
                if(i.itemName == i2.itemName){
                    fitItemCount++;
                }
            }
        }
        if(fitItemCount == itemSelected.combinedFromList.Length && !isCombining){
            Combine();
        }
    }
    void Combine(){
        isCombining = true;
        Transform itemSlot = itemPosition.transform;
        combineSequence = DOTween.Sequence();
        combineSequence.SetTarget(this);
        combineSequence.AppendCallback(() => {
            combineFx.SetActive(true);
        });
        combineSequence.AppendInterval(2f);
        combineSequence.AppendCallback(() => {
            itemCollectedList = new List<Item>();
            Item itemCombined = Instantiate(itemSelected, itemSlot);
            itemCombined.transform.localPosition = Vector3.zero;
            itemCombined.transform.localRotation = itemSelected.transform.localRotation;
            Vector3 desiredWorldScale = itemSelected.transform.localScale;
            Vector3 parentWorldScale = itemSlot.lossyScale;
            itemCombined.transform.localScale = new Vector3(
                SafeDivide(desiredWorldScale.x, parentWorldScale.x),
                SafeDivide(desiredWorldScale.y, parentWorldScale.y),
                SafeDivide(desiredWorldScale.z, parentWorldScale.z));
            itemHolding = itemCombined; 
            itemRestingAtSlot = true;
            combineFx.SetActive(false);
            isCombining = false;
        });
    }

    private float SafeDivide(float value, float divisor)
    {
        if (Mathf.Approximately(divisor, 0f))
        {
            return value;
        }

        return value / divisor;
    }

    public void ResetCombinerOnPause()
    {
        if (combineSequence != null && combineSequence.IsActive())
        {
            combineSequence.Kill();
            combineSequence = null;
        }

        isCombining = false;

        if (combineFx != null)
        {
            combineFx.SetActive(false);
        }

        if (itemCollectedList == null)
        {
            itemCollectedList = new List<Item>();
            return;
        }

        for (int i = 0; i < itemCollectedList.Count; i++)
        {
            Item item = itemCollectedList[i];
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        itemCollectedList.Clear();
    }
    
}
