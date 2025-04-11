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
    [Header("Game Rules")]
    public int maxLives = 3;

    public int currentLives = 3;
    
    public List<Ball> activeBalls = new List<Ball>();
    
    
    
    public static Action EnteringAimStateAction;
    public static Action ExitingAimStateAction;
    
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
    
    public class PopulateBoardState : State
    {
        public override void EnterState()
        {
            gameStateMachine.currentLives = gameStateMachine.maxLives;
            gameStateMachine.StartCoroutine(PopulateBoard());
        }

        public override void UpdateState()
        {
            
        }

        public override void ExitState()
        {
            
        }

        IEnumerator PopulateBoard()
        {
            int numPegs = gameStateMachine.numberPegs;
            Vector2[] positions = GameSingleton.Instance.boardMetrics.GetRandomGridPoints(numPegs);
            
            for (int i = 0; i < numPegs; i++)
            {
                Cabbage c = gameStateMachine.cabbagePooledObject.Spawn(positions[i], Quaternion.identity).GetComponent<Cabbage>();
                c.bonkFeel.PlayFeedbacks();
                yield return new WaitForSeconds(0.05f);
            }

            State newState = new AimingState();
            gameStateMachine.ChangeState(newState);
        }
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
                gameStateMachine.currentLives--;
                State newState = new BouncingState();
                gameStateMachine.ChangeState(newState);
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

                if (gameStateMachine.currentLives <= 0)
                {
                    
                }
                
                gameStateMachine.ChangeState(newState);
            }
        }

        public override void ExitState()
        {
            
        }
    }
}
