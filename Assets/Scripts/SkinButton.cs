using System;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Button representing a skin, with purchase requirements and IHoverable implementation.
/// </summary>
[RequireComponent(typeof(Button))]
public class SkinButton : MonoBehaviour, IHoverable
{
    [Tooltip("Which Skin asset this button represents")]
    public Skin skin;

    [Header("UI References")]
    public TMP_Text nameText;
    public TMP_Text costText;
    public Image iconImage;
    public Image selectedImage;
    public Sprite unownedSprite;

    [Header("Colors")]
    public Color requirementsUnmetColor = Color.black;
    public Color unOwnedColor = Color.grey;
    public Color ownedColor = Color.white;

    [Header("Demo Mode")]
    [Tooltip("Indicator GameObject to enable when this skin is demo-only")]
    public GameObject demoOnlyIndicator;

    [FormerlySerializedAs("isDemoRestricted")] [HideInInspector]
    public bool notInDemo = false;

    private Button _button;

    public PooledObjectData equipVFX;
    public SFXInfo equipSFX;

    void OnEnable()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        if (demoOnlyIndicator != null)
            demoOnlyIndicator.SetActive(false);
    }

    void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public void Start()
    {
        if (nameText != null && skin != null)
            nameText.text = skin.displayName;
        if (iconImage != null && skin != null)
            iconImage.sprite = skin.downSprite;

        UpdateState();
    }

    public void UpdateState()
    {
        if (skin == null) return;

        bool demoMode = Singleton.Instance.buildManager.IsDemoMode();
        var ps = Singleton.Instance.playerStats;
        var sm = Singleton.Instance.skinManager;
        bool owned = sm.ownedSkins.Contains(skin);
        bool requirementsMet = skin.requirements == null || skin.requirements.All(r => r.IsRequirementMet());

        // cost text
        if (costText != null)
        {
            if (owned)
                costText.text = "Owned";
            else
            {
                string costStr = skin.cost.ToString();
                costText.text = ps.metaCurrency >= skin.cost && requirementsMet
                    ? $"<sprite=0>{costStr}"
                    : $"<sprite=0><color=red>{costStr}</color>";
            }
        }

        // icon tint
        if (iconImage != null)
            iconImage.color = owned ? ownedColor : unOwnedColor;

        // selected highlight
        if (selectedImage != null)
            selectedImage.enabled = owned && sm.currentSkin == skin;

        // base interactability
        _button.interactable = owned || requirementsMet;

        // ** demo-only override **
        if (demoMode && notInDemo)
        {
            SetToDemoRestricted();
        }
        else if (demoOnlyIndicator != null)
        {
            demoOnlyIndicator.SetActive(false);
        }
    }

    public void SetToDemoRestricted()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        _button.interactable = false;

        if (demoOnlyIndicator != null)
            demoOnlyIndicator.SetActive(true);
    }

    private void OnClick()
    {
        // swallow any clicks if demo-only
        if (Singleton.Instance.buildManager.IsDemoMode() && notInDemo)
            return;

        var ps = Singleton.Instance.playerStats;
        var sm = Singleton.Instance.skinManager;
        bool owned = sm.ownedSkins.Contains(skin);
        bool requirementsMet = skin.requirements == null || skin.requirements.All(r => r.IsRequirementMet());

        if (!owned)
        {
            if (!requirementsMet)
            {
                Singleton.Instance.floaterManager.SpawnInfoFloater(
                    "Requirements not met!", transform.position, requirementsUnmetColor);
                return;
            }
            if (ps.metaCurrency < skin.cost)
            {
                Singleton.Instance.floaterManager.SpawnInfoFloater(
                    "Can't afford!", transform.position, Color.red);
                return;
            }
            ps.AddMetacurrency(-skin.cost);
            sm.PurchaseSkin(skin);
        }
        else
        {
            if (equipVFX != null) equipVFX.Spawn(transform.position);
            equipSFX.Play();
            sm.EquipSkin(skin);
        }

        UpdateState();
        Singleton.Instance.toolTip.ForceToolTipUpdate();
    }

    //============= IHoverable =============
    public string GetTitleText(HoverableModifier mod = null)
    {
        return Singleton.Instance.skinManager.ownedSkins.Contains(skin)
            ? skin.displayName
            : "???";
    }

    public string GetDescriptionText(HoverableModifier mod = null)
    {
        bool owned = Singleton.Instance.skinManager.ownedSkins.Contains(skin);

        // unowned: list requirements
        if (skin.requirements == null || skin.requirements.Count == 0)
            return "No requirements.";

        var sb = new StringBuilder();
        foreach (var req in skin.requirements)
        {
            bool met = req.IsRequirementMet();
            if (owned)
            {
                sb.AppendLine(skin.description);
            }
            string desc = req.GetRequirementDescription();
            string color = met ? "green" : "red";
            sb.AppendLine($"<color={color}>{desc}</color>");
        }
        return sb.ToString();
    }

    public string GetTypeText(HoverableModifier mod = null) => "Appearance";
    public string GetRarityText() => string.Empty;
    public string GetTriggerText() => string.Empty;

    public Sprite GetImage() =>
        Singleton.Instance.skinManager.ownedSkins.Contains(skin)
            ? skin.downSprite
            : unownedSprite;

    public string GetValueText() => string.Empty;
}
