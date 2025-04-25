using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Febucci.UI;
using MoreMountains.Feedbacks;

public class UIManager : MonoBehaviour
{
    public TMP_Text coinsText;
    public double coinsPerSecond = 100;
    private double currentCoins;
    private double targetCoins;
    public TMP_Text keysText;
    public Transform ballsParent;
    public Transform livesParent;
    public Image ballRemainingPrefab;
    public Image lifeRemainingPrefab;
    [SerializeField]private TMP_Text notificationText;
    public TypewriterByCharacter notificationTextTypewriter;
    public MMF_Player notificationFeel;
    public Animator lifeLostAnimator;
    public Animator lifeGainedAnimator;
    public TMP_Text metacurrencyText;
    public GameObject runOnlyParent;

    private void OnEnable()
    {
        notificationText.alpha = 0f;
        lifeLostAnimator.gameObject.SetActive(false);
        
        GameStateMachine.BallsRemainingUpdatedEvent += UpdateBallsIndicator;
        PlayerStats.CoinsUpdated += CoinsUpdatedListener;
        PlayerStats.LivesUpdated += UpdateLivesIndicator;
        PlayerStats.LifeLostEvent += LifeLostListener;
        PlayerStats.LifeGainedEvent += LifeGainedListener;
        RunManager.RunStartEvent += RunStartListener;
        PlayerStats.KeysUpdatedEvent += KeysUpdatedListener;
        PlayerStats.MetacurrencyUpdatedEvent += MetacurrencyUpdatedListener;
        RunManager.SceneChangedEvent += SceneChangedListener;
        BuildManager.FullGameStartedEvent += FullGameStartedListener;
    }

    private void OnDisable()
    {
        GameStateMachine.BallsRemainingUpdatedEvent -= UpdateBallsIndicator;
        PlayerStats.CoinsUpdated -= CoinsUpdatedListener;
        PlayerStats.LivesUpdated -= UpdateLivesIndicator;
        PlayerStats.LifeLostEvent -= LifeLostListener;
        PlayerStats.LifeGainedEvent -= LifeGainedListener;
        RunManager.RunStartEvent -= RunStartListener;
        PlayerStats.KeysUpdatedEvent -= KeysUpdatedListener;
        PlayerStats.MetacurrencyUpdatedEvent -= MetacurrencyUpdatedListener;
        RunManager.SceneChangedEvent -= SceneChangedListener;
        BuildManager.FullGameStartedEvent -= FullGameStartedListener;
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

    void UpdateLivesIndicator(int livesRemaining)
    {
        Image[] existingImages = livesParent.GetComponentsInChildren<Image>();
        foreach (Image img in existingImages)
        {
            Destroy(img.gameObject);
        }

        for (int i = 0; i < livesRemaining; i++)
        {
            Image img = Instantiate(lifeRemainingPrefab, livesParent);
        }
    }

    void CoinsUpdatedListener(double newCoins)
    {
        targetCoins = newCoins;
    }

    void KeysUpdatedListener(int newKeys)
    {
        keysText.text = newKeys.ToString();
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

    public void ShowNotification(string text)
    {
        notificationText.gameObject.SetActive(true);
        notificationTextTypewriter.ShowText(text);
        notificationFeel.PlayFeedbacks();
    }

    void LifeLostListener()
    {
        lifeLostAnimator.gameObject.SetActive(true);
        lifeLostAnimator.Play("LifeLostAnim");
    }

    void LifeGainedListener()
    {
        lifeGainedAnimator.gameObject.SetActive(true);
        lifeGainedAnimator.Play("LifeGained");
    }

    void RunStartListener(RunManager.RunStartParams rsp)
    {
        ShowNotification("A new journey begins!");
    }

    void MetacurrencyUpdatedListener(double newMetacurrency)
    {
        metacurrencyText.text = Helpers.FormatWithSuffix(newMetacurrency);
    }

    void SceneChangedListener(string sceneName)
    {
        if (sceneName == "Overworld")
        {
            HideRunOnlyItems();
        }

        else
        {
            ShowRunOnlyItems();
        }
    }

    public void HideRunOnlyItems()
    {
        runOnlyParent.SetActive(false);
    }

    public void ShowRunOnlyItems()
    {
        runOnlyParent.SetActive(true);
    }

    void FullGameStartedListener()
    {
        HideRunOnlyItems();
    }
}
