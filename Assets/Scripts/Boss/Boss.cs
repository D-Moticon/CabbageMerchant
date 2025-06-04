using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Boss", menuName = "Scriptable Objects/Boss")]
public class Boss : ScriptableObject
{
    public string bossName;
    public Sprite bossSprite;
    
    public List<BossPhase> phases = new List<BossPhase>();

    
}
