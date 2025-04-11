using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text coinsText;
    public double coinsPerSecond = 100;
    private double currentCoins;
    private double targetCoins;
    public Transform ballsParent;
    public Image ballRemainingPrefab;

    private void OnEnable()
    {
        GameStateMachine.BallsRemainingEvent += UpdateBallsIndicator;
        PlayerStats.CoinsUpdated += CoinsUpdatedListener;
    }

    private void OnDisable()
    {
        GameStateMachine.BallsRemainingEvent -= UpdateBallsIndicator;
        PlayerStats.CoinsUpdated -= CoinsUpdatedListener;
    }


    void UpdateBallsIndicator(int ballsRemaining)
    {
        Image[] existingImages = ballsParent.GetComponentsInChildren<Image>();
        foreach (Image img in existingImages)
        {
            Destroy(img.gameObject);
        }

        for (int i = 0; i < ballsRemaining; i++)
        {
            Image img = Instantiate(ballRemainingPrefab, ballsParent);
        }
    }

    void CoinsUpdatedListener(double newCoins)
    {
        targetCoins = newCoins;
    }

    private void Update()
    {
        double coinDiff = System.Math.Abs(currentCoins - targetCoins);
        
        if (coinDiff < 1)
        {
            currentCoins = targetCoins;
        }
        
        else
        {
            double coinRate = System.Math.Max(coinDiff*2, coinsPerSecond);
            
            if (currentCoins < targetCoins)
            {
                currentCoins += coinRate * Time.deltaTime;
            }

            else
            {
                currentCoins -= coinRate * Time.deltaTime;
            }
        }

        coinsText.text = Helpers.FormatWithSuffix(currentCoins);
    }
}
