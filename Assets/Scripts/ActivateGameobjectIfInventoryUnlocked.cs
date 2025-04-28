using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ActivateGameobjectIfInventoryUnlocked : MonoBehaviour
{
    public GameObject gameObjectToActivate;
    public bool checkOnUpdate = false;
    public PooledObjectData activateVFX;
    public SFXInfo activateSFX;
    private bool activated = false;
    
    private void Awake()
    {
        CheckAndActivate();
    }

    private void Update()
    {
        if (checkOnUpdate)
        {
            CheckAndActivate();
        }
    }

    void CheckAndActivate()
    {
        if (activated)
        {
            return;
        }
        
        List<ItemSlot> lockedInventorySlots = Singleton.Instance.itemManager.itemSlots
            .Where(x => x.isLocked).ToList();
        if (lockedInventorySlots.Count == 0)
        {
            gameObjectToActivate.SetActive(true);
            if (activateVFX != null)
            {
                activateVFX.Spawn(gameObjectToActivate.transform.position);
            }

            activateSFX.Play(gameObjectToActivate.transform.position);

            activated = true;
        }
        else
        {
            gameObjectToActivate.SetActive(false);
        }
    }
}
