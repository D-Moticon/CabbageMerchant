using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Serialization;

public class BossFightManager : MonoBehaviour
{
    [HideInInspector]public Boss boss;
    [HideInInspector]public GameStateMachine gsm;
    private int _currentPhaseIndex = 0;
    private double _currentPhaseStartHealth;
    public bool isBossFight = false;

    public void SetBossFight(Boss b)
    {
        if (b == null)
        {
            isBossFight = false;
            boss = null;
            return;
        }

        isBossFight = true;
        boss = b;
    }
    
    public void StartBossFight()
    {
        _currentPhaseIndex = 0;
        StartCoroutine(RunPhase());
    }

    private IEnumerator RunPhase()
    {
        var phase = boss.phases[_currentPhaseIndex];
        Difficulty difficulty = Singleton.Instance.playerStats.currentDifficulty;

        DialogueContext dc = new DialogueContext();
        dc.dialogueBox = Singleton.Instance.dialogueManager.dialogueBox;
        
        // 1) Pre‐board effects
        yield return StartCoroutine(Singleton.Instance.dialogueManager.DialogueTaskRoutine(phase.preBoardPopulateTasks));
        
        // 2) Populate the board
        bool boardPopulated = false;
        System.Action onPopulated = () => boardPopulated = true;
        GameStateMachine.BoardFinishedPopulatingAction += onPopulated;
        
        gsm.ChangeState(new GameStateMachine.PopulateBoardState());
        while (!boardPopulated)
            yield return null;
        GameStateMachine.BoardFinishedPopulatingAction -= onPopulated;
        
        yield return StartCoroutine(gsm.PopulateBoardWith(phase.boardPopulateInfo));

        // 3) Post‐board effects
        yield return StartCoroutine(Singleton.Instance.dialogueManager.DialogueTaskRoutine(phase.postBoardPopulateTasks));
        
        // 4) Look up this phase’s health for the current difficulty
        var info = phase.difficultyInfos
                        .FirstOrDefault(d => d.difficulty == difficulty);
        if (info == null)
        {
            Debug.LogError(
                $"[BossFight] Phase #{_currentPhaseIndex} has no health entry for difficulty '{difficulty}'"
            );
            yield break;
        }

        double phaseHealth = info.totalHealth;
        gsm.roundGoal = phaseHealth;

        // 6) Let the FSM run Populate→Aiming→Bouncing→Scoring as normal,
        //    but watch for the round-goal event to know if the player cleared it.

        bool phaseCleared = false;
        void OnOver(double over) => phaseCleared = (over >= 1);
        GameStateMachine.RoundGoalOverHitEvent += OnOver;


        // wait for ScoringState to finish
        yield return WaitForScoringExit();

        GameStateMachine.RoundGoalOverHitEvent -= OnOver;

        if (!phaseCleared)
        {
            // player lost — handle your “lose a life” logic here
            yield break;
        }

        // 7) Run any post-shot effects
        yield return StartCoroutine(Singleton.Instance.dialogueManager.DialogueTaskRoutine(phase.postBounceStateExitedTasks));

        // 8) Next phase or victory
        _currentPhaseIndex++;
        if (_currentPhaseIndex < boss.phases.Count)
            yield return StartCoroutine(RunPhase());
        else
            OnBossDefeated();
    }

    private IEnumerator WaitForScoringExit()
    {
        // Wait until we're in ScoringState…
        while (!(gsm.currentState is GameStateMachine.ScoringState))
            yield return null;

        // …then wait for it to exit:
        bool done = false;
        void OnExit() => done = true;
        GameStateMachine.ExitingScoringAction += OnExit;
        while (!done) yield return null;
        GameStateMachine.ExitingScoringAction -= OnExit;
    }
    
    void OnBossDefeated()
    {
        // restore normal UI
        // fire any victory dialogue
        // hand back control to RunManager, etc.
    }
}
