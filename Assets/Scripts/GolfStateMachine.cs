using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

public class GolfStateMachine : MonoBehaviour
{
    public Launcher launcher;
    private Ball activeBall;
    private Prize prizeToGive;
    public DialogueLine loseDialogue;
    public DialogueLine winDialogue;
    public DialogueLine overDidntLoseDialogue;

    const float velocityThreshold = 1f;
    const float requiredTimeBelow = 2.5f;
    private const float maxTime = 17f;

    public List<GameObject> variants;
    [SerializeReference]
    public List<Prize> prizes;
    private bool prizeWon = false;
    
    private void OnEnable()
    {
        GolfHole.PrizeScored += PrizeScoredListener;
        Ball.BallDisabledEvent += BallDisabledListener;
    }

    private void OnDisable()
    {
        GolfHole.PrizeScored -= PrizeScoredListener;
        Ball.BallDisabledEvent -= BallDisabledListener;
    }

    private void Start()
    {
        //DialogueContext dc = new DialogueContext();
        //StartCoroutine(GolfRoutine(dc));
    }

    public IEnumerator GolfRoutine(DialogueContext dc)
    {
        int ballsRemaining = 3;
        GameStateMachine.BallsRemainingUpdatedEvent?.Invoke(ballsRemaining);

        List<GameObject> shuffledVars = new List<GameObject>(variants);
        shuffledVars.Shuffle();

        int currentVar = 0;
        EnableVariant(shuffledVars, currentVar);
        
        while (ballsRemaining > 0)
        {
            yield return new WaitForSeconds(0.5f); //prevent misfires
            while (!Singleton.Instance.playerInputManager.fireDown)
            {
                yield return null;
            }

            activeBall = launcher.LaunchBall();
            ballsRemaining--;
            GameStateMachine.BallsRemainingUpdatedEvent?.Invoke(ballsRemaining);
            
            float slowTimer = 0f;
            float totalTimer = 0f;
            while (activeBall != null && totalTimer < maxTime)
            {
                float speed = activeBall.rb.linearVelocity.magnitude;
                if (speed <= velocityThreshold)
                {
                    // accumulate “slow” time
                    slowTimer += Time.deltaTime;
                    if (slowTimer >= requiredTimeBelow)
                    {
                        activeBall.KillBall();
                        break; // sustained slow → out
                    }
                }
                else
                {
                    // reset if it picks up speed again
                    slowTimer = 0f;
                }

                totalTimer += Time.deltaTime;
                yield return null;
            }

            if (activeBall != null)
            {
                activeBall.KillBall();
            }
            
            if (prizeToGive != null)
            {
                Task winDialogueTask = new Task(winDialogue.RunTask(dc));
                while (winDialogueTask.Running) yield return null;
            
                Task t = new Task(prizeToGive.AwardPrize(dc));
                while (t.Running) yield return null;
                dc.dialogueBox.HideDialogueBox();
                prizeToGive = null;
                
                currentVar++;
                if (currentVar > shuffledVars.Count - 1)
                {
                    currentVar = 0;
                }

                if (ballsRemaining > 0)
                {
                    EnableVariant(shuffledVars, currentVar);
                }

                prizeWon = true;
            }
        }

        if (prizeWon)
        {
            Task winDialogueTask = new Task(overDidntLoseDialogue.RunTask(dc));
            while (winDialogueTask.Running) yield return null;
        }
        
        else
        {
            Task loseDialogueTask = new Task(loseDialogue.RunTask(dc));
            while (loseDialogueTask.Running) yield return null;
        }
        
        //yield return new WaitForSeconds(1f);
    }

    void EnableVariant(List<GameObject> varGOs, int index)
    {
        for (int i = 0; i < varGOs.Count; i++)
        {
            if (i == index)
            {
                varGOs[i].gameObject.SetActive(true);
                GolfHole gHole = varGOs[i].GetComponentInChildren<GolfHole>();
                int randPrize = Random.Range(0, prizes.Count);
                gHole.SetPrize(prizes[randPrize]);
            }

            else
            {
                varGOs[i].gameObject.SetActive(false);
            }
        }
    }
    
    void PrizeScoredListener(Prize p)
    {
        prizeToGive = p;
    }

    void BallDisabledListener(Ball ball)
    {
        if (ball == activeBall)
        {
            activeBall = null;
        }
    }
}
