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

    [Tooltip("Sprite to show when unowned")]
    public Sprite unownedSprite;

    [Header("Demo Mode")]
    [Tooltip("Indicator GameObject to enable when this pet is full-game-only in demo")]
    public GameObject demoOnlyIndicator;

    private Button button;
    public Color unOwnedColor = Color.black;
    public Color ownedColor   = Color.white;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        if (demoOnlyIndicator != null)
            demoOnlyIndicator.SetActive(false);
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }

    public void Start()
    {
        if (nameText  != null) nameText.text   = petDefinition.displayName;
        if (iconImage != null) iconImage.sprite = petDefinition.downSprite;
        UpdateState();
    }

    public void UpdateState()
    {
        var ps = Singleton.Instance.playerStats;
        var pm = Singleton.Instance.petManager;
        bool owned      = pm.ownedPets.Contains(petDefinition);
        bool active     = pm.currentPet == petDefinition;
        bool demo       = Singleton.Instance.buildManager.IsDemoMode();
        bool fullOnly   = !petDefinition.InDemo;  // now full-game-only

        // cost text
        if (costText != null)
        {
            if (owned) costText.text = "Owned";
            else if (ps.metaCurrency >= petDefinition.cost)
                costText.text = $"<sprite index=0/>{petDefinition.cost}";
            else
                costText.text = $"<sprite index=0/><color=red>{petDefinition.cost}</color>";
        }

        // icon tint & selection highlight
        if (iconImage     != null) iconImage.color     = owned ? ownedColor : unOwnedColor;
        if (selectedImage != null) selectedImage.enabled = active;

        // base interactability
        button.interactable = owned || ps.metaCurrency >= petDefinition.cost;

        // ** demo-mode override: block full-game-only pets **
        if (demo && fullOnly)
        {
            button.interactable = false;
            if (demoOnlyIndicator != null)
                demoOnlyIndicator.SetActive(true);
        }
        else if (demoOnlyIndicator != null)
        {
            demoOnlyIndicator.SetActive(false);
        }
    }

    private void OnClick()
    {
        // block clicks for full-game-only pets in demo
        if (Singleton.Instance.buildManager.IsDemoMode() && !petDefinition.InDemo)
            return;

        var ps = Singleton.Instance.playerStats;
        var pm = Singleton.Instance.petManager;
        bool owned = pm.ownedPets.Contains(petDefinition);

        if (!owned)
        {
            if (ps.metaCurrency < petDefinition.cost)
            {
                Singleton.Instance.floaterManager
                    .SpawnInfoFloater("Can't afford!", transform.position, Color.red);
                return;
            }
            ps.AddMetacurrency(-petDefinition.cost);
            pm.PurchasePet(petDefinition);
        }
        else
        {
            pm.SetCurrentPet(petDefinition);
        }

        UpdateState();
        Singleton.Instance.toolTip.ForceToolTipUpdate();
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
