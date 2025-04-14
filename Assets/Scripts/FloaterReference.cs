using UnityEngine;

[CreateAssetMenu(fileName = "FloaterReference", menuName = "Floaters/Floater Reference")]
public class FloaterReference : ScriptableObject
{
    public Floater floaterPrefab;

    public void Spawn(string text, Vector2 pos = default, Color color = default, float scale = 1f)
    {
        Singleton.Instance.floaterManager.SpawnFloater(this, text, pos, color, scale);
    }
}