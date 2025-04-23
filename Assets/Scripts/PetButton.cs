using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Button component that displays a PetDefinition and handles purchase/equip logic.
/// </summary>
[RequireComponent(typeof(Button))]
public class PetButton : MonoBehaviour
{
    [Tooltip("Reference to the pet definition this button represents")] 
    public PetDefinition def;

    [Tooltip("UI Text for pet name")] public TMP_Text nameText;
    public TMP_Text costText;
    [Tooltip("UI Image for pet icon")] public Image iconImage;
    [Tooltip("Button component")] public Button button;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }

    public void Start()
    {
        // initialize UI
        if (nameText != null) nameText.text = def.displayName;
        if (iconImage != null) iconImage.sprite = def.downSprite;
        if (costText != null) costText.text = def.cost.ToString();
        UpdateState();
    }

    /// <summary>
    /// Called when player clicks this pet button.
    /// Purchases if not owned, otherwise equips.
    /// </summary>
    void OnClick()
    {
        var pm = Singleton.Instance.petManager;
        if (!pm.ownedPets.Contains(def))
        {
            // purchase
            Singleton.Instance.playerStats.AddCoins(-def.cost);
            pm.PurchasePet(def);
        }
        else
        {
            // equip
            pm.SetCurrentPet(def);
        }
        UpdateState();
    }

    /// <summary>
    /// Update visual state based on ownership and current selection.
    /// </summary>
    public void UpdateState()
    {
        var pm = Singleton.Instance.petManager;
        bool owned = pm.ownedPets.Contains(def);
        bool active = pm.currentPet == def;

        // disable if not enough coins and not owned
        button.interactable = owned || (Singleton.Instance.playerStats.coins >= def.cost);

        // highlight active pet
        var colors = button.colors;
        colors.normalColor = active ? Color.green : Color.white;
        button.colors = colors;
    }
}