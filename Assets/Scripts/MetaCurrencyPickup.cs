using System;
using UnityEngine;
using TMPro;

public class MetaCurrencyPickup : MonoBehaviour
{
    public TMP_Text currencyValueText;
    public int currentValue;
    public PooledObjectData pickupVFX;
    public SFXInfo pickupSFX;

    private void OnTriggerEnter2D(Collider2D other)
    {
        MapCharacter mc = other.GetComponent<MapCharacter>();
        if (mc == null)
        {
            return;
        }

        if (pickupVFX != null)
        {
            pickupVFX.Spawn(this.transform.position);
        }

        pickupSFX.Play();
        
        Singleton.Instance.playerStats.AddMetacurrency(currentValue);
        gameObject.SetActive(false);
    }

    public void SetValue(int newValue)
    {
        currentValue = newValue;
        currencyValueText.text = $"{newValue:F0}";
    }
}
