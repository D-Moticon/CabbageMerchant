using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using System;

public class BossFightManager : MonoBehaviour
{
    [HideInInspector]public Boss boss;
    [HideInInspector]public GameStateMachine gsm;
    [HideInInspector]public int _currentPhaseIndex = 0;
    private double _currentPhaseStartHealth;
    [HideInInspector]public bool isBossFight = false;

    public class BossPhaseParams
    {
        public int phaseNumber = 0;
    }

    public delegate void BossPhaseDelegate(BossPhaseParams bpp);
    public static BossPhaseDelegate bossPhaseStartedEvent;
    
    public static Action BossPhaseBeatStateEnterEvent;
    public static Action BossPhaseBeatStateExitedEvent;

    public SFXInfo bossFullBeatSFX;
    public PooledObjectData phaseBeatCabbagePopVFX;
    public SFXInfo phaseBeatCabbagePopSFX;

    public Animator bossStrikeAnimator;

    private Coroutine currentPhaseCoroutine;
    
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
        if (!boss.bossMusic.IsNull)
        {
            bool forceRestart = false;
            if (Singleton.Instance.musicManager.GetMusicPhase() > 0)
            {
                forceRestart = true;
                Singleton.Instance.musicManager.SetMusicPhase(0);
            }
            
            Singleton.Instance.musicManager.ChangeMusic(boss.bossMusic, true, forceRestart);
        }
        
