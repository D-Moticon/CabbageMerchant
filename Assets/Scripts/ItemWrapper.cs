using System;
using MoreMountains.Feedbacks;
using UnityEngine;

public class ItemWrapper : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Material holofoilMaterial;
    [HideInInspector]public Item item;
    public MMF_Player triggerFeel;
    public ParticleSystem purchaseVFX;
    public SFXInfo purchaseSFX;
    public FloaterReference purchaseFloater;
    public Color purchaseFloaterColor = Color.white;
    public MMF_Player hoveredFeel;
    public SFXInfo hoveredSFX;
    
    private void OnEnable()
    {
        ItemManager.ItemPurchasedEvent += ItemPurchasedListener;
        ToolTip.HoverableHoveredEvent += HoverableHoveredListener;
    }

    private void OnDisable()
    {
        ItemManager.ItemPurchasedEvent -= ItemPurchasedListener;
        ToolTip.HoverableHoveredEvent -= HoverableHoveredListener;
    }

    void ItemPurchasedListener(Item purchasedItem)
    {
        if (purchasedItem == item)
        {
            triggerFeel.PlayFeedbacks();
            purchaseVFX.Play();
            purchaseSFX.Play();
            purchaseFloater.Spawn(item.GetItemPrice().ToString(), this.transform.position, purchaseFloaterColor);
        }
    }

    void HoverableHoveredListener(IHoverable hovered)
    {
        if (hovered == item)
        {
            hoveredFeel.PlayFeedbacks();
            hoveredSFX.Play();
        }
    }

    public void DestroyItem(bool withFX = false)
    {
        Destroy(this.gameObject);
    }
}
