using UnityEngine;

public class GrindRailHPEffect : ItemEffect
{
    public int hpAdd = 1;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        
    }

    public override string GetDescription()
    {
        return ($"Grind rails can be used +{hpAdd} extra time before being destroyed");
    }

    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        GrindRail.GrindRailAwakeEvent += OnGrindRailAwake;
    }

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        GrindRail.GrindRailAwakeEvent -= OnGrindRailAwake;
    }

    void OnGrindRailAwake(GrindRail gr)
    {
        gr.AddHP(hpAdd);
    }
}
