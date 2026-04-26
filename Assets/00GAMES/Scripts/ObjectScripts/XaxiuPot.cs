using UnityEngine;

public class XaxiuPot : InteractableObject
{
    [SerializeField]
    public float xaxiuTime = 1f;
    [SerializeField]
    public GameObject xaxiuFX;

    private bool isXaxiuing;

    public override void LevelUp()
    {
        level++;
    }

    void Awake()
    {
        if (xaxiuFX != null)
        {
            xaxiuFX.SetActive(false);
        }
    }

    void Update()
    {
        if (!GameManager.Instance.isRunning)
            return;

        if (grabable)
        {
            Grab();
        }

        if (pushable)
        {
            Push();
        }

        TryXaxiu();
    }

    public void TryXaxiu()
    {
        if (isXaxiuing || itemHolding == null || !isItemInPosition())
        {
            return;
        }

        if (!itemHolding.canXaxiu)
        {
            return;
        }

        StartCoroutine(XaxiuRoutine(itemHolding));
    }

    private System.Collections.IEnumerator XaxiuRoutine(Item xaxiuingItem)
    {
        isXaxiuing = true;

        if (xaxiuFX != null)
        {
            xaxiuFX.SetActive(true);
            xaxiuFX.transform.localScale = Vector3.one;
        }

        float duration = Mathf.Max(0.01f, xaxiuTime);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (itemHolding == null || itemHolding != xaxiuingItem)
            {
                if (xaxiuFX != null)
                {
                    xaxiuFX.transform.localScale = Vector3.zero;
                    xaxiuFX.SetActive(false);
                }

                isXaxiuing = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (xaxiuFX != null)
        {
            xaxiuFX.transform.localScale = Vector3.zero;
            xaxiuFX.SetActive(false);
        }

        if (itemHolding == xaxiuingItem && xaxiuingItem != null)
        {
            Item afterXaxiuPrefab = xaxiuingItem.AfterXaxiuItem;
            Transform itemSlot = xaxiuingItem.transform.parent;

            Destroy(xaxiuingItem.gameObject);

            if (afterXaxiuPrefab != null && itemSlot != null)
            {
                Item replacedItem = Instantiate(afterXaxiuPrefab, itemSlot);
                replacedItem.transform.localPosition = Vector3.zero;
                replacedItem.transform.localRotation = afterXaxiuPrefab.transform.localRotation;
                Vector3 desiredWorldScale = afterXaxiuPrefab.transform.localScale;
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

        isXaxiuing = false;
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
        isXaxiuing = false;

        if (xaxiuFX != null)
        {
            xaxiuFX.transform.localScale = Vector3.zero;
            xaxiuFX.SetActive(false);
        }
    }
}
