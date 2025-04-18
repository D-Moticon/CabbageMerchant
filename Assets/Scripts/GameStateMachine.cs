using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Linq;

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
    public PooledObjectData cabbagePooledObject;
    public BoardMetrics boardMetrics;
    public Launcher launcher;
    
    //Game Rules
    [HideInInspector]public int currentBalls = 3;
    [HideInInspector]public double roundGoal = 0;
    [HideInInspector] public double currentRoundScore;
    [HideInInspector] public double currentRoundScoreOverMult = 0;
    [HideInInspector] public double roundScoreOverMultRounded = 0;
    public static int coinsPerRoundGoal = 3;
    
    [HideInInspector]public List<Ball> activeBalls = new List<Ball>();
    [HideInInspector]public List<Cabbage> activeCabbages = new List<Cabbage>();

    public class CabbageSlot
    {
        public Vector2 position;
        public Cabbage c;
    }

    [HideInInspector] public List<CabbageSlot> cabbageSlots = new List<CabbageSlot>();
    
    public static Action BoardFinishedPopulatingAction;
    public static Action EnteringAimStateAction;
    public static Action ExitingAimStateAction;
    public static Action ExitingBounceStateAction;
    public static Action ExitingScoringAction;

    public delegate void IntDelegate(int ballsRemaining);
    public static IntDelegate BallsRemainingUpdatedEvent;

    public delegate void BallDelegate(Ball b);
    public static event BallDelegate BallFiredEvent;

    public delegate void DoubleDelegate(double value);

    public static DoubleDelegate RoundGoalUpdatedEvent;
    public static DoubleDelegate RoundScoreUpdatedEvent;
    public static DoubleDelegate RoundGoalOverHitEvent;

    private void OnEnable()
    {
        Cabbage.CabbageMergedEvent += CabbageMergedListener;
    }

    private void OnDisable()
    {
        Cabbage.CabbageMergedEvent -= CabbageMergedListener;
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

    public CabbageSlot GetEmptyCabbageSlot(bool ensureNotOverlappingCabbage = true, float checkRadius = 0.5f)
    {
        List<CabbageSlot> validSlots = 
            (from cs in cabbageSlots
                where cs.c == null
                select cs).ToList();

        if (ensureNotOverlappingCabbage)
        {
            validSlots = validSlots.Where(slot =>
            {
                Collider2D[] overlaps = Physics2D.OverlapCircleAll(slot.position, checkRadius);
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
    
    public class PopulateBoardState : State
    {
        public override void EnterState()
        {
            gameStateMachine.currentBalls = Singleton.Instance.playerStats.currentBalls;
            gameStateMachine.StartCoroutine(PopulateBoard());
            BallsRemainingUpdatedEvent?.Invoke(gameStateMachine.currentBalls);
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
            
            yield return new WaitForSeconds(.75f);
            
            int numPegs = gameStateMachine.numberPegs + Singleton.Instance.playerStats.extraStartingCabbages;
            
            gameStateMachine.cabbageSlots.Clear();
            gameStateMachine.activeCabbages.Clear();
            
            List<Vector2> gridPoints = GameSingleton.Instance.boardMetrics.GetAllGridPoints();
            for (int i = 0; i < gridPoints.Count; i++)
            {
                CabbageSlot cs = new CabbageSlot();
                cs.position = gridPoints[i];
                gameStateMachine.cabbageSlots.Add(cs);
            }

            List<CabbageSlot> slotsToPopulate = Helpers.GetUniqueRandomEntries(gameStateMachine.cabbageSlots, numPegs);
            for (int i = 0; i < slotsToPopulate.Count; i++)
            {
                gameStateMachine.SpawnCabbageInSlot(slotsToPopulate[i]);
                yield return new WaitForSeconds(0.05f);
            }
            
            Vector2[] positions = GameSingleton.Instance.boardMetrics.GetRandomGridPoints(numPegs);
 
            
            State newState = new AimingState();
            gameStateMachine.ChangeState(newState);
        }
    }

    public Cabbage SpawnCabbageInSlot(CabbageSlot cs)
    {
        Cabbage c = cabbagePooledObject.Spawn(cs.position, Quaternion.identity).GetComponent<Cabbage>();
        cs.c = c;
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
        if (MapSingleton.Instance == null)
        {
            //return;
        }
        
        int mapLayer = MapSingleton.Instance.mapManager.currentLayerIndex;
        double firstRoundGoal = MapSingleton.Instance.mapManager.currentMapBlueprint.firstRoundGoal;
        float goalBase = MapSingleton.Instance.mapManager.currentMapBlueprint.goalBase;
        float goalPower = MapSingleton.Instance.mapManager.currentMapBlueprint.goalPower;

        roundGoal = firstRoundGoal + goalBase * Mathf.Pow(mapLayer, goalPower);
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
        Singleton.Instance.playerStats.AddCoins(coinsPerRoundGoal);
        RoundGoalOverHitEvent?.Invoke(roundScoreOverMultRounded);
    }

    void CabbageMergedListener(Cabbage.CabbageMergedParams cmp)
    {
        for(int i = 0; i < cabbageSlots.Count; i++)
        {
            if (cabbageSlots[i].c == cmp.oldCabbageA || cabbageSlots[i].c == cmp.oldCabbageB)
            {
                cabbageSlots[i].c = null;
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
        }

        public override void UpdateState()
        {
            if (playerInputManager.fireDown)
            {
                Ball b = gameStateMachine.launcher.LaunchBall();
                gameStateMachine.currentBalls--;
                State newState = new BouncingState();
                gameStateMachine.ChangeState(newState);
                BallsRemainingUpdatedEvent?.Invoke(gameStateMachine.currentBalls);
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
        public override void EnterState()
        {
            
        }

        public override void UpdateState()
        {
            if (gameStateMachine.activeBalls.Count <= 0)
            {
                State newState = new AimingState();

                if (gameStateMachine.currentBalls <= 0)
                {
                    newState = new ScoringState();
                }
                
                gameStateMachine.ChangeState(newState);
            }
        }

        public override void ExitState()
        {
            ExitingBounceStateAction?.Invoke();
        }
    }

    public class ScoringState : State
    {
        public override void EnterState()
        {
            gameStateMachine.StartCoroutine(ScoringRoutine());
        }

        public override void UpdateState()
        {
            
        }

        public override void ExitState()
        {
            ExitingScoringAction?.Invoke();
            Singleton.Instance.runManager.GoToMap();
        }

        IEnumerator ScoringRoutine()
        {
            yield return new WaitForSeconds(1f);
            
            
            if (gameStateMachine.currentRoundScore < gameStateMachine.roundGoal)
            {
                Singleton.Instance.uiManager.ShowNotification("<color=red>Round Goal Missed</color>");
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
                    Singleton.Instance.runManager.StartNewRun();
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
                    cabbages[i].gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.05f);
                
                }

                double coinsToGive = Math.Ceiling(gameStateMachine.currentRoundScore / gameStateMachine.roundGoal)*coinsPerRoundGoal;
                //Singleton.Instance.playerStats.AddCoins(coinsToGive);
                
                yield return new WaitForSeconds(1f);
            }
            
            ExitState();
        }
    }
}
