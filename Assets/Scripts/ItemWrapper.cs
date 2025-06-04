using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

public class ItemWrapper : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Material holofoilMaterial;
    public Material petMaterial;
    public Material ghostMaterial;
    public Sprite mysteriousSprite;
    [HideInInspector]public Item item;
    public MMF_Player triggerFeel;
    public ParticleSystem purchaseVFX;
    public SFXInfo purchaseSFX;
    public FloaterReference purchaseFloater;
    public Color purchaseFloaterColor = Color.white;
    public MMF_Player hoveredFeel;
    public SFXInfo hoveredSFX;
    public Slider cooldownSlider;
    public TMP_Text extraText;
    public MMF_Player extraTextFeel;
    public PooledObjectData destroyVFX;
    public SFXInfo destroySFX;
    
    private void OnEnable()
    {
        ItemManager.ItemPurchasedEvent += ItemPurchasedListener;
        ToolTip.HoverableHoveredEvent += HoverableHoveredListener;
        extraText.enabled = false;
        extraText.text = "";
    }

    private void OnDisable()
    {
        ItemManager.ItemPurchasedEvent -= ItemPurchasedListener;
        ToolTip.HoverableHoveredEvent -= HoverableHoveredListener;
    }

    private void Update()
    {
        if (item.hasCooldown)
        {
            if (item.cooldownCounter > 0)
            {
                cooldownSlider.gameObject.SetActive(true);
                cooldownSlider.value = item.cooldownCounter / item.cooldownDuration;
            }

            else
            {
                cooldownSlider.gameObject.SetActive(false);
            }
        }
    }

    public void InitializeItemWrapper(Item theItem)
    {
        item = theItem;
        if (item.hasCooldown)
        {
            cooldownSlider.gameObject.SetActive(true);
        }

        else
        {
            cooldownSlider.gameObject.SetActive(false);
        }

        if (item.itemType == Item.ItemType.Pet)
        {
            spriteRenderer.material = petMaterial;
        }
        
        ApplyMaterialPropertyOverrides();
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
        if (hovered as Object == item as Object)
        {
            hoveredFeel.PlayFeedbacks();
            hoveredSFX.Play();
        }
    }

    public void DestroyItem(bool withFX = false)
    {
        if (withFX)
        {
            if (destroyVFX != null) destroyVFX.Spawn(this.transform.position);
            destroySFX.Play();
        }

        Destroy(this.gameObject);
    }

    public void SetExtraText(string text)
    {
        if (extraText != null)
        {
            extraText.enabled = true;
            extraText.text = text;
        }

        if (extraTextFeel != null)
        {
            extraTextFeel.PlayFeedbacks();
        }
    }

    public void SetMysterious()
    {
        spriteRenderer.sprite = mysteriousSprite;
    }

    public void EndMysterious()
    {
        spriteRenderer.sprite = item.icon;
        if (item.customMaterial != null)
        {
            spriteRenderer.material = item.customMaterial;
        }
    }
    
    public void SetSprite(Sprite s)
    {
        spriteRenderer.sprite = s;
    }
    
    private void ApplyMaterialPropertyOverrides()
    {
        if (item.materialPropertyOverrides == null || item.materialPropertyOverrides.Count == 0)
            return;

        var block = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(block);

        foreach (var ov in item.materialPropertyOverrides)
            ov.ApplyTo(block);

        spriteRenderer.SetPropertyBlock(block);
    }

    public void InitializeItemTemporary()
    {
        extraText.enabled = true;
        extraText.text = item.numberShotsBeforeDestroy.ToString();
    }

    public void UpdateTemporaryCountdown(int numShots)
    {
        extraText.text = numShots.ToString();
        extraTextFeel.PlayFeedbacks();
    }
}
