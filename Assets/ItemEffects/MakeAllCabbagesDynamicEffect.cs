using UnityEngine;
using System.Collections.Generic;

public class MakeAllCabbagesDynamicEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        //Triggering on cabbage spawn
    }

    public override string GetDescription()
    {
        return ($"Cabbages move and bonk each other for {Singleton.Instance.playerStats.dynamicCabbageBonkPower}");
    }

    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        Cabbage.CabbageSpawnedEvent += CabbageSpawnedListener;
        GameStateMachine.TimerUpEvent += TimerUpListener;
        GameStateMachine.BallFiredEvent += BallFiredListener;
    }

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        Cabbage.CabbageSpawnedEvent -= CabbageSpawnedListener;
        GameStateMachine.TimerUpEvent -= TimerUpListener;
        GameStateMachine.BallFiredEvent -= BallFiredListener;
    }
    
    private void CabbageSpawnedListener(Cabbage c)
    {
        if (GameSingleton.Instance.gameStateMachine.currentState is GameStateMachine.BouncingState)
        {
            c.MakeDynamic();
        }
        
    }
    
    private void TimerUpListener()
    {
        if (GameSingleton.Instance == null)
        {
            return;
        }

        List<Cabbage> cabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;
        foreach (var c in cabbages)
        {
            c.MakeUnDynamic();
        }
    }
    
    private void BallFiredListener(Ball b)
    {
        List<Cabbage> cabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;
        foreach (var c in cabbages)
        {
            c.MakeDynamic();
        }
    }
}
