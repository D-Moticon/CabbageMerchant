using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Button component that displays a PetDefinition and handles purchase/equip logic,
/// implements IHoverable by delegating to the in-game item.
/// </summary>
[RequireComponent(typeof(Button))]
public class PetButton : MonoBehaviour, IHoverable
{
    [Tooltip("Reference to the pet definition this button represents")] 
    public PetDefinition petDefinition;

    [Tooltip("UI Text for pet name")] 
    public TMP_Text nameText;
    [Tooltip("UI Text for cost or owned state")] 
    public TMP_Text costText;
    [Tooltip("UI Image for pet icon")] 
    public Image iconImage;
    [Tooltip("Visual indicator when this pet is currently selected")] 
    public Image selectedImage;

    public Sprite unownedSprite;

    [Tooltip("Button component")] 
    public Button button;

    public Color unOwnedColor = Color.black;
    public Color ownedColor = Color.white;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }

    public void Start()
    {
        // initialize UI
        if (nameText != null)
            nameText.text = petDefinition.displayName;
        if (iconImage != null)
            iconImage.sprite = petDefinition.downSprite;
        UpdateState();
    }

    void OnClick()
    {
        var ps = Singleton.Instance.playerStats;
        var pm = Singleton.Instance.petManager;

        bool owned = pm.ownedPets.Contains(petDefinition);
        if (!owned)
        {
            // attempt purchase
            if (ps.metaCurrency < petDefinition.cost)
            {
                Singleton.Instance.floaterManager.SpawnInfoFloater(
                    "Can't afford!", transform.position, Color.red);
                return;
            }
            ps.AddMetacurrency(-petDefinition.cost);
            pm.PurchasePet(petDefinition);
        }
        else
        {
            // equip
            pm.SetCurrentPet(petDefinition);
        }
        UpdateState();
        Singleton.Instance.toolTip.ForceToolTipUpdate();
    }

    /// <summary>
    /// Update visual state based on ownership and current selection.
    /// </summary>
    public void UpdateState()
    {
        var ps = Singleton.Instance.playerStats;
        var pm = Singleton.Instance.petManager;

        bool owned = pm.ownedPets.Contains(petDefinition);
        bool active = pm.currentPet == petDefinition;

        // cost text
        if (costText != null)
        {
            if (owned)
            {
                costText.text = "Owned";
            }

            else
            {
                if (ps.metaCurrency >= petDefinition.cost)
                {
                    costText.text = $"<sprite index=0/>{petDefinition.cost.ToString()}";
                }

                else
                {
                    costText.text = $"<sprite index=0/><color=red>{petDefinition.cost.ToString()}</color>";
                }
            }
        }

        // icon transparency
        if (iconImage != null)
        {
            iconImage.color = owned? ownedColor : unOwnedColor;
        }

        // selected indicator
        if (selectedImage != null)
            selectedImage.enabled = active;

        // button interactable
        button.interactable = owned || (ps.metaCurrency >= petDefinition.cost);
    }

    //=============== IHoverable ===============
    public string GetTitleText(HoverableModifier mod = null)
        => Singleton.Instance.petManager.ownedPets.Contains(petDefinition)? petDefinition.itemPrefab.GetTitleText(mod) : "???";
    public string GetDescriptionText(HoverableModifier mod = null)
        => Singleton.Instance.petManager.ownedPets.Contains(petDefinition)? petDefinition.itemPrefab.GetDescriptionText(mod) : "???";
    public string GetTypeText(HoverableModifier mod = null)
        => petDefinition.itemPrefab.GetTypeText(mod);
    public string GetRarityText()
        => "";
    public string GetTriggerText()
        => Singleton.Instance.petManager.ownedPets.Contains(petDefinition)? petDefinition.itemPrefab.GetTriggerText() : "???";
    public Sprite GetImage()
        => Singleton.Instance.petManager.ownedPets.Contains(petDefinition)? petDefinition.itemPrefab.GetImage() : unownedSprite;
    public string GetValueText()
        => "";
}
