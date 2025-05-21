using UnityEngine;
using System.Collections.Generic;

public class DifficultyRequirement : Requirement
{
    public List<Difficulty> difficulties;
    public override bool IsRequirementMet()
    {
        for (int i = 0; i < difficulties.Count; i++)
        {
            if (Singleton.Instance.playerStats.currentDifficulty == difficulties[i])
            {
                return true;
            }
        }

        return false;
    }
}
