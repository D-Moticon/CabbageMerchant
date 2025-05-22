using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkinDatabase", menuName = "Scriptable Objects/SkinDatabase")]
public class SkinDatabase : ScriptableObject
{
    [System.Serializable]
    public class SkinInfo
    {
        public Skin skin;
    }

    public List<SkinInfo> skinInfos = new List<SkinInfo>();
}
