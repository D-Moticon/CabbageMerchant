using System.Collections.Generic;
using UnityEngine;

public class InsertMapLayersItemEffect : ItemEffect
{
    public MapInsertedLayers mapInsertedLayers;
    public string layerDescription;
    
    public override void InitializeItemEffect()
    {
        MapGenerator.CollectInsertedLayersEvent += CollectInsertedLayersListener;
    }

    public override void DestroyItemEffect()
    {
        MapGenerator.CollectInsertedLayersEvent -= CollectInsertedLayersListener;
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        
    }

    public override string GetDescription()
    {
        return $"Every {mapInsertedLayers.everyXLayer} map points, encounter {layerDescription}";
    }
    
    private void CollectInsertedLayersListener(List<MapInsertedLayers> insertedlayers)
    {
        insertedlayers.Add(mapInsertedLayers);
    }
}
