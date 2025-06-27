using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BoardPopulateInfo
{
    public List<GameObject> spawnerRootPrefabs;
    [Tooltip("If ≥ 0, this will replace the normal calculation of numberPegs.")]
    public int overrideStartingCabbages = -1;
}
