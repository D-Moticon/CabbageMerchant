using UnityEngine;
using TMPro;

public class BoardGameBucket : MonoBehaviour
{
    [HideInInspector]public int moveAmount;
    public TMP_Text moveNumberText;
    
    public void SetMoveNumber(int num)
    {
        moveAmount = num;
        moveNumberText.text = num.ToString();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball b = other.GetComponent<Ball>();
        if (b == null) return;

        GameSingleton.Instance.boardGameManager.OnBucketHit(this);
    }
}
