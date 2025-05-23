using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;

public class SurvivalManager : MonoBehaviour
{
    public int maxFoodLevel = 10;
    private int currentFoodLevel = 10;
    public int foodValue = 6;

    public int maxHealthLevel = 10;
    private int currentHealthLevel = 10;
    private bool isSick = false;

    public PooledObjectData virusPooledObject;
    public float virusSpawnChance = 0.1f;
    
    public GameObject survivalCanvas;
    
    public Slider foodMeter;
    public TMP_Text foodMeterText;
    public MMF_Player foodMeterFeel;
    
    public Slider healthMeter;
    public TMP_Text healthMeterText;
    public MMF_Player healthMeterFeel;
    public Image virusIcon;
    
    [HideInInspector]public bool survivalModeOn = false;

    private void OnEnable()
    {
        GameStateMachine.BallFiredEvent += BallFiredListener;
        RunManager.RunEndedEvent += RunEndedListener;
        ItemManager.ItemSoldEvent += ItemSoldListener;
        GameStateMachine.BoardFinishedPopulatingAction += BoardFinishedPopulatingListener;
        
        DisableSurvivalMode();
    }

    

    private void OnDisable()
    {
        GameStateMachine.BallFiredEvent -= BallFiredListener;
        RunManager.RunEndedEvent -= RunEndedListener;
        ItemManager.ItemSoldEvent -= ItemSoldListener;
        GameStateMachine.BoardFinishedPopulatingAction -= BoardFinishedPopulatingListener;
    }

    public void EnableSurvivalMode()
    {
        survivalModeOn = true;
        survivalCanvas.SetActive(true);
        currentFoodLevel = maxFoodLevel;
        currentHealthLevel = maxHealthLevel;
        isSick = false;
        UpdateFoodMeter();
        UpdateHealthMeter();
    }

    public void DisableSurvivalMode()
    {
        survivalModeOn = false;
        survivalCanvas.SetActive(false);
    }
    
    private void BallFiredListener(Ball b)
    {
        if (!survivalModeOn)
        {
            return;
        }
        
        currentFoodLevel--;
        UpdateFoodMeter();
        
        if (currentFoodLevel <= 0)
        {
            DieOfHunger();
        }

        if (isSick)
        {
            currentHealthLevel--;
            UpdateHealthMeter();
            if (currentHealthLevel <= 0)
            {
                DieOfSickness();
            }
        }
    }
    
    private void ItemSoldListener(Item item)
    {
        if (!survivalModeOn)
        {
            return;
        }
        
        if (item.itemType != Item.ItemType.Consumable
            && item.itemName != "Kebab"
            && item.itemName != "Egg")
        {
            return;
        }

        if (item.itemName == "Medkit")
        {
            return;
        }

        else
        {
            AddFood(foodValue);
        }
        
    }

    public void AddFood(int amount)
    {
        currentFoodLevel+=amount;
        if (currentFoodLevel > maxFoodLevel)
        {
            currentFoodLevel = maxFoodLevel;
        }
        UpdateFoodMeter();
    }
    
    public void CureSick()
    {
        isSick = false;
        currentHealthLevel = maxHealthLevel;
        UpdateHealthMeter();
        Singleton.Instance.uiManager.ShowNotification("Dysentery cured!");
    }

    private void BoardFinishedPopulatingListener()
    {
        if (!survivalModeOn)
        {
            return;
        }
        
        if (GameSingleton.Instance == null || virusPooledObject == null)
            return;


        float rand = Random.value;
        if (rand > virusSpawnChance)
        {
            return;
        }
        
        var slots = GameSingleton.Instance.gameStateMachine.bonkableSlots;
        if (slots == null || slots.Count < 2)
            return;
        
        var slotA = slots[Random.Range(0, slots.Count)];
        int idxA = slots.IndexOf(slotA);
        int idxB = (idxA + 1) % slots.Count;
        var slotB = slots[idxB];

        float dist = Vector2.Distance(slotA.transform.position, slotB.transform.position);
        if (dist > 6f)
        {
            idxB = (idxA - 1 + slots.Count) % slots.Count;
            slotB = slots[idxB];
        }

        Vector3 spawnPos = (slotA.transform.position + slotB.transform.position) * 0.5f;
        var virus = virusPooledObject.Spawn(spawnPos, Quaternion.identity);
    }
    
    public void MakeSick()
    {
        isSick = true;
        Singleton.Instance.uiManager.ShowNotification("You got dysentery!");
        UpdateHealthMeter();
    }
    
    void UpdateFoodMeter()
    {
        foodMeter.maxValue = maxFoodLevel;
        foodMeter.value = currentFoodLevel;
        foodMeterText.text = currentFoodLevel.ToString();
        foodMeterFeel.PlayFeedbacks();
    }

    void UpdateHealthMeter()
    {
        healthMeter.maxValue = maxHealthLevel;
        healthMeter.value = currentHealthLevel;
        healthMeterText.text = currentHealthLevel.ToString();
        healthMeterFeel.PlayFeedbacks();

        if (isSick)
        {
            virusIcon.gameObject.SetActive(true);
        }

        else
        {
            virusIcon.gameObject.SetActive(false);
        }
    }
    
    private void RunEndedListener()
    {
        DisableSurvivalMode();
    }

    void DieOfHunger()
    {
        if (!survivalModeOn)
        {
            return;
        }
        
        Singleton.Instance.uiManager.ShowNotification("<color=red>You died of hunger</color>");
        Singleton.Instance.runManager.FinishRun(false, "<color=red>You died of hunger</color>");
    }

    void DieOfSickness()
    {
        if (!survivalModeOn)
        {
            return;
        }
        
        Singleton.Instance.uiManager.ShowNotification("<color=red>You died of dysentery</color>");
        Singleton.Instance.runManager.FinishRun(false, "<color=red>You died of dysentery</color>");
    }
}
