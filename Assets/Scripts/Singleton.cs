using UnityEngine;
using UnityEngine.Serialization;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }

    public Vector2 offScreenPosition = new Vector2(-50f, -50f);
    public PlayerInputManager playerInputManager;
    public FloaterManager floaterManager;
    public AudioManager audioManager;
    public MusicManager musicManager;
    public PrefabReferences prefabReferences;
    public ItemManager itemManager;
    public ItemGraveyard itemGraveyard;
    public UIManager uiManager;
    public PlayerStats playerStats;
    public ScreenShaker screenShaker;
    public RunManager runManager;
    public ToolTip toolTip;
    public ObjectPoolManager objectPoolManager;
    public GameHintManager gameHintManager;
    public BoundsManager boundsManager;
    public MenuManager menuManager;
    public PetManager petManager;
    public SaveManager saveManager;
    public PauseManager pauseManager;
    public BuildManager buildManager;
    public EffectsManager effectsManager;
    public DialogueManager dialogueManager;
    public LaunchModifierManager launchModifierManager;
    public OverworldSceneChanger overworldSceneChanger;
    public TemporaryBonusManager temporaryBonusManager;
    public ChaosManager chaosManager;
    public SpecialRuleManager specialRuleManager;
    public SkinManager skinManager;
    public SurvivalManager survivalManager;
    public BossFightManager bossFightManager;
    
    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        //Application.targetFrameRate = 165;
        Application.targetFrameRate = 999;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
        }
        
        if (buildManager.buildMode == BuildManager.BuildMode.release)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break();
        }
    }
}
