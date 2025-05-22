using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for run-based requirements.
/// </summary>
[Serializable]
public abstract class Requirement
{
    /// <summary>
    /// Returns true if the run history satisfies this requirement.
    /// </summary>
    public abstract bool IsRequirementMet();

    /// <summary>
    /// Returns a human-readable description of this requirement.
    /// </summary>
    public abstract string GetRequirementDescription();
}

/// <summary>
/// Requires that the player has beaten at least one run with a specific pet at or above a given difficulty.
/// </summary>
[Serializable]
public class RunBeatenWithPetRequirement : Requirement
{
    public PetDefinition requiredPet;
    public int minDifficultyLevel;

    public override bool IsRequirementMet()
    {
        if (requiredPet == null) return false;
        return Singleton.Instance.saveManager.GetAllRunRecords()
            .Any(r => r.petID == requiredPet.dataName && r.difficultyLevel >= minDifficultyLevel);
    }

    public override string GetRequirementDescription()
    {
        string petName = requiredPet != null ? requiredPet.displayName : "<none>";
        string diffName = RequirementUtils.DifficultyName(minDifficultyLevel);
        return $"Complete a run with {petName} on {diffName} difficulty or higher.";
    }
}

/// <summary>
/// Requires that the player has beaten at least one run at or above the specified minimum difficulty.
/// </summary>
[Serializable]
public class RunBeatenOnDifficultyRequirement : Requirement
{
    public int minDifficultyLevel;

    public override bool IsRequirementMet()
    {
        return Singleton.Instance.saveManager.GetAllRunRecords()
            .Any(r => r.difficultyLevel >= minDifficultyLevel);
    }

    public override string GetRequirementDescription()
    {
        string diffName = RequirementUtils.DifficultyName(minDifficultyLevel);
        return $"Complete a run on {diffName} difficulty or higher.";
    }
}

/// <summary>
/// Requires that the player has beaten at least one run under a given time (in seconds) and minimum difficulty.
/// </summary>
[Serializable]
public class RunBeatenUnderTimeRequirement : Requirement
{
    public int minDifficultyLevel;
    public float maxRunTime;

    public override bool IsRequirementMet()
    {
        return Singleton.Instance.saveManager.GetAllRunRecords()
            .Any(r => r.difficultyLevel >= minDifficultyLevel && r.runTime <= maxRunTime);
    }

    public override string GetRequirementDescription()
    {
        string diffName = RequirementUtils.DifficultyName(minDifficultyLevel);
        return $"Complete a run on {diffName} difficulty or higher in under {maxRunTime:F1} seconds.";
    }
}

/// <summary>
/// Requires that the player has beaten at least one run with total bonk value at or above a threshold and minimum difficulty.
/// </summary>
[Serializable]
public class RunBeatenWithBonkValueRequirement : Requirement
{
    public int minDifficultyLevel;
    public double minTotalBonkValue;

    public override bool IsRequirementMet()
    {
        return Singleton.Instance.saveManager.GetAllRunRecords()
            .Any(r => r.difficultyLevel >= minDifficultyLevel && r.totalBonkValue >= minTotalBonkValue);
    }

    public override string GetRequirementDescription()
    {
        string diffName = RequirementUtils.DifficultyName(minDifficultyLevel);
        return $"Complete a run on {diffName} difficulty or higher with at least {minTotalBonkValue} bonk value.";
    }
}

/// <summary>
/// Requires that the player has beaten at least one run while equipping a specific chaos cabbage at or above a given difficulty.
/// </summary>
[Serializable]
public class RunBeatenWithChaosCabbageRequirement : Requirement
{
    public ChaosCabbageSO requiredChaosCabbage;
    public int minDifficultyLevel;

    public override bool IsRequirementMet()
    {
        if (requiredChaosCabbage == null) return false;
        return Singleton.Instance.saveManager.GetAllRunRecords()
            .Any(r => r.difficultyLevel >= minDifficultyLevel && r.chaosCabbageIDs.Contains(requiredChaosCabbage.dataName));
    }

    public override string GetRequirementDescription()
    {
        string cabbageName = requiredChaosCabbage != null ? requiredChaosCabbage.displayName : "<none>";
        string diffName = RequirementUtils.DifficultyName(minDifficultyLevel);
        return $"Complete a run on {diffName} difficulty or higher with Chaos Cabbage of {cabbageName} equipped.";
    }
}

/// <summary>
/// Requires that the player has beaten at least one run with at least a given number of chaos cabbages equipped at or above a given difficulty.
/// </summary>
[Serializable]
public class RunBeatenWithNumberChaosCabbagesRequirement : Requirement
{
    public int minDifficultyLevel;
    public int minChaosCount;

    public override bool IsRequirementMet()
    {
        return Singleton.Instance.saveManager.GetAllRunRecords()
            .Any(r => r.difficultyLevel >= minDifficultyLevel && r.chaosCabbageIDs.Count >= minChaosCount);
    }

    public override string GetRequirementDescription()
    {
        string diffName = RequirementUtils.DifficultyName(minDifficultyLevel);
        return $"Complete a run on {diffName} difficulty or higher with at least {minChaosCount} chaos cabbages equipped.";
    }
}

/// <summary>
/// Helper for converting difficulty levels to names.
/// </summary>
public static class RequirementUtils
{
    public static string DifficultyName(int level)
    {
        switch (level)
        {
            case 0: return "Easy";
            case 1: return "Easy";
            case 2: return "Medium";
            case 3: return "Hard";
            default: return $"Level {level}";
        }
    }
}
