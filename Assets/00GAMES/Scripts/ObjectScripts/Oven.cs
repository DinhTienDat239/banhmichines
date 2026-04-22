using UnityEngine;

public class Oven : InteractableObject
{
    [SerializeField]
    public float ovenTime = 3f;
    [SerializeField]
    public GameObject ovenFX;

    private bool isOvening;

    public override void LevelUp()
    {
        level++;
    }

    void Awake()
    {
        if (ovenFX != null)
        {
            ovenFX.SetActive(false);
        }
    }

    void Update()
    {
        if (grabable)
        {
            Grab();
        }

        if (pushable)
        {
            Push();
        }

        TryOven();
    }

    public void TryOven()
    {
        if (isOvening || itemHolding == null || !isItemInPosition())
        {
            return;
        }

        if (!itemHolding.canOven)
        {
            return;
        }

        StartCoroutine(OvenRoutine(itemHolding));
    }

    private System.Collections.IEnumerator OvenRoutine(Item oveningItem)
    {
        isOvening = true;

        if (ovenFX != null)
        {
            ovenFX.SetActive(true);
            ovenFX.transform.localScale = Vector3.one;
        }

        float duration = Mathf.Max(0.01f, ovenTime);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (itemHolding == null || itemHolding != oveningItem)
            {
                if (ovenFX != null)
                {
                    ovenFX.transform.localScale = Vector3.zero;
                    ovenFX.SetActive(false);
                }

                isOvening = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (ovenFX != null)
        {
            ovenFX.transform.localScale = Vector3.zero;
            ovenFX.SetActive(false);
        }

        if (itemHolding == oveningItem && oveningItem != null)
        {
            Item afterOvenPrefab = oveningItem.AfterOvenItem;
            Transform itemSlot = oveningItem.transform.parent;

            Destroy(oveningItem.gameObject);

            if (afterOvenPrefab != null && itemSlot != null)
            {
                Item replacedItem = Instantiate(afterOvenPrefab, itemSlot.position, itemSlot.rotation, itemSlot);
                replacedItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                itemHolding = replacedItem;
            }
            else
            {
                itemHolding = null;
            }
        }

        isOvening = false;
    }
}
