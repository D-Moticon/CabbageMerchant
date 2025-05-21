using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enables and styles pet statues in the museum based on SaveManager records.
/// Statues unlock when the player has beaten the game with that pet,
/// and the statue material reflects the highest difficulty beaten.
/// </summary>
public class MuseumManager : MonoBehaviour
{
    [System.Serializable]
    public class DifficultyInfo
    {
        public Difficulty difficulty;
        public Material statueMat;
    }

    [Header("Difficulty Materials")]
    [Tooltip("Mapping from difficulty level to statue material.")]
    public List<DifficultyInfo> difficultyInfos;

    [System.Serializable]
    public class PetStatueInfo
    {
        [Tooltip("Which pet this statue represents.")]
        public PetDefinition petDefinition;

        [Tooltip("The SpriteRenderer or MeshRenderer for the statue.")]
        public Renderer statueRenderer;
    }

    [Header("Pet Statues")]
    [Tooltip("All pet statues in the museum.")]
    public List<PetStatueInfo> petStatueInfos;

    [System.Serializable]
    public class ChaosCabbageStatueInfo
    {
        public ChaosCabbageSO chaosCabbage;
        public Renderer statueRenderer;
    }

    public List<ChaosCabbageStatueInfo> chaosCabbageStatueInfos;
    
    void Start()
    {
        RefreshMuseum();
    }

    /// <summary>
    /// Updates each statue: activates it if beaten, and applies the correct material.
    /// </summary>
    public void RefreshMuseum()
    {
        var save = Singleton.Instance.saveManager;

        foreach (var info in petStatueInfos)
        {
            // Determine pet ID (assuming PetDefinition has a unique identifier)
            string petID = info.petDefinition.dataName;

            // Get the max difficulty beaten for this pet
            int maxDiff = save.GetPetMaxDifficulty(petID);

            // Unlock statue if beaten at least on easiest difficulty (> 0)
            bool unlocked = maxDiff > 0;
            info.statueRenderer.enabled = unlocked;

            if (unlocked)
            {
                // Find the highest DifficultyInfo whose enum value <= maxDiff
                var bestMatch = difficultyInfos
                    .Where(d => d.difficulty.difficultyLevel <= maxDiff)
                    .OrderBy(d => d.difficulty.difficultyLevel)
                    .LastOrDefault();

                if (bestMatch != null)
                {
                    info.statueRenderer.material = bestMatch.statueMat;
                }
                else
                {
                    Debug.LogWarning($"No material found for pet '{petID}' at difficulty {maxDiff}.");
                }
            }
        }

        foreach (var info in chaosCabbageStatueInfos)
        {
            bool unlocked = Singleton.Instance.chaosManager.IsChaosCabbageUnlocked(info.chaosCabbage);
            info.statueRenderer.enabled = unlocked;
        }
    }
}
