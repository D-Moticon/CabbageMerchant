using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Scriptable Objects/Difficulty")]
public class Difficulty : ScriptableObject
{
    public string displayName;
    
    public double firstRoundGoal = 15;
    public float goalBase = 1f; //exponent base
    public float goalPower = 1.2f;
    public float metacurrencyBase = 1f;
    public float metacurrencyPower = 1.2f;

    public int mapLayerNumberForCalc = 45;
    public double roundGoalCalc;
    
    [Button("Calculate Round Goal")]
    public void calculateRoundGoal()
    {
        double roundGoal = firstRoundGoal + goalBase * Mathf.Pow(mapLayerNumberForCalc, goalPower);
        roundGoalCalc = roundGoal;
    }
}
