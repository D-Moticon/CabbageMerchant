using System.Collections;
using UnityEngine;

public class ChaosTempleDialogueTask : DialogueTask
{
    public DialogueSequenceTask introSequence;
    public DialogueSequenceTask easyDifficultyLine;
    public DialogueSequenceTask mediumDifficultyLine;
    public DialogueSequenceTask hardDifficultyLine;
    
    public DialogueSequenceTask worthyOfChaosCabbageTask;

    public DialogueLine noPetToChooseLine;
    public DialogueLine petChoseLine;

    public DialogueSequenceTask postCCAddLine;
    
    public DialogueSequenceTask farewellLine;

    private Item instantiatedCCPerk;
    private bool perkAdded = false;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Task introTask = new Task(introSequence.RunTask(dc));
        while (introTask.Running) yield return null;

        IEnumerator difficultyRoutine = null;

        int difficulty = Singleton.Instance.playerStats.currentDifficulty.difficultyLevel;
        PetDefinition currentPet = Singleton.Instance.petManager.currentPet;
        
        switch (difficulty)
        {
            case 1:
                difficultyRoutine = easyDifficultyLine.RunTask(dc);
                break;
            case 2:
                difficultyRoutine = mediumDifficultyLine.RunTask(dc);
                break;
            case 3:
                difficultyRoutine = hardDifficultyLine.RunTask(dc);
                break;
            default:
                difficultyRoutine = hardDifficultyLine.RunTask(dc);
                break;
        }

        Task difficultyTask = new Task(difficultyRoutine);
        while (difficultyTask.Running) yield return null;
        
        if (difficulty <= 1)
        {
            Task fareWellTask = new Task(farewellLine.RunTask(dc));
            while (fareWellTask.Running) yield return null;
            yield break;
        }

        
        else if (Singleton.Instance.chaosManager.GetChaosCabbageFromPetDef(currentPet) == null)
        {
            Task noPetToChooseTask = new Task(noPetToChooseLine.RunTask(dc));
            while (noPetToChooseTask.Running) yield return null;
            Task fareWellTask = new Task(farewellLine.RunTask(dc));
            while (fareWellTask.Running) yield return null;
            yield break;
        }
 
        else
        {
            Task worthyOfCCTask = new Task(worthyOfChaosCabbageTask.RunTask(dc));
            while (worthyOfCCTask.Running) yield return null;
            
            DialogueLine petChoosesLine = new DialogueLine();
            petChoosesLine.dialogueLine = $"{currentPet.displayName} is playing with one of the Chaos Cabbages!";
            petChoosesLine.overrideSprite = currentPet.itemPrefab.icon;
            petChoosesLine.playSFX = true;

            Task petChoosesTask = new Task(petChoosesLine.RunTask(dc));
            while (petChoosesTask.Running) yield return null;

            Task petChoseTask = new Task(petChoseLine.RunTask(dc));
            while (petChoseTask.Running) yield return null;

            ItemManager.ItemPurchasedEvent += ItemPurchasedListener;
            
            var slot = GameObject.Instantiate(
                Singleton.Instance.itemManager.itemSlotPrefab,
                dc.dialogueBox.itemSlotParent);
            slot.transform.localPosition = new Vector3(0f, 0f, 0f);
            slot.isEventSlot = true;

            ChaosCabbageSO chaosCabbageToGive = Singleton.Instance.chaosManager.GetChaosCabbageFromPetDef(currentPet);
            Item ChaosCabbageItemPrefab = chaosCabbageToGive.item;
            
            instantiatedCCPerk = Singleton.Instance.itemManager.GenerateItemWithWrapper(ChaosCabbageItemPrefab,
                slot.transform.position, slot.transform);
            Singleton.Instance.itemManager.AddItemToSlot(instantiatedCCPerk, slot);

            perkAdded = false;
            while (!perkAdded)
            {
                yield return null;
            }
            ItemManager.ItemPurchasedEvent -= ItemPurchasedListener;
            slot.DestroySlot();

            Singleton.Instance.chaosManager.AddChaosCabbageToSaveFile(chaosCabbageToGive);
            yield return new WaitForSeconds(3f);

            DialogueLine youCollectedLine = new DialogueLine();
            youCollectedLine.dialogueLine = $"You collected the Chaos Cabbage of {chaosCabbageToGive.displayName}!";

            Task youCollectedTask = new Task(youCollectedLine.RunTask(dc));
            while (youCollectedTask.Running) yield return null;

            Task postCCAddTask = new Task(postCCAddLine.RunTask(dc));
            while (postCCAddTask.Running) yield return null;
            Task fareWellTask = new Task(farewellLine.RunTask(dc));
            while (fareWellTask.Running) yield return null;

            yield break;
        }
    }

    void ItemPurchasedListener(Item item)
    {
        if (item != instantiatedCCPerk)
        {
            return;
        }
        perkAdded = true;
    }
}
