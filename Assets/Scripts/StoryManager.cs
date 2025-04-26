using UnityEngine;

public class StoryManager : MonoBehaviour
{
    private SaveManager saveManager;

    public Dialogue overworldIntro;
    public Dialogue gameplayIntro;
    public Dialogue firstRunVictory;
    public Dialogue firstRunDefeat;
    public Dialogue shopIntro;
    public Dialogue dojoIntro;
    public Dialogue libraryIntro;
    
    void Awake()
    {
        // cache a reference to your SaveManager in the scene
        saveManager = Singleton.Instance.saveManager;
    }

    void OnEnable()
    {
        SaveManager.DataLoadedEvent            += OnDataLoaded;
        OverworldManager.overworldStartedAction += OnOverworldStarted;
        RunManager.RunFinishedEvent            += OnRunFinished;
        GameStateMachine.BoardFinishedPopulatingAction += OnBoardFinishedPopulating;
        RunManager.SceneChangedEvent += SceneChangedListener;
    }

    

    void OnDisable()
    {
        SaveManager.DataLoadedEvent            -= OnDataLoaded;
        OverworldManager.overworldStartedAction -= OnOverworldStarted;
        RunManager.RunFinishedEvent            -= OnRunFinished;
        GameStateMachine.BoardFinishedPopulatingAction -= OnBoardFinishedPopulating;
        RunManager.SceneChangedEvent -= SceneChangedListener;
    }

    // optional: if you want something right after data loads
    private void OnDataLoaded()
    {
        // e.g. you could trigger a “welcome” on first ever launch here
    }

    // ─── first time they enter the Overworld ───
    private void OnOverworldStarted()
    {
        if (!saveManager.HasSeenOverworldIntro())
        {
            // fire your dialogue system
            Singleton.Instance.dialogueManager.PlayDialogue(overworldIntro);
            saveManager.MarkSeenOverworldIntro();
        }
    }

    // ─── first time they finish a run ───
    private void OnRunFinished(RunManager.RunCompleteParams rep)
    {
        /*if (rep.success && !saveManager.HasWonFirstRun())
        {
            Singleton.Instance.dialogueManager.PlayDialogue(firstRunVictory);
            saveManager.MarkWonFirstRun();
        }
        
        else if (!rep.success && !saveManager.HasLostFirstRun())
        {
            Singleton.Instance.dialogueManager.PlayDialogue(firstRunDefeat);
            saveManager.MarkLostFirstRun();
        }*/
    }

    void OnBoardFinishedPopulating()
    {
        if (!saveManager.HasSeenGameplayTutorial())
        {
            Singleton.Instance.dialogueManager.PlayDialogue(gameplayIntro);
            saveManager.MarkSeenGameplayTutorial();
        }
    }
    
    private void SceneChangedListener(string s)
    {
        if (s == "Shop")
        {
            if (!saveManager.HasSeenShopIntro())
            {
                Singleton.Instance.dialogueManager.PlayDialogue(shopIntro);
                saveManager.MarkSeenShopIntro();
            }
        }

        if (s == "Dojo")
        {
            if (!saveManager.HasSeenDojoIntro())
            {
                Singleton.Instance.dialogueManager.PlayDialogue(dojoIntro);
                saveManager.MarkSeenDojoIntro();
            }
        }

        if (s == "Library")
        {
            if (!saveManager.HasSeenLibraryIntro())
            {
                Singleton.Instance.dialogueManager.PlayDialogue(libraryIntro);
                saveManager.MarkSeenLibraryIntro();
            }
        }
    }
}