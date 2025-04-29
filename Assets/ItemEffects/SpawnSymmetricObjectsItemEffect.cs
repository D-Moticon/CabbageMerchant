using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class SpawnSymmetricObjectsItemEffect : ItemEffect
{
    [Header("Y-position settings")]
    public Vector2 yRange = new Vector2(-4f,  3f);
    [Tooltip("Minimum vertical distance between outer objects and the center.")]
    public float    minYSeparation = 1f;

    public List<GameObject> centralPrefabs;
    public List<GameObject> outerPrefabs;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        int centralRand = Random.Range(0, centralPrefabs.Count);
        int outerRand   = Random.Range(0, outerPrefabs.Count);

        // use the static GameObject.Instantiate since this isn't a MonoBehaviour
        GameObject centralObject = GameObject.Instantiate(
            centralPrefabs[centralRand],
            GameSingleton.Instance.gameStateMachine.transform);
        GameObject outerObj1 = GameObject.Instantiate(
            outerPrefabs[outerRand],
            GameSingleton.Instance.gameStateMachine.transform);
        GameObject outerObj2 = GameObject.Instantiate(
            outerPrefabs[outerRand],
            GameSingleton.Instance.gameStateMachine.transform);

        // pick center Y
        float centralY = Random.Range(yRange.x, yRange.y);

        // pick outer Y at least minYSeparation from centralY
        float outerY = ChooseOuterY(centralY);

        // apply positions
        centralObject.transform.position = new Vector3(   0f, centralY, 0f);
        outerObj1.transform.position      = new Vector3(-4.5f,   outerY, 0f);
        outerObj2.transform.position      = new Vector3( 4.5f,   outerY, 0f);
        
        // reset rotations & scales
        centralObject.transform.rotation = Quaternion.identity;
        outerObj1.transform.rotation     = Quaternion.identity;
        outerObj2.transform.rotation     = Quaternion.identity;

        centralObject.transform.localScale = Vector3.one;
        outerObj1.transform.localScale     = Vector3.one;
        outerObj2.transform.localScale     = new Vector3(-1f,1f,1f);
    }

    private float ChooseOuterY(float centralY)
    {
        float lowerMax = centralY - minYSeparation;
        float upperMin = centralY + minYSeparation;

        bool hasLower = lowerMax > yRange.x;
        bool hasUpper = upperMin < yRange.y;

        if (!hasLower && !hasUpper)
        {
            // no valid gap—just fallback
            return Random.Range(yRange.x, yRange.y);
        }
        else if (hasLower && hasUpper)
        {
            if (Random.value < 0.5f)
                return Random.Range(yRange.x, lowerMax);
            else
                return Random.Range(upperMin, yRange.y);
        }
        else if (hasLower)
        {
            return Random.Range(yRange.x, lowerMax);
        }
        else // only upper
        {
            return Random.Range(upperMin, yRange.y);
        }
    }

    public override string GetDescription()
    {
        return "";
    }
}
