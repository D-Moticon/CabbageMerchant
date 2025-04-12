using UnityEngine;

public class MapSingleton : MonoBehaviour
{
    public static MapSingleton Instance { get; private set; }

    public MapManager mapManager;
    public MapGenerator mapGenerator;
    
    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

    }
}