        if (boss.preFightTasks != null && boss.preFightTasks.Count > 0)
        {
            StartCoroutine(RunPreFightDialogueThenStart());
        }
        else
        {
            currentPhaseCoroutine = StartCoroutine(RunPhase());
        }
    }

    private IEnumerator RunPreFightDialogueThenStart()
    {
        yield return Singleton.Instance.dialogueManager
            .DialogueTaskRoutine(boss.preFightTasks);

        // stop any existing just in case
        if (currentPhaseCoroutine != null) 
            StopCoroutine(currentPhaseCoroutine);

        // now start and capture the real phase
        currentPhaseCoroutine = StartCoroutine(RunPhase());
        yield break;
    }
    
    private IEnumerator RunPhase()
    {
        var phase = boss.phases[_currentPhaseIndex];
        Singleton.Instance.musicManager.SetMusicPhase(phase.musicPhase);
        var difficulty = Singleton.Instance.playerStats.currentDifficulty;

        var info = phase.difficultyInfos
            .FirstOrDefault(d => d.difficulty == difficulty);
        if (info == null)
        {
            Debug.LogError(
                $"[BossFightManager] Phase #{_currentPhaseIndex} missing health for difficulty {difficulty}"
            );
            yield break;
        }
        
        BossPhaseParams bpp = new BossPhaseParams();
        bpp.phaseNumber = _currentPhaseIndex;
        bossPhaseStartedEvent?.Invoke(bpp);

        double phaseHealth = info.totalHealth;
        gsm.roundGoal = phaseHealth;
        gsm.SetRoundGoal();
        gsm.ResetRoundScore();
        
        // ─── 1) Pre‐board tasks (unchanged) ───────────────────────────────────────
        yield return StartCoroutine(
            Singleton.Instance.dialogueManager
                     .DialogueTaskRoutine(phase.preBoardPopulateTasks)
        );

        // ─── 2) Populate board and wait for it (unchanged) ────────────────────
        bool boardPopulated = false;
        System.Action onPopulated = () => boardPopulated = true;
        GameStateMachine.BoardFinishedPopulatingAction += onPopulated;

        gsm.ChangeState(new GameStateMachine.PopulateBoardState());
        while (!boardPopulated)
            yield return null;
        GameStateMachine.BoardFinishedPopulatingAction -= onPopulated;

        // ─── 3) Post‐board tasks (unchanged) ──────────────────────────────────
        yield return StartCoroutine(
            Singleton.Instance.dialogueManager
                     .DialogueTaskRoutine(phase.postBoardPopulateTasks)
        );
        

        bool phaseBeat   = false;
        bool roundFailed = false;

        void OnPhaseBeat(double over) => phaseBeat   = (over >= 1);
        void OnRoundFailed()          => roundFailed = true;

        GameStateMachine.RoundGoalOverHitEvent += OnPhaseBeat;
        GameStateMachine.RoundFailedEvent    += OnRoundFailed;

        try
        {
            while (!phaseBeat)
            {
                if (roundFailed)
                {
                    gsm.stopTryButton.SetActive(false);
                    yield break; // jumps to finally, unsubscribing cleanly
                }
                yield return null;
            }
        }
        finally
        {
            gsm.KillAllBalls();
            gsm.ClearBoardOfGlobalObjects();
            gsm.stopTryButton.SetActive(false);
            GameStateMachine.RoundGoalOverHitEvent -= OnPhaseBeat;
            GameStateMachine.RoundFailedEvent    -= OnRoundFailed;
        }
        
        //IF LOOPING, THE FOLLOWING CODE WILL NOT EXECUTE
        
        //If gsm.ChangeState is here, then the BossBeatenExit event gets called before we can subscribe to it
        bool stateFinished = false;
        void OnScoringExited() => stateFinished = true;
        BossPhaseBeatStateExitedEvent += OnScoringExited;
        
        gsm.ChangeState(new BossPhaseBeatenState());
    
        bossFullBeatSFX?.Play();
    
        if (_currentPhaseIndex >= boss.phases.Count - 1)
        {
            if (!boss.bossAfterMusic.IsNull)
            {
                Singleton.Instance.musicManager.ChangeMusic(boss.bossAfterMusic);
            }
        }

        yield return StartCoroutine(
            Singleton.Instance.dialogueManager
                .DialogueTaskRoutine(phase.postPhaseBeatEarlyTasks)
        );
    
        while (!stateFinished) yield return null;
        BossPhaseBeatStateExitedEvent -= OnScoringExited;
    
        yield return StartCoroutine(
            Singleton.Instance.dialogueManager
                .DialogueTaskRoutine(phase.postPhaseBeatTasks)
        );
    
        yield return StartCoroutine(
            Singleton.Instance.dialogueManager
                .DialogueTaskRoutine(phase.postBounceStateExitedTasks)
        );
        
        // ─── 10) Advance to next phase or finish boss ─────────────────────────
        _currentPhaseIndex++;

        if (_currentPhaseIndex < boss.phases.Count)
        {
            yield return StartCoroutine(RunPhase());
        }
        else
        {
            yield return StartCoroutine(OnBossDefeated());
        }
    }
    
    public void AdvanceToNextBossPhase()
    {
        // tear down the old one
        if (currentPhaseCoroutine != null)
        {
            StopCoroutine(currentPhaseCoroutine);
            currentPhaseCoroutine = null;
        }

        _currentPhaseIndex++;
        currentPhaseCoroutine = StartCoroutine(RunPhase());
    }

    public class BossPhaseBeatenState : GameStateMachine.State
    {
        private BossFightManager bossFightManager;
        
        public override void EnterState()
        {
            BossPhaseBeatStateEnterEvent?.Invoke();
            bossFightManager = Singleton.Instance.bossFightManager;
            Singleton.Instance.bossFightManager.StartCoroutine(BossPhaseBeatenCoroutine());
        }

        public override void UpdateState()
        {
            
        }

        public override void ExitState()
        { 
           BossPhaseBeatStateExitedEvent?.Invoke();
        }

        IEnumerator BossPhaseBeatenCoroutine()
        {
            if (gameStateMachine == null)
            {
                Debug.Log("BossPhaseBeatenCoroutine: gameStateMachine was null");
                if (GameSingleton.Instance != null)
                {
                    gameStateMachine = GameSingleton.Instance.gameStateMachine;
                }

                else
                {
                    Debug.Log("BossPhaseBeatenCoroutine: GameSingleton was null");
                }
            }
            
            List<Cabbage> cabbages = new List<Cabbage>(gameStateMachine.activeCabbages);

            for (int i = 0; i < cabbages.Count; i++)
            {
                if (cabbages[i] == null)
                {
                    continue;
                }
                if (!cabbages[i].gameObject.activeInHierarchy)
                {
                    continue;
                }
                
                cabbages[i].PlayPopVFX();
                cabbages[i].PlayScoringSFX();

                if (bossFightManager.phaseBeatCabbagePopVFX != null)
                {
                    bossFightManager.phaseBeatCabbagePopVFX.Spawn(cabbages[i].transform.position);
                }

                bossFightManager.phaseBeatCabbagePopSFX.Play(cabbages[i].transform.position);
                
                cabbages[i].FullReset();
                cabbages[i].Remove();
                
                yield return new WaitForSeconds(0.1f);
            }
            
            //CLeanup any other cabbages
            if (GameSingleton.Instance != null)
            {
                Cabbage[] cs = GameSingleton.Instance.transform.parent.GetComponentsInChildren<Cabbage>();
                foreach (Cabbage cabbage in cs)
                {
                    cabbage.FullReset();
                    cabbage.Remove();
                }
            }
            
            yield return new WaitForSeconds(1f); //mostly to allow item stuff to get destroyed
            ExitState();
        }
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
    
    IEnumerator OnBossDefeated()
    {
        
        BossPhaseBeatStateExitedEvent?.Invoke();
        Physics2D.gravity = new Vector2(0f,-9.81f);
        GameStateMachine.ExitingScoringAction?.Invoke();
        isBossFight = false;

        yield return StartCoroutine(
            Singleton.Instance.dialogueManager
                .DialogueTaskRoutine(boss.postBeatTasks)
        );
        
        print("Ending Boss");
        Singleton.Instance.runManager.GoToMap();
    }
    
    public IEnumerator FinishPhaseRoutine(bool loopPhase)
    {
        if (currentPhaseCoroutine != null)
        {
            StopCoroutine(currentPhaseCoroutine);
            currentPhaseCoroutine = null;
        }
        
        GameSingleton.Instance.gameStateMachine.ClearBoard();
        GameSingleton.Instance.gameStateMachine.ChangeState(new GameStateMachine.StandbyState());
        yield return null;
        
        if (loopPhase)
        {
            // FULL clear here, since RunPhase's branch won't run
            currentPhaseCoroutine = StartCoroutine(RunPhase());
        }
        else
        {
            _currentPhaseIndex++;
            currentPhaseCoroutine = StartCoroutine(RunPhase());
        }

        yield break;
    }
}
