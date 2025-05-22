using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkinManager : MonoBehaviour
{
    public Skin defaultSkin;
    public Skin currentSkin;

    public SkinDatabase skinDatabase;
    
    public delegate void SkinDelegate(Skin s);
    public static SkinDelegate SkinEquippedEvent;

    [HideInInspector] public List<Skin> ownedSkins;
    public SFXInfo skinSelectedSFX;
    
    private void OnEnable()
    {
        currentSkin = defaultSkin;
    }

    private void Start()
    {
        LoadOwnedSkinsFromSave();
    }

    public void EquipSkin(Skin skin)
    {
        currentSkin = skin;
        SkinEquippedEvent?.Invoke(skin);
    }

    public void PurchaseSkin(Skin skin)
    {
        if (!ownedSkins.Contains(skin))
        {
            ownedSkins.Add(skin);
            EquipSkin(skin);

            var ids = ownedSkins.Select(p => p.dataName).ToList();
            Singleton.Instance.saveManager.SetOwnedSkinIDs(ids);
            
            skinSelectedSFX.Play();
        }
    }
    
    public void LoadOwnedSkinsFromSave()
    {
        var save = Singleton.Instance.saveManager;
        List<string> savedIds = save.GetOwnedSkinIDs();
        ownedSkins.Clear();

        foreach (string id in savedIds)
        {
            SkinDatabase.SkinInfo skinInfo = skinDatabase.skinInfos.Find(p => p.skin.dataName == id);
            if (skinInfo != null)
                ownedSkins.Add(skinInfo.skin);
        }
        // Re-apply the current pet state
        EquipSkin(currentSkin);
    }
}
