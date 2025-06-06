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
    private int _currentPhaseIndex = 0;
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

    public PooledObjectData phaseBeatCabbagePopVFX;
    public SFXInfo phaseBeatCabbagePopSFX;

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
        

        bool phaseBeat = false;
        void OnPhaseBeat(double over) => phaseBeat = (over >= 1);
        GameStateMachine.RoundGoalOverHitEvent += OnPhaseBeat;

        while (!phaseBeat)
            yield return null;
        
        GameStateMachine.RoundGoalOverHitEvent -= OnPhaseBeat;
        
        gsm.KillAllBalls();
        gsm.ClearBoardOfGlobalObjects();
        gsm.stopTryButton.SetActive(false);

        gsm.ChangeState(new BossPhaseBeatenState());
        bool stateFinished = false;
        void OnScoringExited() => stateFinished = true;
        BossPhaseBeatStateExitedEvent += OnScoringExited;
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
        // ──────────────────────────────────────────────────────────────────────┘

        // ─── 10) Advance to next phase or finish boss ─────────────────────────
        _currentPhaseIndex++;
        if (_currentPhaseIndex < boss.phases.Count)
        {
            yield return StartCoroutine(RunPhase());
        }
        else
        {
            OnBossDefeated();
        }
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
            List<Cabbage> cabbages = new List<Cabbage>(gameStateMachine.activeCabbages);

            for (int i = 0; i < cabbages.Count; i++)
            {
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
                
                yield return new WaitForSeconds(0.05f);
            }
            
            //CLeanup any other cabbages
            Cabbage[] cs = GameSingleton.Instance.GetComponentsInChildren<Cabbage>();
            foreach (Cabbage cabbage in cs)
            {
                if (!cabbage.gameObject.activeInHierarchy)
                {
                    continue;
                }
                
                cabbage.Remove();
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
    
    void OnBossDefeated()
    {
        BossPhaseBeatStateExitedEvent?.Invoke();
        Physics2D.gravity = new Vector2(0f,-9.81f);
        GameStateMachine.ExitingScoringAction?.Invoke();
        isBossFight = false;
        Singleton.Instance.runManager.GoToMap();
    }
}
