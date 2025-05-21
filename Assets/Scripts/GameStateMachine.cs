using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameStateMachine : MonoBehaviour
{
    public abstract class State
    {
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();

        public GameStateMachine gameStateMachine;
        public PlayerInputManager playerInputManager;
        
        protected State()
        {
            gameStateMachine = GameSingleton.Instance.gameStateMachine;
            playerInputManager = Singleton.Instance.playerInputManager;
        }
    }

    public State currentState;
    public int numberPegs;
    public Launcher launcher;
    
    //Game Rules
    [HideInInspector]public int currentBalls = 3;
    [HideInInspector]public double roundGoal = 0;
    [HideInInspector] public double currentRoundScore;
    [HideInInspector] public double currentRoundScoreOverMult = 0;
    [HideInInspector] public double roundScoreOverMultRounded = 0;
    public float secondsBeforeStopTryButton = 15f;
    public float roundGoalMultBeforeStopTryButton = 50f;
    public static int coinsPerRoundGoal = 3;
    
    [HideInInspector]public List<Ball> activeBalls = new List<Ball>();
    [HideInInspector]public List<Cabbage> activeCabbages = new List<Cabbage>();
    
    [HideInInspector] public List<BonkableSlot> bonkableSlots = new List<BonkableSlot>();
    
    public static Action BoardFinishedPopulatingAction;
    public static Action EnteringAimStateAction;
    public static Action ExitingAimStateAction;
    public static Action EnteringBounceStateAction;
    public static Action ExitingBounceStateAction;
    public static Action EnteringScoringAction;
    public static Action ExitingScoringAction;

    public delegate void IntDelegate(int ballsRemaining);
    public static IntDelegate BallsRemainingUpdatedEvent;

    public delegate void BallDelegate(Ball b);
    public static event BallDelegate BallFiredEvent;
    public static event BallDelegate BallFiredEarlyEvent; //fired before BallFired for things that need it

    public delegate void DoubleDelegate(double value);

    public static DoubleDelegate RoundGoalUpdatedEvent;
    public static DoubleDelegate RoundScoreUpdatedEvent;
    public static DoubleDelegate RoundGoalOverHitEvent;

    public delegate void FloatDelegate(float value);
    public static FloatDelegate TimerUpdatedEvent;

    public GameObject stopTryButton;
    private bool stopTry = false;

    [FormerlySerializedAs("keyYPos")] public float keyMaxYPos = -3.5f;

    float countdownTimerDuration = 0f;
    private float countdownTimer = 0f;
    [HideInInspector]public bool usingTimer = false;

    public GameObject floorObject;
    [HideInInspector]public bool forceEndRound = false;
    private bool forceRoundFail = false;
    [FormerlySerializedAs("roundEndMessage")] [HideInInspector]public string roundEndMessageOverride = "";
    
    private void OnEnable()
    {
        Cabbage.CabbageMergedEvent += CabbageMergedListener;
        Ball.BallEnabledEvent += BallEnabledListener;
        Ball.BallDisabledEvent += BallDisabledListener;
    }

    private void OnDisable()
    {
        Cabbage.CabbageMergedEvent -= CabbageMergedListener;
        Ball.BallEnabledEvent -= BallEnabledListener;
        Ball.BallDisabledEvent -= BallDisabledListener;
        Physics2D.gravity = new Vector2(0f,-9.81f);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        State startState = new PopulateBoardState();
        ChangeState(startState);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState();
        }
        
        UpdateRoundScore();
    }

    void ChangeState(State newState)
    {
        if (currentState != null)
        {
            currentState.ExitState();
        }
        
        currentState = newState;
        newState.EnterState();
    }

    void BallEnabledListener(Ball b)
    {
        AddActiveBall(b);
    }

    void BallDisabledListener(Ball b)
    {
        RemoveActiveBall(b);
    }
    
    public void AddActiveBall(Ball b)
    {
        if(!activeBalls.Contains(b))
        {
            activeBalls.Add(b);
        }
    }

    public void RemoveActiveBall(Ball b)
    {
        if(activeBalls.Contains(b))
        {
            activeBalls.Remove(b);
        }
    }
    
    public void AddActiveCabbage(Cabbage c)
    {
        if(!activeCabbages.Contains(c))
        {
            activeCabbages.Add(c);
        }
    }

    public void RemoveActiveCabbage(Cabbage c)
    {
        if(activeCabbages.Contains(c))
        {
            activeCabbages.Remove(c);
        }
    }

    public int GetNumberActiveCabbages()
    {
        return activeCabbages.Count;
    }

    public void ForceEndRound()
    {
        forceEndRound = true;
    }
    
    public BonkableSlot GetEmptyBonkableSlot(bool ensureNotOverlappingCabbage = true, float checkRadius = 0.5f)
    {
        List<BonkableSlot> validSlots = 
            (from bs in bonkableSlots
                where bs.bonkable == null
                select bs).ToList();

        if (ensureNotOverlappingCabbage)
        {
            validSlots = validSlots.Where(slot =>
            {
                Collider2D[] overlaps = Physics2D.OverlapCircleAll(slot.transform.position, checkRadius);
                foreach (var col in overlaps)
                {
                    if (col.GetComponent<Cabbage>() != null)
                    {
                        return false; // This slot overlaps a cabbage
                    }
                }
                return true;
            }).ToList();
        }

        if (validSlots.Count == 0)
        {
            return null;
        }

        return validSlots[Random.Range(0, validSlots.Count)];
    }


    public void SetTimerMode(bool useTimer, float duration = 10f)
    {
        usingTimer = useTimer;
        countdownTimerDuration = duration;
    }

    public void TurnOnFloor()
    {
        floorObject.SetActive(true);
    }
    
    public void TurnOffFloor()
    {
        floorObject.SetActive(false);
    }

    public void AddExtraBall(int quantity)
    {
        currentBalls += quantity;
        BallsRemainingUpdatedEvent?.Invoke(currentBalls);
    }

    public void KillAllBalls()
    {
        for (int i = activeBalls.Count - 1; i >= 0; i--)
        {
            activeBalls[i].KillBall();
        }
    }

    public void UpdateActiveCabbages()
    {
        for (int i = activeCabbages.Count - 1; i >= 0; i--)
        {
            if (!activeCabbages[i].gameObject.activeSelf)
            {
                activeCabbages.RemoveAt(i);
            }
        }
    }

    public void ForceRoundFail(string failMessage)
    {
        forceRoundFail = true;
        roundEndMessageOverride = failMessage;
    }
    
    public class PopulateBoardState : State
    {
        public override void EnterState()
        {
            gameStateMachine.currentBalls = Singleton.Instance.playerStats.currentBalls;
            gameStateMachine.StartCoroutine(PopulateBoard());
            BallsRemainingUpdatedEvent?.Invoke(gameStateMachine.currentBalls);
            gameStateMachine.stopTryButton.SetActive(false);
        }

        public override void UpdateState()
        {
            
        }

        public override void ExitState()
        {
            BoardFinishedPopulatingAction?.Invoke();
        }

        IEnumerator PopulateBoard()
        {
            gameStateMachine.ResetRoundScore();
            gameStateMachine.SetRoundGoal();

            if (GameSingleton.Instance.currentBiomeParent == null)
            {
                GameSingleton.Instance.SetBiome(GameSingleton.Instance.biomeInfos[0].biome);
            }
            
            BiomeParent.BoardVariantInfo boardVariantInfo = GameSingleton.Instance.currentBiomeParent.GetBoardVariant();
            GameObject boardVariant = boardVariantInfo.boardVariant;
            
            yield return new WaitForSeconds(.75f);
            
            int numPegs = gameStateMachine.numberPegs + Singleton.Instance.playerStats.extraStartingCabbages;
            
            gameStateMachine.bonkableSlots.Clear();
            gameStateMachine.activeCabbages.Clear();

            BonkableSlotSpawner[] bonkableSlotSpawners = boardVariant.GetComponentsInChildren<BonkableSlotSpawner>();
            
            if (GameSingleton.Instance.currentBiomeParent.spawnCabbagesInAllSlots)
            {
                int totalSlots = 0;
                foreach (var bss in bonkableSlotSpawners)
                {
                    totalSlots += bss.GetTotalNumberStartingSlots();
                }

                numPegs = totalSlots;
            }
            
            foreach (BonkableSlotSpawner bss in bonkableSlotSpawners)
            {
                bss.SpawnBonkableSlots();
                gameStateMachine.bonkableSlots.AddRange(bss.bonkableSlots);
            }

            List<BonkableSlot> slotsToPopulate = Helpers.GetUniqueRandomEntries(gameStateMachine.bonkableSlots, numPegs);
            for (int i = 0; i < slotsToPopulate.Count; i++)
            {
                gameStateMachine.SpawnCabbageInSlot(slotsToPopulate[i]);
                yield return new WaitForSeconds(0.05f);
            }
            
            
            if (Random.value < Singleton.Instance.playerStats.keyChance)
            {
                // find any slots *not* holding a cabbage
                var emptySlots = gameStateMachine.bonkableSlots
                    .Except(slotsToPopulate)
                    .Where(x => x.transform.position.y <= gameStateMachine.keyMaxYPos)
                    .ToList();
        
                if (emptySlots.Count > 0)
                {
                    // pick one at random
                    var keySlot = emptySlots[Random.Range(0, emptySlots.Count)];
            
                    // build a spawn position at the slot's X, but at keyYPos
                    Vector3 keyPos = keySlot.transform.position;
            
                    // spawn from your item‐manager's pool
                    Singleton.Instance.itemManager
                        .keyPooledObject
                        .Spawn(keyPos, Quaternion.identity);
                }
            }

            AwardChaosCabbage acc = boardVariant.GetComponentInChildren<AwardChaosCabbage>();
            if (acc != null)
            {
                if (boardVariantInfo.chaosCabbage != null)
                {
                    acc.SetChaosCabbage(boardVariantInfo.chaosCabbage);
                }
            }
            
            State newState = new AimingState();
            gameStateMachine.ChangeState(newState);
        }
    }

    public Cabbage SpawnCabbageInSlot(BonkableSlot bs)
    {
        PooledObjectData cabbagePrefab = GameSingleton.Instance.currentBiomeParent.cabbagePooledObject;
        
        BonkableSlotSpawner bss = bs.GetComponentInParent<BonkableSlotSpawner>();
        if (bss != null)
        {
            if (bss.cabbageOverride != null)
            {
                cabbagePrefab = bss.cabbageOverride;
            }
        }
        
        Cabbage c = cabbagePrefab.Spawn(bs.transform.position, Quaternion.identity).GetComponent<Cabbage>();
        c.transform.parent = bs.transform;
        bs.bonkable = c;
        c.bonkFeel.PlayFeedbacks();

        float goldRand = Random.Range(0f, 1f);
        if (goldRand < Singleton.Instance.playerStats.goldenCabbageChance)
        {
            c.SetVariant(CabbageVariantType.golden);
        }
                
        activeCabbages.Add(c);
        return c;
    }

    public void SetRoundGoal()
    {
        int mapLayer = Singleton.Instance.playerStats.currentMapLayer;
        roundGoal = Singleton.Instance.playerStats.currentDifficulty.GetRoundGoal(mapLayer);

        RoundGoalUpdatedEvent?.Invoke(roundGoal);
    }


    public void ResetRoundScore()
    {
        currentRoundScore = 0;
        currentRoundScoreOverMult = 0;
        roundScoreOverMultRounded = 0;
        RoundScoreUpdatedEvent?.Invoke(currentRoundScore);
    }
    
    public void UpdateRoundScore()
    {
        currentRoundScore = 0;
        for (int i = 0; i < activeCabbages.Count; i++)
        {
            currentRoundScore += activeCabbages[i].points;
        }

        currentRoundScore = Math.Ceiling(currentRoundScore);
        currentRoundScoreOverMult = currentRoundScore / roundGoal;
        if (double.IsNaN(currentRoundScoreOverMult))
        {
            currentRoundScoreOverMult = 0;
        }

        double newOverRounded = Math.Floor(currentRoundScoreOverMult);
        
        if (newOverRounded > roundScoreOverMultRounded)
        {
            roundScoreOverMultRounded = Math.Floor(currentRoundScoreOverMult);
            RoundGoalHit();
        }
        
        roundScoreOverMultRounded = Math.Floor(currentRoundScoreOverMult);
        RoundScoreUpdatedEvent?.Invoke(currentRoundScore);
    }

    void RoundGoalHit()
    {
        //Singleton.Instance.playerStats.AddCoins(coinsPerRoundGoal);
        RoundGoalOverHitEvent?.Invoke(roundScoreOverMultRounded);
    }

    public Ball GetRandomActiveBall()
    {
        if (activeBalls.Count == 0)
        {
            return null;
        }
        int rand = Random.Range(0, activeBalls.Count);
        if (activeBalls[rand] != null)
        {
            return activeBalls[rand];
        }
        else
        {
            return null;
        }
    }
    
    public Cabbage GetRandomActiveCabbage()
    {
        if (activeCabbages.Count == 0)
        {
            return null;
        }
        int rand = Random.Range(0, activeCabbages.Count);
        if (activeCabbages[rand] != null)
        {
            return activeCabbages[rand];
        }
        else
        {
            return null;
        }
    }

    public void StopTry()
    {
        stopTry = true;
    }
    
    void CabbageMergedListener(Cabbage.CabbageMergedParams cmp)
    {
        for(int i = 0; i < bonkableSlots.Count; i++)
        {
            if (bonkableSlots[i].bonkable as Object == cmp.oldCabbageA as Object || bonkableSlots[i].bonkable as Object == cmp.oldCabbageB as Object)
            {
                bonkableSlots[i].bonkable = null;
            }
        }
    }
    
    public class AimingState : State
    {
        public override void EnterState()
        {
            GameStateMachine.EnteringAimStateAction?.Invoke();

            if (gameStateMachine.currentBalls == 1)
            {
                Singleton.Instance.uiManager.ShowNotification("Last Ball!");
            }
            
            gameStateMachine.stopTryButton.SetActive(false);
        }

        public override void UpdateState()
        {
            if (PauseManager.IsPaused())
            {
                return;
            }
            if (playerInputManager.fireDown)
            {
                Vector2 mousePos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
                Collider2D hit = Physics2D.OverlapPoint(mousePos);
                if (hit != null && hit.GetComponentInParent<ItemSlot>() != null)
                {
                    // we’re over an item slot → do not fire
                    return;
                }
                
                Ball b = gameStateMachine.launcher.LaunchBall();
                gameStateMachine.currentBalls--;
                State newState = new BouncingState();
                gameStateMachine.ChangeState(newState);
                BallsRemainingUpdatedEvent?.Invoke(gameStateMachine.currentBalls);
                BallFiredEarlyEvent?.Invoke(b);
                BallFiredEvent?.Invoke(b);
            }
        }

        public override void ExitState()
        {
            GameStateMachine.ExitingAimStateAction?.Invoke();
        }
    }

    public class BouncingState : State
    {
        private Coroutine stopTryCoroutine;
        
        public override void EnterState()
        {
            stopTryCoroutine = gameStateMachine.StartCoroutine(StopTryButtonShowRoutine());
            EnteringBounceStateAction?.Invoke();
            
            //This needs to be after the event for any items that start the timer
            if (gameStateMachine.usingTimer)
            {
                gameStateMachine.countdownTimer = gameStateMachine.countdownTimerDuration;
            }
        }

        public override void UpdateState()
        {
            gameStateMachine.UpdateActiveCabbages();
            
            if (gameStateMachine.GetNumberActiveCabbages() <= 0)
            {
                State newState = new ScoringState();
                gameStateMachine.ChangeState(newState);
            }

            if (gameStateMachine.currentRoundScoreOverMult > gameStateMachine.roundGoalMultBeforeStopTryButton || gameStateMachine.GetNumberActiveCabbages() <= 1)
            {
                gameStateMachine.stopTryButton.gameObject.SetActive(true);
            }

            if (gameStateMachine.stopTry)
            {
                gameStateMachine.KillAllBalls();

                gameStateMachine.stopTry = false;
            }

            if (gameStateMachine.usingTimer)
            {
                gameStateMachine.countdownTimer -= Time.deltaTime;
                TimerUpdatedEvent?.Invoke(gameStateMachine.countdownTimer);
                if (gameStateMachine.countdownTimer <= 0f)
                {
                    gameStateMachine.KillAllBalls();
                }
            }
            
            if (gameStateMachine.activeBalls.Count <= 0)
            {
                State newState = new AimingState();

                if (gameStateMachine.currentBalls <= 0)
                {
                    newState = new ScoringState();
                }
                
                gameStateMachine.ChangeState(newState);
            }

            if (gameStateMachine.forceEndRound)
            {
                gameStateMachine.KillAllBalls();
                State newState = new ScoringState();
                gameStateMachine.ChangeState(newState);
                gameStateMachine.forceEndRound = false;
            }
        }

        public override void ExitState()
        {
            if (stopTryCoroutine != null)
            {
                gameStateMachine.StopCoroutine(stopTryCoroutine);
            }

            ExitingBounceStateAction?.Invoke();
        }

        IEnumerator StopTryButtonShowRoutine()
        {
            yield return new WaitForSeconds(gameStateMachine.secondsBeforeStopTryButton);
            gameStateMachine.stopTryButton.SetActive(true);
        }
    }

    public class ScoringState : State
    {
        public override void EnterState()
        {
            Singleton.Instance.objectPoolManager.DespawnAll(Singleton.Instance.itemManager.keyPooledObject);
            gameStateMachine.stopTryButton.SetActive(false);
            EnteringScoringAction?.Invoke();
            gameStateMachine.StartCoroutine(ScoringRoutine());
        }

        public override void UpdateState()
        {
            
        }

        public override void ExitState()
        {
            Physics2D.gravity = new Vector2(0f,-9.81f);
            ExitingScoringAction?.Invoke();
            Singleton.Instance.runManager.GoToMap();
        }

        IEnumerator ScoringRoutine()
        {
            yield return new WaitForSeconds(1f);
            
            if (gameStateMachine.currentRoundScore < gameStateMachine.roundGoal || gameStateMachine.forceRoundFail)
            {
                if (!String.IsNullOrEmpty(gameStateMachine.roundEndMessageOverride))
                {
                    Singleton.Instance.uiManager.ShowNotification(gameStateMachine.roundEndMessageOverride);
                }

                else
                {
                    Singleton.Instance.uiManager.ShowNotification("<color=red>Round Goal Missed</color>");
                }
                
                yield return new WaitForSeconds(1.5f);
                Singleton.Instance.playerStats.RemoveLife();
                yield return new WaitForSeconds(1.5f);
                if (Singleton.Instance.playerStats.lives > 0)
                {
                    Singleton.Instance.runManager.ReloadCurrentScene();
                    yield break;
                }
                else
                {
                    Singleton.Instance.runManager.FinishRun(false);
                    yield break;
                }
            }

            else
            {
                List<Cabbage> cabbages = gameStateMachine.activeCabbages;

                for (int i = 0; i < cabbages.Count; i++)
                {
                    if (!cabbages[i].gameObject.activeInHierarchy)
                    {
                        continue;
                    }
                
                    cabbages[i].PlayPopVFX();
                    cabbages[i].PlayScoringSFX();
                
                    Color col = Color.white;
                    Singleton.Instance.floaterManager.SpawnFloater(
                        cabbages[i].scoreFloater,
                        Helpers.FormatWithSuffix(cabbages[i].points),
                        cabbages[i].transform.position,
                        col,
                        cabbages[i].transform.localScale.x);
                    //cabbages[i].gameObject.SetActive(false);
                    cabbages[i].Harvest();
                    yield return new WaitForSeconds(0.05f);
                
                }

                //double coinsEarned = Math.Ceiling(gameStateMachine.currentRoundScore / gameStateMachine.roundGoal)*coinsPerRoundGoal;
                double coinsEarned = Math.Round(gameStateMachine.roundScoreOverMultRounded * coinsPerRoundGoal);
                Singleton.Instance.playerStats.AddCoins(coinsEarned);
                Singleton.Instance.uiManager.DisplayCoinsGainedAnimation(coinsEarned);
                
                yield return new WaitForSeconds(1f);
            }
            
            ExitState();
        }
    }
}
