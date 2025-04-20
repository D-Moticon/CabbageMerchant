using System;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class ItemSlot : MonoBehaviour
{
    public int slotNumber = 0;
    public Item currentItem;
    public GameObject priceTextParent;
    public TMP_Text priceText;
    public bool isEventSlot = false;
    public MMF_Player bumpFeel;
    public SFXInfo spawnSFX;
    public SFXInfo itemAddedSFX;

    public enum AllowedTypes
    {
        any,
        itemOnly,
        perkOnly,
        weaponOnly
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
}
