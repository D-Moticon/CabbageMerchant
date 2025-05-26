using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkinDatabase", menuName = "Scriptable Objects/SkinDatabase")]
public class SkinDatabase : ScriptableObject
{
    [System.Serializable]
    public class SkinInfo
    {
        [Tooltip("The skin asset")]
        public Skin skin;

        [Tooltip("If true, this skin is hidden/disabled in the demo build")]
        public bool InDemo = false;
    }

    public List<SkinInfo> skinInfos = new List<SkinInfo>();
}