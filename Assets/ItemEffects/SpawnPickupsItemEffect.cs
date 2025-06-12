using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnPickupsItemEffect : ItemEffect
{
    public enum TriggerMode
    {
        OnAllCollected,
        OnBallsExhaustedWithRemaining
    }

    [Tooltip("When to trigger the item effects.")]
    public TriggerMode triggerMode = TriggerMode.OnAllCollected;

    [Tooltip("Pickup prefab to spawn.")]
    public PooledObjectData objectToSpawn;
    [Tooltip("Total pickups to spawn.")]
    public int numberObjects = 10;

    [Tooltip("Effects to run when trigger condition is met.")]
    [SerializeReference]
    public List<ItemEffect> effectsOnCondition;

    private List<GameObject> spawnedPickups;
    private const float maxAllowedDistance = 3.0f;
    private bool effectsTriggered = false;

    public override void InitializeItemEffect()
    {
        GameStateMachine.PreBoardPopulateAction += ClearPickups;
        GameStateMachine.BoardFinishedPopulatingAction += SpawnPickups;
        GameStateMachine.ExitingBounceStateAction += OnBounceStateExited;
        Pickup.pickupCollectedEvent += PickupCollectedListener;

        foreach (var eff in effectsOnCondition)
        {
            eff.owningItem = owningItem;
            eff.InitializeItemEffect();
        }
    }

    public override void DestroyItemEffect()
    {
        GameStateMachine.PreBoardPopulateAction -= ClearPickups;
        GameStateMachine.BoardFinishedPopulatingAction -= SpawnPickups;
        GameStateMachine.ExitingBounceStateAction -= OnBounceStateExited;
        Pickup.pickupCollectedEvent -= PickupCollectedListener;

        foreach (var eff in effectsOnCondition)
            eff.DestroyItemEffect();
    }

    private void ClearPickups()
    {
        effectsTriggered = false;
        if (GameSingleton.Instance == null || objectToSpawn == null)
            return;

        if (spawnedPickups != null)
        {
            foreach (GameObject p in spawnedPickups)
            {
                if (p != null)
                {
                    p.gameObject.SetActive(false);
                }
            }
            spawnedPickups.Clear();
        }
        
    }

    private void SpawnPickups()
    {
        effectsTriggered = false;
        if (GameSingleton.Instance == null || objectToSpawn == null)
            return;

        var slots = GameSingleton.Instance.gameStateMachine.bonkableSlots;
        if (slots == null || slots.Count < 2)
            return;

        spawnedPickups = new List<GameObject>();
        for (int i = 0; i < numberObjects; i++)
        {
            var slotA = slots[Random.Range(0, slots.Count)];
            int idxA = slots.IndexOf(slotA);
            int idxB = (idxA + 1) % slots.Count;
            var slotB = slots[idxB];

            float dist = Vector2.Distance(slotA.transform.position, slotB.transform.position);
            if (dist > maxAllowedDistance)
            {
                idxB = (idxA - 1 + slots.Count) % slots.Count;
                slotB = slots[idxB];
            }

            Vector3 spawnPos = (slotA.transform.position + slotB.transform.position) * 0.5f;
            var pickup = objectToSpawn.Spawn(spawnPos, Quaternion.identity);
            spawnedPickups.Add(pickup);
        }
    }

    private void PickupCollectedListener(Pickup p)
    {
        if (spawnedPickups == null || effectsTriggered)
            return;

        spawnedPickups.RemoveAll(x => x == null || !x.activeInHierarchy);
        if (triggerMode == TriggerMode.OnAllCollected && spawnedPickups.Count == 0)
        {
            TriggerEffects();
        }
    }

    private void OnBounceStateExited()
    {
        if (spawnedPickups == null || effectsTriggered)
            return;

        if (triggerMode == TriggerMode.OnBallsExhaustedWithRemaining)
        {
            bool anyLeft = spawnedPickups.Any(x => x != null && x.activeInHierarchy);
            int ballsLeft = GameSingleton.Instance.gameStateMachine.currentBalls;
            if (anyLeft && ballsLeft <= 0)
                TriggerEffects();
        }
    }

    private void TriggerEffects()
    {
        effectsTriggered = true;
        var tc = new TriggerContext();
        foreach (var eff in effectsOnCondition)
            eff.TriggerItemEffect(tc);
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // not used: spawning handled via BoardFinishedPopulatingEvent
    }

    public override string GetDescription()
    {
        string modeDesc = triggerMode == TriggerMode.OnAllCollected
            ? "when all pickups are collected"
            : "if pickups remain when balls run out";
        return $"Spawns {numberObjects} pickups. Triggers effects {modeDesc}.";
    }
}
