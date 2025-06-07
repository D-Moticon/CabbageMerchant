using System.Collections;
using UnityEngine;

public class BossStrikeTask : DialogueTask
{
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Singleton.Instance.uiManager.ShowNotification("<color=green><wave a=.5>Boss Phase Beaten!");
        BossFightManager bossFightManager = Singleton.Instance.bossFightManager;
        bossFightManager.bossStrikeAnimator.GetComponentInChildren<SpriteRenderer>().sprite = bossFightManager.boss.bossSprite;
        bossFightManager.bossStrikeAnimator.gameObject.SetActive(true);
        bossFightManager.bossStrikeAnimator.Play("BossBeat");
        yield return new WaitForSeconds(2f);
    }
    
    
    
}
