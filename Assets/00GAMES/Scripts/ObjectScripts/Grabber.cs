using DG.Tweening;
using UnityEngine;

public class Grabber : InteractableObject
{
    public bool isSmartGrabber = false;
    public string allowToGrabObject = null;
    [SerializeField]
    GameObject grabberHand;
    [SerializeField]
    GameObject smartGrabberHand;
    public override void LevelUp()
    {
        level++;
        canSetup = true;
        canUpgrade = false;
        sellPrice = sellPrice+upgradePrice;
        isSmartGrabber = true;
        grabberHand.SetActive(false);
        smartGrabberHand.SetActive(true);
        allowToGrabObject = null;
        VFXManager.Instance.SpawnVFX(VFXManager.Instance.upgradeFX,new Vector3(this.transform.position.x,
        this.transform.position.y+0.5f,this.transform.position.z));
        GameManager.Instance.startMoney -= upgradePrice;
        
    }
    public override void Grab(){
        if (!grabable || itemHolding != null || itemPosition == null)
        {
            return;
        }
        

        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            return;
        }

        Direction oppositeDirection = GetOppositeDirection(currentDirection);
        Vector3 dir = GetDirectionVector(oppositeDirection);

        if (!gameManager.TryGetAdjacentGridWorldPosition(transform.position, dir, out Vector3 oppositeGridWorld))
        {
            return;
        }

        InteractableObject opposite = gameManager.GetInteractableAtWorldPosition(oppositeGridWorld, this);
        if (opposite == null)
        {
            return;
        }
        if(opposite.itemHolding != null && isSmartGrabber){
            if(opposite.itemHolding.itemName != allowToGrabObject && isSmartGrabber && allowToGrabObject != null)
            {
                return;
            }
        }
        if (opposite is IngreBox box && box.ownItem != null)
        {
            if (Time.time < nextIngreBoxGrabTime)
            {
                return;
            }
            if(isSmartGrabber && box.ownItem.itemName != allowToGrabObject && allowToGrabObject != null)
                return;
            Item spawned = Instantiate(box.ownItem, itemPosition);
            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = box.ownItem.transform.localRotation;
            // keep prefab scale stable under scaled slots
            Vector3 desiredWorldScale = box.ownItem.transform.localScale;
            Vector3 parentWorldScale = itemPosition.lossyScale;
            spawned.transform.localScale = new Vector3(
                SafeDivide(desiredWorldScale.x, parentWorldScale.x),
                SafeDivide(desiredWorldScale.y, parentWorldScale.y),
                SafeDivide(desiredWorldScale.z, parentWorldScale.z));
            itemHolding = spawned;
            itemRestingAtSlot = true;
            nextIngreBoxGrabTime = Time.time + Mathf.Max(0f, gameManager.ingreBoxCoolDown);
            return;
        }

        if (opposite.itemHolding == null || !opposite.isItemInPosition())
        {
            return;
        }

        Item item = opposite.itemHolding;
        opposite.itemHolding = null;
        opposite.itemRestingAtSlot = false;

        itemHolding = item;
        itemRestingAtSlot = false;

        Quaternion worldRot = item.transform.rotation;
        Vector3 worldScale = item.transform.lossyScale;
        item.transform.DOKill();
        item.transform.SetParent(itemPosition, true);
        item.transform.DOLocalMove(Vector3.zero, grabItemMoveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (item == null || itemHolding != item || itemPosition == null)
                {
                    return;
                }

                item.transform.SetParent(itemPosition, false);
                item.transform.localPosition = Vector3.zero;
                item.transform.rotation = worldRot;
                Vector3 parentWorldScale = itemPosition.lossyScale;
                item.transform.localScale = new Vector3(
                    SafeDivide(worldScale.x, parentWorldScale.x),
                    SafeDivide(worldScale.y, parentWorldScale.y),
                    SafeDivide(worldScale.z, parentWorldScale.z));
                itemRestingAtSlot = true;
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
}
