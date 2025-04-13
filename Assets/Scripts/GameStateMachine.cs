using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

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
    [FormerlySerializedAs("pegPooledObject")] public PooledObjectData cabbagePooledObject;
    public BoardMetrics boardMetrics;
    public Launcher launcher;
    
    //Game Rules
    [HideInInspector]public int currentBalls = 3;
    [HideInInspector]public double roundGoal = 0;
    [HideInInspector] public double currentRoundScore;
    
    [HideInInspector]public List<Ball> activeBalls = new List<Ball>();
    [HideInInspector]public List<Cabbage> activeCabbages = new List<Cabbage>();

    public static Action BoardFinishedPopulatingAction;
    public static Action EnteringAimStateAction;
    public static Action ExitingAimStateAction;

    public delegate void BallFiredDelegate(int ballsRemaining);
    public static BallFiredDelegate BallsRemainingEvent;

    public delegate void DoubleDelegate(double value);

    public static DoubleDelegate RoundGoalUpdatedEvent;
    public static DoubleDelegate RoundScoreUpdatedEvent;
    
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
    
    public class PopulateBoardState : State
    {
        public override void EnterState()
        {
            gameStateMachine.currentBalls = Singleton.Instance.runManager.currentBalls;
            gameStateMachine.StartCoroutine(PopulateBoard());
            BallsRemainingEvent?.Invoke(gameStateMachine.currentBalls);
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
            yield return new WaitForSeconds(.75f);
            
            int numPegs = gameStateMachine.numberPegs + Singleton.Instance.runManager.extraStartingCabbages;
            Vector2[] positions = GameSingleton.Instance.boardMetrics.GetRandomGridPoints(numPegs);
            
            gameStateMachine.activeCabbages.Clear();
            
            for (int i = 0; i < numPegs; i++)
            {
                Cabbage c = gameStateMachine.cabbagePooledObject.Spawn(positions[i], Quaternion.identity).GetComponent<Cabbage>();
                c.bonkFeel.PlayFeedbacks();
                gameStateMachine.activeCabbages.Add(c);
                yield return new WaitForSeconds(0.05f);
            }

            gameStateMachine.ResetRoundScore();
            gameStateMachine.SetRoundGoal();
            State newState = new AimingState();
            gameStateMachine.ChangeState(newState);
        }
    }

    public void SetRoundGoal()
    {
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
        RoundScoreUpdatedEvent?.Invoke(currentRoundScore);
    }
    
    public void UpdateRoundScore()
    {
        currentRoundScore = 0;
        for (int i = 0; i < activeCabbages.Count; i++)
        {
            currentRoundScore += activeCabbages[i].points;
        }
        RoundScoreUpdatedEvent?.Invoke(currentRoundScore);
    }
    
    public class AimingState : State
    {
        public override void EnterState()
        {
            GameStateMachine.EnteringAimStateAction?.Invoke();
        }

        public override void UpdateState()
        {
            if (playerInputManager.fireDown)
            {
                gameStateMachine.launcher.LaunchBall();
                gameStateMachine.currentBalls--;
                State newState = new BouncingState();
                gameStateMachine.ChangeState(newState);
                BallsRemainingEvent?.Invoke(gameStateMachine.currentBalls);
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
            Singleton.Instance.runManager.GoToMap();
        }

        IEnumerator ScoringRoutine()
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

            double coinsToGive = Math.Ceiling(gameStateMachine.currentRoundScore / gameStateMachine.roundGoal)*3;
            Singleton.Instance.playerStats.AddCoins(coinsToGive);

            yield return new WaitForSeconds(1f);
            ExitState();
        }
    }
}
