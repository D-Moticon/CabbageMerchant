using System;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

/// <summary>
/// Button representing a Chaos Cabbage.  Uses UI Image and material property manipulation to indicate equip state.
/// </summary>
public class ChaosCabbageButton : MonoBehaviour, IHoverable
{
    [Header("UI Colors")]
    public Color unequippedColor = Color.grey;
    public Color unownedColor = new Color(0f, 0f, 0f, 0.5f);
    public Sprite unownedHoverSprite;

    [Header("Material Property (UI) ")]
    [Tooltip("Shader float property on the Image's material to adjust for unequipped state.")]
    public string propertyName = "_OutlineWidth";
    [Tooltip("Value to set when unequipped.")]
    public float unequippedPropertyValue = 1f;

    [Header("References")]
    public ChaosCabbageSO chaosCabbage;
    public Image image;
    public SFXInfo equipSFX;
    public PooledObjectData equipVFX;
    public MMF_Player equipFeel;

    public enum State
    {
        unowned,
        unequipped,
        equipped
    }

    private State state;
    private Material _defaultMaterial;

    private void Awake()
    {
        if (image == null)
        {
            Debug.LogError("ChaosCabbageButton: Missing Image reference.");
            return;
        }
        // Cache the original shared material
        _defaultMaterial = image.material;
    }

    private void OnMouseDown()
    {
        switch (state)
        {
            case State.unowned:
                // can't equip
                break;
            case State.unequipped:
                SetCabbageEquipped();
                break;
            case State.equipped:
                SetCabbageUnequipped();
                break;
        }
    }

    private void OnEnable()
    {
        ApplyInitialState();
    }

    public void ApplyInitialState()
    {
        if (chaosCabbage == null)
            return;

        bool unlocked = Singleton.Instance.chaosManager.IsChaosCabbageUnlocked(chaosCabbage);
        bool equipped = unlocked && Singleton.Instance.chaosManager.IsChaosCabbageEquipped(chaosCabbage);

        if (!unlocked)
            SetCabbageUnowned();
        else if (equipped)
            SetCabbageEquipped();
        else
            SetCabbageUnequipped();
    }

    private void SetCabbageEquipped()
    {
        // UI color
        image.color = Color.white;
        // Reset to original material
        image.material = _defaultMaterial;
        state = State.equipped;

        Singleton.Instance.chaosManager.EquipChaosCabbage(chaosCabbage);

        // Feedback
        equipSFX?.Play();
        if (equipVFX != null)
            equipVFX.Spawn(transform.position);
        equipFeel?.PlayFeedbacks();
    }

    private void SetCabbageUnequipped()
    {
        image.color = unequippedColor;
        ApplyUnequippedMaterial();
        state = State.unequipped;

        Singleton.Instance.chaosManager.UnequipChaosCabbage(chaosCabbage);
    }

    private void SetCabbageUnowned()
    {
        image.color = unownedColor;
        // Still show default material
        image.material = _defaultMaterial;
        state = State.unowned;

        Singleton.Instance.chaosManager.UnequipChaosCabbage(chaosCabbage);
    }

    private void ApplyUnequippedMaterial()
    {
        if (_defaultMaterial == null)
            return;
        // Instantiate a new material instance so others aren't affected
        var matInst = new Material(_defaultMaterial);
        matInst.SetFloat(propertyName, unequippedPropertyValue);
        image.material = matInst;
    }

    public string GetTitleText(HoverableModifier hoverableModifier = null)
    {
        if (Singleton.Instance.chaosManager.IsChaosCabbageUnlocked(chaosCabbage))
        {
            return chaosCabbage.displayName;
        }

        return "???";
    }

    public string GetDescriptionText(HoverableModifier hoverableModifier = null)
    {
        if (Singleton.Instance.chaosManager.IsChaosCabbageUnlocked(chaosCabbage))
        {
            return chaosCabbage.item.GetDescriptionText();
        }

        return "";
    }

    public string GetTypeText(HoverableModifier hoverableModifier = null)
    {
        if (Singleton.Instance.chaosManager.IsChaosCabbageUnlocked(chaosCabbage))
        {
            return ("<chaos>Chaos Cabbage</chaos>");
        }

        return "";
    }

    public string GetRarityText()
    {
        if (Singleton.Instance.chaosManager.IsChaosCabbageUnlocked(chaosCabbage))
        {
            return ("<chaos>Chaotic</chaos>");
        }

        return "";
    }

    public string GetTriggerText()
    {
        return "";
    }

    public Sprite GetImage()
    {
        if (Singleton.Instance.chaosManager.IsChaosCabbageUnlocked(chaosCabbage))
        {
            return chaosCabbage.item.icon;
        }

        return unownedHoverSprite;
    }

    public string GetValueText()
    {
        return "";
    }
}
