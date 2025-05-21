using System;
using UnityEngine;
using TMPro;

public class TemporaryBonusManager : MonoBehaviour
{
    public int nextItemHolofoilStacks = 0;
    public GameObject nextItemHolofoilIcon;
    public TMP_Text nextItemHolofoilText;


    private void OnEnable()
    {
        RunManager.RunEndedEvent += RunEndedListener;
        ItemManager.ItemAddedToSlotEvent += ItemAddedListener;
        ResetAllModifiers();
    }

    private void OnDisable()
    {
        RunManager.RunEndedEvent -= RunEndedListener;
        ItemManager.ItemAddedToSlotEvent -= ItemAddedListener;
    }

    void RunEndedListener()
    {
        ResetAllModifiers();
    }

    void ResetAllModifiers()
    {
        nextItemHolofoilStacks = 0;
        nextItemHolofoilIcon.SetActive(false);
        nextItemHolofoilText.text = "";
    }

    public void AddHolofoilStacks(int number)
    {
        nextItemHolofoilStacks += number;
        nextItemHolofoilIcon.SetActive(true);
        nextItemHolofoilText.text = $"{nextItemHolofoilStacks}x";
    }

    void ItemAddedListener(Item item, ItemSlot slot)
    {
        if (nextItemHolofoilStacks > 0)
        {
            item.SetHolofoil();
            nextItemHolofoilStacks--;
            if (nextItemHolofoilStacks <= 0)
            {
                nextItemHolofoilStacks = 0;
                nextItemHolofoilIcon.SetActive(false);
            }
        }
    }
}
