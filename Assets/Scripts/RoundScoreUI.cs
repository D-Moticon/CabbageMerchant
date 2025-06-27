using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Linq;

public class RoundScoreUI : MonoBehaviour
{
    public TMP_Text roundGoalHeaderText;
    public TMP_Text roundScoreText;
    public MMF_Player roundScoreFeel;
    public TMP_Text roundGoalText;
    public Slider roundScoreSlider;
    public TMP_Text roundScoreMultText;
    public MMF_Player roundGoalHitFeel;
    public SFXInfo roundGoalHitSFX;
    public PooledObjectData roundGoalHitVFX;
    public Transform roundGoalHitVFXLocation;
    private bool isFlashing = false; //Give the bar a second to flash before it drops to zero
    public float flashDuration = 0.5f;
    private float flashCounter = 0f;
    private double oldScore = 0;
    public FadeObject tickLine;
    public MMF_Player tickLineFeel;
    public Vector2 inputTickPowerRange = new Vector2(0, 1000);
    public Vector2 outputTickPowerRange = new Vector2(1f, 10f);
    
    [Header("Boss Mode UI")]    
    public Sprite bossFillSprite;
    public Color bossFillColor;
    public Color tickLineBossColor = Color.red;
    public Image bossSpriteImage;
    private bool isBossMode = false;

    private void Awake()
    {
        isBossMode = Singleton.Instance.bossFightManager.isBossFight;
        if (isBossMode)
        {
            BossFightManager.BossPhaseParams bpp = new BossFightManager.BossPhaseParams();
            SetupBossMode(bpp);
        }
    }

    private void OnEnable()
    {
        GameStateMachine.RoundGoalUpdatedEvent += RoundGoalUpdated;
        GameStateMachine.RoundScoreUpdatedEvent += RoundScoreUpdated;
        GameStateMachine.BoardFinishedPopulatingAction += BoardPopulatedListener;
        GameStateMachine.RoundGoalOverHitEvent += RoundGoalHitListener;
        BossFightManager.bossPhaseStartedEvent += SetupBossMode;
    }

    private void OnDisable()
    {
        GameStateMachine.RoundGoalUpdatedEvent -= RoundGoalUpdated;
        GameStateMachine.RoundScoreUpdatedEvent -= RoundScoreUpdated;
        GameStateMachine.BoardFinishedPopulatingAction -= BoardPopulatedListener;
        GameStateMachine.RoundGoalOverHitEvent -= RoundGoalHitListener;
        BossFightManager.bossPhaseStartedEvent -= SetupBossMode;
    }

    private void SetupBossMode(BossFightManager.BossPhaseParams bpp)
    {
        roundGoalHeaderText.text = "Boss Health";
        
        // 1) Swap slider fill sprite
        if (bossFillSprite != null && roundScoreSlider.fillRect != null)
        {
            var fillImage = roundScoreSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.sprite = bossFillSprite;
                fillImage.type = Image.Type.Sliced;
                fillImage.color = bossFillColor;
            }
        }

        // 2) Change tick line color
        var tickLineImage = tickLine.GetComponent<Image>();
        if (tickLineImage != null)
        {
            tickLineImage.color = tickLineBossColor;
        }

        // 3) Spawn phase markers along the bar
        var bossDef = Singleton.Instance.bossFightManager.boss;

        // 4) Display boss sprite
        if (bossSpriteImage != null && bossDef != null)
        {
            //bossSpriteImage.sprite = bossDef.bossSprite;
            //bossSpriteImage.gameObject.SetActive(true);
        }
    }
    
    void RoundGoalUpdated(double newGoal)
    {
        if (newGoal > 0)
        {
            roundGoalText.text = Helpers.FormatWithSuffix(newGoal);
        }

        else
        {
            roundGoalText.text = "---";
        }
    }

    private void RoundScoreUpdated(double newScore)
    {
        if (isBossMode)
        {
            // Boss mode: countdown current phase health
            double max = GameSingleton.Instance.gameStateMachine.roundGoal;
            float remainingFrac = max > 0 ? 1f - (float)(newScore / max) : 0f;
            roundScoreSlider.value = remainingFrac;

            // show remaining health
            double bosshealth = Math.Max(Math.Ceiling(max - newScore), 0);
            roundScoreText.text = Helpers.FormatWithSuffix(bosshealth);

            double diff = Math.Abs(newScore - oldScore);
            
            // trigger tick line & feedback on damage
            if (diff > 0.5)
            {
                roundScoreFeel.PlayFeedbacks();
                float power = Helpers.RemapClamped((float)diff,
                    inputTickPowerRange.x, inputTickPowerRange.y,
                    outputTickPowerRange.x, outputTickPowerRange.y);
                tickLine.properties[0].materialFloatStartValue = power;
                tickLine.FadeForward();
                tickLineFeel.PlayFeedbacks();

                // flash full bar briefly
                isFlashing = true;
                flashCounter = flashDuration;
            }

            // percent health remaining
            int percent = Mathf.CeilToInt(remainingFrac * 100f);
            if (percent < 0)
            {
                percent = 0;
            }
            roundScoreMultText.text = $"{percent}%";

            oldScore = newScore;
            return;
        }

        // Normal scoring...
        if (GameSingleton.Instance.gameStateMachine.roundGoal < 1)
        {
            roundScoreMultText.text = $"0<size=5>x";
            roundScoreSlider.value = 0;
        }

        roundScoreText.text = Helpers.FormatWithSuffix(newScore);
        double roundScoreMult = GameSingleton.Instance.gameStateMachine.currentRoundScoreOverMult;

        if (double.IsNaN(roundScoreMult))
        {
            roundScoreMultText.text = $"0<size=5>x";
            roundScoreSlider.value = 0;
            return;
        }

        if (newScore > oldScore)
        {
            roundScoreFeel.PlayFeedbacks();
            double diff = newScore - oldScore;
            float power = Helpers.RemapClamped((float)diff,
                inputTickPowerRange.x, inputTickPowerRange.y,
                outputTickPowerRange.x, outputTickPowerRange.y);
            tickLine.properties[0].materialFloatStartValue = power;
            tickLine.FadeForward();
            tickLineFeel.PlayFeedbacks();
        }

        double sliderVal = roundScoreMult % 1;
        if (isFlashing)
        {
            flashCounter -= Time.deltaTime;
            if (flashCounter <= 0f) isFlashing = false;
            sliderVal = 1;
        }

        roundScoreSlider.value = (float)sliderVal;
        roundScoreMultText.text = $"{Helpers.FormatWithSuffix(Math.Floor(roundScoreMult))}<size=5>x";
        oldScore = newScore;
    }

    void BoardPopulatedListener()
    {
        roundScoreSlider.value = 0;
        roundScoreMultText.text = "";
    }

    void RoundGoalHitListener(double multiple)
    {
        roundGoalHitFeel.PlayFeedbacks();
        roundGoalHitVFX.Spawn(roundGoalHitVFXLocation.position);
        roundGoalHitSFX.Play();
        isFlashing = true;
        flashCounter = flashDuration;
    }

}
