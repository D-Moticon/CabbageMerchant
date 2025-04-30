using UnityEngine;

public class ForceNextBallScaleEffect : ItemEffect
{
    public float newScale = 2f;
    public PhysicsMaterial2D newPhysMat;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.launchModifierManager.IncreaseNextBallScale(newScale);

        if (newPhysMat != null)
        {
            Singleton.Instance.launchModifierManager.ForceNextBallPhysMat(newPhysMat);
        }
    }

    public override string GetDescription()
    {
        return ($"Increase size of next ball by {newScale:F1}");
    }
}
