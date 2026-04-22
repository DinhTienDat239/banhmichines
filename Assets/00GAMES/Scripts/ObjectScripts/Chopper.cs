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
    {
        if (grabable)
        {
            Grab();
        }

        if (pushable)
        {
            Push();
        }

        TryChop();
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

        StartCoroutine(ChopRoutine(itemHolding));
    }

    private System.Collections.IEnumerator ChopRoutine(Item choppingItem)
    {
        isChopping = true;

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
                Item replacedItem = Instantiate(afterChopPrefab, itemSlot.position, itemSlot.rotation, itemSlot);
                replacedItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                itemHolding = replacedItem;
            }
            else
            {
                itemHolding = null;
            }
        }

        isChopping = false;
    }
}
