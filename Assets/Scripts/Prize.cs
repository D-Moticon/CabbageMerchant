using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public abstract class Prize
{
    public Sprite icon;
    public abstract IEnumerator AwardPrize(DialogueContext dc);
    public DialogueLine awardLine;
}

[System.Serializable]
public class CoinsPrize : Prize
{
    public int numberCoins = 10;
    public override IEnumerator AwardPrize(DialogueContext dc)
    {
        Singleton.Instance.playerStats.AddCoins(numberCoins);
        awardLine.dialogueLine = $"+{numberCoins} coins!";
        Task awardLineTask = new Task(awardLine.RunTask(dc));
        while (awardLineTask.Running) yield return null;
        
        yield break;
    }
}
    
[System.Serializable]
public class LivesPrize : Prize
{
    public int numberLives = 1;
    public override IEnumerator AwardPrize(DialogueContext dc)
    {
        Singleton.Instance.playerStats.AddLife(numberLives);
        
        Task awardLineTask = new Task(awardLine.RunTask(dc));
        while (awardLineTask.Running) yield return null;
        
        yield break;
    }
}
    
[System.Serializable]
public class ItemPrize : Prize
{
    public List<Item> itemPool;
    public override IEnumerator AwardPrize(DialogueContext dc)
    {
        Task awardLineTask = new Task(awardLine.RunTask(dc));
        while (awardLineTask.Running) yield return null;
        
        int rand = Random.Range(0, itemPool.Count);
        Item itemPrefab = itemPool[rand];
        Item item = Singleton.Instance.itemManager.GenerateItemWithWrapper(itemPrefab);
        ItemSlot itemSlot = DialogueTask.CreateItemSlot(dc);
        Singleton.Instance.itemManager.AddItemToSlot(item, itemSlot);
        
        dc.dialogueBox.ActivateButtons(1);
        DialogueButton passButton = dc.dialogueBox.choiceButtons[0];
        passButton.buttonPressed = false;
        passButton.SetText("Pass");

        while (!passButton.buttonPressed && item != null && !Singleton.Instance.itemManager.GetItemsInInventory().Contains(item))
        {
            yield return null;
        }

        if (item!= null && !Singleton.Instance.itemManager.GetItemsInInventory().Contains(item))
        {
            item.DestroyItem();
        }
        
        itemSlot.DestroySlot();
        dc.dialogueBox.HideAllChoiceButtons();
        
        yield break;
    }
}
