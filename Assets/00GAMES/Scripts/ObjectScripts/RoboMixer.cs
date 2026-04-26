using UnityEngine;

public class RoboMixer : InteractableObject
{
    [SerializeField]
    public float mixTime = 1f;
    [SerializeField]
    public GameObject mixFX;

    private bool isMixing;

    public override void LevelUp()
    {
        level++;
    }

    void Awake()
    {
        if (mixFX != null)
        {
            mixFX.SetActive(false);
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

        TryMix();
    }

    public void TryMix()
    {
        if (isMixing || itemHolding == null || !isItemInPosition())
        {
            return;
        }

        if (!itemHolding.canMix)
        {
            return;
        }

        StartCoroutine(MixRoutine(itemHolding));
    }

    private System.Collections.IEnumerator MixRoutine(Item mixingItem)
    {
        isMixing = true;

        if (mixFX != null)
        {
            mixFX.SetActive(true);
            mixFX.transform.localScale = new Vector3(0.25f,0.25f,0.25f);
        }

        float duration = Mathf.Max(0.01f, mixTime);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (itemHolding == null || itemHolding != mixingItem)
            {
                if (mixFX != null)
                {
                    mixFX.transform.localScale = Vector3.zero;
                    mixFX.SetActive(false);
                }

                isMixing = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (mixFX != null)
        {
            mixFX.transform.localScale = Vector3.zero;
            mixFX.SetActive(false);
        }

        if (itemHolding == mixingItem && mixingItem != null)
        {
            Item afterMixPrefab = mixingItem.AfterMixItem;
            Transform itemSlot = mixingItem.transform.parent;

            Destroy(mixingItem.gameObject);

            if (afterMixPrefab != null && itemSlot != null)
            {
                // Keep prefab's intended orientation (e.g. X = -90) when parenting into slot.
                Item replacedItem = Instantiate(afterMixPrefab, itemSlot);
                replacedItem.transform.localPosition = Vector3.zero;
                replacedItem.transform.localRotation = afterMixPrefab.transform.localRotation;
                Vector3 desiredWorldScale = afterMixPrefab.transform.localScale;
                Vector3 parentWorldScale = itemSlot.lossyScale;
                replacedItem.transform.localScale = new Vector3(
                    SafeDivide(desiredWorldScale.x, parentWorldScale.x),
                    SafeDivide(desiredWorldScale.y, parentWorldScale.y),
                    SafeDivide(desiredWorldScale.z, parentWorldScale.z));
                itemHolding = replacedItem;
                
                if(replacedItem.itemName == "Unbaked Banh Mi" && GameManager.Instance.isTutorial){
                    GameManager.Instance.MoveToNextStage();
                    ObjectUIManager.Instance.UIPause();
                }
            }
            else
            {
                itemHolding = null;
            }
        }
        isMixing = false;
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
        isMixing = false;

        if (mixFX != null)
        {
            mixFX.transform.localScale = Vector3.zero;
            mixFX.SetActive(false);
        }
    }
}
