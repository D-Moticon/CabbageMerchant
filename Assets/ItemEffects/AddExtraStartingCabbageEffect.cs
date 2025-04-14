using UnityEngine;

public class AddExtraStartingCabbageEffect : ItemEffect
{
    public int cabbagesToAdd = 1;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddExtraStartingCabbage();
    }

    public override string GetDescription()
    {
        string plural = "";
        if (cabbagesToAdd > 1)
        {
            plural = "s";
        }

        return ($"Spawn {cabbagesToAdd}{plural} additional cabbages at start of round");
    }
}
