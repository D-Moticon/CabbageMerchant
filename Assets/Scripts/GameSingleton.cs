using UnityEngine;

public class GameSingleton : MonoBehaviour
{
    public static GameSingleton Instance { get; private set; }

    public ObjectPoolManager objectPoolManager;
    public ScreenShaker screenShaker;
    public GameStateMachine gameStateMachine;
    public BoardMetrics boardMetrics;
    
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
