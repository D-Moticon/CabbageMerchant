using UnityEngine;

public class SurvivalModeItemEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.survivalManager.EnableSurvivalMode();
    }

    public override string GetDescription()
    {
        return ($"Enable survival mode:" + "\n" +
                $"-Food meter decreases every time ball is fired.  Eat food to replenish."
                + "\n" +
                $"-Viruses occasionally spawn on board.  Hitting them with ball causes dysentery that depletes health every turn.  Consume medkit from shop to cure.");
    }
}
