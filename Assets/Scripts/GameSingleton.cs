using UnityEngine;

public class GameSingleton : MonoBehaviour
{
    public static GameSingleton Instance { get; private set; }

    public ObjectPoolManager objectPoolManager;
    public GameStateMachine gameStateMachine;
    public BoardMetrics boardMetrics;
    public Transform gameSceneParent;
    public GasFluidManager gasSim;
    public LiquidManager liquidSim;
    public FluidRTReferences fluidRTReferences;
    
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
