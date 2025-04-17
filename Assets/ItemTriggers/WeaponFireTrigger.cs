using UnityEngine;

public class WeaponFireTrigger : Trigger
{
    
    public override void InitializeTrigger(Item item)
    {
        PlayerInputManager.weaponFireDownAction += WeaponFireDownListener;
    }

    public override void RemoveTrigger(Item item)
    {
        PlayerInputManager.weaponFireDownAction -= WeaponFireDownListener;
    }

    public override string GetTriggerDescription()
    {
        return ("Weapon Fire");
    }

    void WeaponFireDownListener()
    {
        if (GameSingleton.Instance == null)
        {
            return;
        }
        
        if (!(GameSingleton.Instance.gameStateMachine.currentState is GameStateMachine.BouncingState))
        {
            return;
        }
        owningItem.TryTriggerItem();
    }
}
