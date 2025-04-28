using UnityEngine;

public class BounceStateEnteredTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.EnteringBounceStateAction += OnBounceStateEntered;
    }

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.EnteringBounceStateAction -= OnBounceStateEntered;
    }

    public override string GetTriggerDescription()
    {
        return ("Ball fired");
    }

    void OnBounceStateEntered()
    {
        owningItem.TryTriggerItem();
    }
}
