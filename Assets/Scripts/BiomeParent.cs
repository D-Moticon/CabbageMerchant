using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BiomeParent : MonoBehaviour
{
    public PooledObjectData cabbagePooledObject;
    public BoardVariantInfo[] boardVariantInfos;
    
    [System.Serializable]
    public class BoardVariantInfo
    {
        public GameObject boardVariant;
        public ChaosCabbageSO chaosCabbage;
    }

    public BoardVariantInfo GetBoardVariant()
    {
        List<BoardVariantInfo> bvis = new List<BoardVariantInfo>();

        for (int i = 0; i < boardVariantInfos.Length; i++)
        {
            if (boardVariantInfos[i].chaosCabbage == null)
            {
                bvis.Add(boardVariantInfos[i]);
            }

            else
            {
                if (boardVariantInfos[i].chaosCabbage ==
                    Singleton.Instance.chaosManager.GetChaosCabbageFromPetDef(Singleton.Instance.petManager.currentPet))
                {
                    bvis.Add(boardVariantInfos[i]);
                }
            }
        }
        
        bvis.Shuffle();
        if (bvis.Count > 0)
        {
            return bvis[0];
        }
        else
        {
            return boardVariantInfos[0];
        }
    }
    
    public bool spawnCabbagesInAllSlots = false;
    public bool preventCabbageConversion = false;
}
