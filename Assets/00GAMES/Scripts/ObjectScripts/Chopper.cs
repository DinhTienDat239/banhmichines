using UnityEngine;

public class Chopper : InteractableObject
{
    [SerializeField]
    public float chopTime = 1f;
    [SerializeField]
    public GameObject chopFX;

    private bool isChopping;

    public override void LevelUp()
    {
        level++;
    }

    void Awake()
    {
        if (chopFX != null)
        {
            chopFX.SetActive(false);
        }
    }

    void Update()
    {   if(!GameManager.Instance.isRunning)
            return;

        
        TryChop();
        if (grabable)
        {
            Grab();
        }

        if (pushable && !isChopping)
        {
            Push();
        }

    }

    public void TryChop()
    {
        if (isChopping || itemHolding == null || !isItemInPosition())
        {
            return;
        }

        if (!itemHolding.canChop)
        {
            return;
        }
        isChopping = true;
        StartCoroutine(ChopRoutine(itemHolding));
    }

    private System.Collections.IEnumerator ChopRoutine(Item choppingItem)
    {
        

        if (chopFX != null)
        {
            chopFX.SetActive(true);
            chopFX.transform.localScale = Vector3.one;
        }

        float duration = Mathf.Max(0.01f, chopTime);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (itemHolding == null || itemHolding != choppingItem)
            {
                if (chopFX != null)
                {
                    chopFX.transform.localScale = Vector3.zero;
                    chopFX.SetActive(false);
                }

                isChopping = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (chopFX != null)
        {
            chopFX.transform.localScale = Vector3.zero;
            chopFX.SetActive(false);
        }

        if (itemHolding == choppingItem && choppingItem != null)
        {
            Item afterChopPrefab = choppingItem.AfterChopItem;
            Transform itemSlot = choppingItem.transform.parent;

            Destroy(choppingItem.gameObject);

            if (afterChopPrefab != null && itemSlot != null)
            {
                Item replacedItem = Instantiate(afterChopPrefab, itemSlot);
                replacedItem.transform.localPosition = Vector3.zero;
                replacedItem.transform.localRotation = afterChopPrefab.transform.localRotation;
                Vector3 desiredWorldScale = afterChopPrefab.transform.localScale;
                Vector3 parentWorldScale = itemSlot.lossyScale;
                replacedItem.transform.localScale = new Vector3(
                    SafeDivide(desiredWorldScale.x, parentWorldScale.x),
                    SafeDivide(desiredWorldScale.y, parentWorldScale.y),
                    SafeDivide(desiredWorldScale.z, parentWorldScale.z));
                itemHolding = replacedItem;
            }
            else
            {
                itemHolding = null;
            }
        }

        isChopping = false;
    }

    private float SafeDivide(float value, float divisor)
    {
        if (Mathf.Approximately(divisor, 0f))
        {
            return value;
        }

        return value / divisor;
    }

    public void ForceStopAndClearFx()
    {
        StopAllCoroutines();
        isChopping = false;

        if (chopFX != null)
        {
            chopFX.transform.localScale = Vector3.zero;
            chopFX.SetActive(false);
        }
    }
}
