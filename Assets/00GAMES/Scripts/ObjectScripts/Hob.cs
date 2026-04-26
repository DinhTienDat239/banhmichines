using UnityEngine;

public class Hob : InteractableObject
{
    [SerializeField]
    public float hobTime = 1f;
    [SerializeField]
    public GameObject hobFX;

    private bool isHobbing;

    public override void LevelUp()
    {
        level++;
    }

    void Awake()
    {
        if (hobFX != null)
        {
            hobFX.SetActive(false);
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

        TryHob();
    }

    public void TryHob()
    {
        if (isHobbing || itemHolding == null || !isItemInPosition())
        {
            return;
        }

        if (!itemHolding.canHob)
        {
            return;
        }

        StartCoroutine(HobRoutine(itemHolding));
    }

    private System.Collections.IEnumerator HobRoutine(Item hobbingItem)
    {
        isHobbing = true;

        if (hobFX != null)
        {
            hobFX.SetActive(true);
            hobFX.transform.localScale = Vector3.one;
        }

        float duration = Mathf.Max(0.01f, hobTime);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (itemHolding == null || itemHolding != hobbingItem)
            {
                if (hobFX != null)
                {
                    hobFX.transform.localScale = Vector3.zero;
                    hobFX.SetActive(false);
                }

                isHobbing = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (hobFX != null)
        {
            hobFX.transform.localScale = Vector3.zero;
            hobFX.SetActive(false);
        }

        if (itemHolding == hobbingItem && hobbingItem != null)
        {
            Item afterHobPrefab = hobbingItem.AfterHobItem;
            Transform itemSlot = hobbingItem.transform.parent;

            Destroy(hobbingItem.gameObject);

            if (afterHobPrefab != null && itemSlot != null)
            {
                Item replacedItem = Instantiate(afterHobPrefab, itemSlot);
                replacedItem.transform.localPosition = Vector3.zero;
                replacedItem.transform.localRotation = afterHobPrefab.transform.localRotation;
                Vector3 desiredWorldScale = afterHobPrefab.transform.localScale;
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

        isHobbing = false;
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
        isHobbing = false;

        if (hobFX != null)
        {
            hobFX.transform.localScale = Vector3.zero;
            hobFX.SetActive(false);
        }
    }
}
