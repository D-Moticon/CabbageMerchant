using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Scriptable Objects/Difficulty")]
public class Difficulty : ScriptableObject
{
    public string displayName;
    
    public double firstRoundGoal = 15;
    public float goalBase = 1f; //exponent base
    public float goalPower = 1.2f;
    public float metacurrencyBase = 1f;
    public float metacurrencyPower = 1.2f;
}
