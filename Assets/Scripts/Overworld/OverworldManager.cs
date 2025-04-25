using UnityEngine;

/// <summary>
/// Handles overworld-specific setup, including spawning all owned pets at Start.
/// Attach this to a central GameObject in your Overworld scene.
/// </summary>
public class OverworldManager : MonoBehaviour
{
    public static System.Action overworldStartedAction;
    
    void Start()
    {
        // Spawn all owned pets via your PetManager
        if (Singleton.Instance.petManager != null)
        {
            Singleton.Instance.petManager.SpawnOwnedOverworldPets();

            // Ensure the currently active pet follows immediately
            var current = Singleton.Instance.petManager.currentPet;
            if (current != null)
                Singleton.Instance.petManager.SetCurrentPet(current);
        }
        else
        {
            Debug.LogWarning("OverworldManager: petManager not found on Singleton.Instance.");
        }
        
        overworldStartedAction?.Invoke();
    }
}