using System.Collections;
using UnityEngine;

public class ItemEffectDialogueTask : DialogueTask
{
    [SerializeReference]
    public ItemEffect itemEffect;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        TriggerContext tc = new TriggerContext();
        if (GameSingleton.Instance != null)
        {
            tc.ball = GameSingleton.Instance.gameStateMachine.GetRandomActiveBall();
        }
        itemEffect.TriggerItemEffect(tc);
        yield break;
    }
}
