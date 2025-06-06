using System;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class ItemSlot : MonoBehaviour
{
    public int slotNumber = 0;
    public Item currentItem;
    public SpriteRenderer frameSpriteRenderer;
    public GameObject priceTextParent;
    public TMP_Text priceText;
    public bool isEventSlot = false;
    public MMF_Player bumpFeel;
    public SFXInfo spawnSFX;
    public SFXInfo itemAddedSFX;
    public bool isLocked;
    public SpriteRenderer lockedSR;
    public PooledObjectData unlockVFX;
    public SFXInfo unlockSFX;
    public TMP_Text slotNumberText;

    [Header("Freezing")]
    public Color frozenFrameColor = Color.red;
    private Color originalFrameColor;
    [HideInInspector]public bool isFrozen = false;
    public GameObject frozenGameObject;
    public PooledObjectData frozenVFX;
    public SFXInfo frozenSFX;
    public PooledObjectData unfrozenVFX;
    public SFXInfo unfrozenSFX;
    

    public enum AllowedTypes
    {
        any,
        itemOnly,
        perkOnly,
        weaponOnly,
        itemOrConsumable
    }

    public AllowedTypes allowedTypes;

    private void OnEnable()
    {
        PlayerStats.CoinsUpdated += PlayerCoinsUpdatedListener;
        if (bumpFeel != null)
        {
            bumpFeel.PlayFeedbacks();
        }
        spawnSFX.Play();

        if (!isLocked)
        {
            UnLockSlot(false);
        }

        if (frameSpriteRenderer != null)
        {
            originalFrameColor = frameSpriteRenderer.color;
        }
        
    }

    private void OnDisable()
    {
        PlayerStats.CoinsUpdated -= PlayerCoinsUpdatedListener;
    }

    public void SetPriceText()
    {
        if (priceText == null)
        {
            return;
        }

        if (currentItem == null)
        {
            priceTextParent.SetActive(false);
        }

        else
        {
            priceTextParent.SetActive(true);

            string colorPrefix = "";
            
            if (Singleton.Instance.playerStats.coins < currentItem.GetItemPrice())
            {
                colorPrefix = "<color=red>";
            }
            
            priceText.text = colorPrefix + Helpers.FormatWithSuffix(currentItem.GetItemPrice());
        }
        
        
    }

    public void HidePriceText()
    {
        if (priceText == null)
        {
            return;
        }
        priceTextParent.SetActive(false);
    }

    public void ShowPriceText()
    {
        if (priceText == null)
        {
            return;
        }
        priceTextParent.SetActive(true);
    }

    public void PlayerCoinsUpdatedListener(double newCoins)
    {
        SetPriceText();
    }

    public void PlayItemAddedToSlotFX()
    {
        if (bumpFeel != null)
        {
            bumpFeel.PlayFeedbacks(this.transform.position,0.125f);
        }
        
        itemAddedSFX.Play();
    }

    public void DestroySlot()
    {
        Destroy(this.gameObject);
    }

    public void LockSlot()
    {
        isLocked = true;
        if (lockedSR != null)
        {
            lockedSR.enabled = true;
        }

        if (currentItem != null)
        {
            currentItem.itemWrapper.spriteRenderer.enabled = false;
        }
    }

    public void UnLockSlot(bool playFX = true)
    {
        isLocked = false;
        if (lockedSR != null)
        {
            lockedSR.enabled = false;
        }
        
        if (currentItem != null)
        {
            currentItem.itemWrapper.spriteRenderer.enabled = true;
        }

        if (playFX)
        {
            unlockVFX.Spawn(this.transform.position);
            unlockSFX.Play(this.transform.position);
        }
    }

    public void SetSlotNumber(int newSlotNumber)
    {
        slotNumber = newSlotNumber;
        if (slotNumberText != null)
        {
            slotNumberText.gameObject.SetActive(true);
            slotNumberText.text = (newSlotNumber+1).ToString();
        }
    }

    public void FreezeSlot()
    {
        isFrozen = true;
        frozenGameObject.SetActive(true);
        if (frozenVFX != null)
        {
            frozenVFX.Spawn(this.transform.position);
        }

        frozenSFX.Play();
        frameSpriteRenderer.color = frozenFrameColor;

        if (currentItem != null)
        {
           currentItem.FreezeItem();
        }
    }

    public void UnFreezeSlot()
    {
        isFrozen = false;
        frozenGameObject.SetActive(false);
        if (unfrozenVFX != null)
        {
            unfrozenVFX.Spawn(this.transform.position);
        }

        unfrozenSFX.Play();
        frameSpriteRenderer.color = originalFrameColor;
        
        if (currentItem != null)
        {
            currentItem.UnFreezeItem();
        }
    }
}
