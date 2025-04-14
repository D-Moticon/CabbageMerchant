using System;
using UnityEngine;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public int slotNumber = 0;
    public Item currentItem;
    public GameObject priceTextParent;
    public TMP_Text priceText;

    private void OnEnable()
    {
        PlayerStats.CoinsUpdated += PlayerCoinsUpdatedListener;
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
}
