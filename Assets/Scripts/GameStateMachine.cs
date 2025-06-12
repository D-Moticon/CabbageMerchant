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
    private double pointsBankedFromWallPoppedCabbages = 0;
    public int minCabbagesBeforeNewSpawns = 10;
    
    [HideInInspector]public List<Ball> activeBalls = new List<Ball>();
    [HideInInspector]public List<Cabbage> activeCabbages = new List<Cabbage>();
    
    [HideInInspector] public List<BonkableSlot> bonkableSlots = new List<BonkableSlot>();

    public static Action PreBoardPopulateAction;
    public static Action BoardFinishedPopulatingAction;
    public static Action EnteringAimStateAction;
    public static Action ExitingAimStateAction;
    public static Action EnteringBounceStateAction;
    public static Action ExitingBounceStateAction;
    public static Action ExitingBounceStateEarlyAction;
    public static Action EnteringScoringAction;
    public static Action ExitingScoringAction;
    public static IntDelegate ExtraBallGainedAction;
    public static Action RoundFailedEvent;
    public static Action GameStateMachineStartedAction;

    public delegate void GSMDelegate(GameStateMachine gsm);
    public static GSMDelegate GSM_Enabled_Event;

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
    public GameObject harvestButton;

    [FormerlySerializedAs("keyYPos")] public float keyMaxYPos = -3.5f;

    float countdownTimerDuration = 0f;
    private float countdownTimer = 0f;
    [HideInInspector]public bool usingTimer = false;

    public GameObject floorObject;
    [HideInInspector]public bool forceEndRound = false;
    private bool forceRoundFail = false;
    [FormerlySerializedAs("roundEndMessage")] [HideInInspector]public string roundEndMessageOverride = "";
    
    private bool noRoundGoal = false;
    private ChaosCabbageSO chaosCabbageToAward;
    
    private void OnEnable()
    {
        Cabbage.CabbageMergedEvent += CabbageMergedListener;
        Ball.BallEnabledEvent += BallEnabledListener;
        Ball.BallDisabledEvent += BallDisabledListener;
        
        GSM_Enabled_Event?.Invoke(this);

        if (Singleton.Instance.buildManager.buildMode == BuildManager.BuildMode.release)
        {
            Debug.Log($"Entering Game State Machine.  Item List:");
            List<Item> items = Singleton.Instance.itemManager.GetItemsInInventory();
            foreach (var item in items)
            {
                Debug.Log(item.itemName);
                Debug.Log(item.GetDescriptionText());
                Debug.Log(item.GetTriggerText());
                Debug.Log("-----");
            }
        }
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
        GameStateMachineStartedAction?.Invoke();
        
        if (Singleton.Instance.bossFightManager.isBossFight)
        {
            Singleton.Instance.bossFightManager.gsm = this;
            Singleton.Instance.bossFightManager.StartBossFight();
            return;
        }
        
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

        if (currentRoundScore > roundGoal && !Singleton.Instance.bossFightManager.isBossFight)
        {
            harvestButton.SetActive(true);
        }

        else
        {
            harvestButton.SetActive(false);
        }
    }

    public void ChangeState(State newState)
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

    public void BankPoints(double pts)
    {
        pointsBankedFromWallPoppedCabbages += pts;
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
        if (quantity > 0)
        {
            ExtraBallGainedAction?.Invoke(quantity);
        }
        
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
        if (Singleton.Instance.bossFightManager.isBossFight)
        {
            // BossFightManager already set roundGoal to the current phase’s health.
            RoundGoalUpdatedEvent?.Invoke(roundGoal);
            return;
        }
        
        int mapLayer = Singleton.Instance.playerStats.currentMapLayer;
        roundGoal = Singleton.Instance.playerStats.currentDifficulty.GetRoundGoal(mapLayer);

        if (noRoundGoal)
        {
            roundGoal = 0;
        }
        
        RoundGoalUpdatedEvent?.Invoke(roundGoal);
    }

    public void SetNoRoundGoal()
    {
        noRoundGoal = true;
    }

    public void ResetRoundScore()
    {
        currentRoundScore = 0;
        currentRoundScoreOverMult = 0;
        roundScoreOverMultRounded = 0;
        pointsBankedFromWallPoppedCabbages = 0;
        RoundScoreUpdatedEvent?.Invoke(currentRoundScore);
    }
    
    public void UpdateRoundScore()
    {
        currentRoundScore = 0;
        for (int i = 0; i < activeCabbages.Count; i++)
        {
            currentRoundScore += activeCabbages[i].points;
        }

        currentRoundScore += pointsBankedFromWallPoppedCabbages;
        
        currentRoundScore = Math.Ceiling(currentRoundScore);
        currentRoundScoreOverMult = currentRoundScore / roundGoal;
        if (roundGoal < 1 || double.IsNaN(currentRoundScoreOverMult))
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

    public void Harvest()
    {
        if (Singleton.Instance.bossFightManager.isBossFight)
        {
            return;
        }
        if (currentRoundScore > roundGoal)
        {
            KillAllBalls();
            ChangeState(new ScoringState());
        }
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

        if (activeCabbages.Count < minCabbagesBeforeNewSpawns)
        {
            BonkableSlot bs = GetEmptyBonkableSlot(true, 1.5f);
            if (bs != null)
            {
                Cabbage c = SpawnCabbageInSlot(bs);
            }
        }
    }

    public void MultiplyRoundScore(double multiple)
    {
        double bankScore = currentRoundScore * multiple - currentRoundScore;
        pointsBankedFromWallPoppedCabbages += bankScore;
        UpdateRoundScore();
    }
    
    public void CollectChaosCabbage(ChaosCabbageSO ccso)
    {
        chaosCabbageToAward = ccso;
        ChaosCabbageCollectState newState = new ChaosCabbageCollectState();
        ChangeState(newState);
    }
    
    
    //-------------------------------STATES--------------------------------------
    
    
    
    public class PopulateBoardState : State
    {
        public override void EnterState()
        {
            gameStateMachine.currentBalls = Singleton.Instance.playerStats.maxBalls;
            gameStateMachine.StartCoroutine(PopulateBoard());
            BallsRemainingUpdatedEvent?.Invoke(gameStateMachine.currentBalls);
            gameStateMachine.stopTryButton.SetActive(false);
        }

        public override void UpdateState()
        {
            gameStateMachine.UpdateRoundScore();
        }

        public override void ExitState()
        {
            BoardFinishedPopulatingAction?.Invoke();
        }

        IEnumerator PopulateBoard()
        {
            PreBoardPopulateAction?.Invoke();
            
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
                yield return null; //wait a frame for lists to settle
                gameStateMachine.bonkableSlots.AddRange(bss.bonkableSlots);
            }

            gameStateMachine.bonkableSlots.RemoveAll(x => x == null);
            List<BonkableSlot> slotsToPopulate = Helpers.GetUniqueRandomEntries(gameStateMachine.bonkableSlots, numPegs);
            for (int i = 0; i < slotsToPopulate.Count; i++)
            {
                if (slotsToPopulate[i] == null)
                {
                    Debug.LogError($"[PopulateBoard] BonkableSlotSpawner produced a null slot in its bonkableSlots list.");
                    continue;
                }
                
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

    public IEnumerator PopulateBoardWith(BoardPopulateInfo bpi)
    {
        yield break;
    }
    
    public void ClearBoardOfGlobalObjects()
    {
        Singleton.Instance.objectPoolManager.DespawnAll(Singleton.Instance.itemManager.keyPooledObject);
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
            
            gameStateMachine.UpdateRoundScore();
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

                ExitingBounceStateEarlyAction?.Invoke();
                
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
            
            gameStateMachine.UpdateRoundScore();
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
            float elapsedTime = 0f;
            while (elapsedTime < gameStateMachine.secondsBeforeStopTryButton)
            {
                if (Singleton.Instance.pauseManager.isPaused)
                {
                    yield return null;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            gameStateMachine.stopTryButton.SetActive(true);
        }
    }
    
    public class ScoringState : State
    {
        public override void EnterState()
        {
            gameStateMachine.ClearBoardOfGlobalObjects();
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
                    RoundFailedEvent?.Invoke();
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

    public class ChaosCabbageCollectState : State
    {
        public override void EnterState()
        {
            EnteringScoringAction?.Invoke();
            gameStateMachine.StartCoroutine(ChaosCabbageGetRoutine());
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

        IEnumerator ChaosCabbageGetRoutine()
        {
            gameStateMachine.KillAllBalls();
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
            
            Task t = new Task(Singleton.Instance.chaosManager.CollectChaosCabbageTask(gameStateMachine.chaosCabbageToAward));
            while (t.Running)
            {
                yield return null;
            }
            ExitState();
        }
    }
}
