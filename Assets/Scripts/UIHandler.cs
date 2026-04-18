using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System;
using TMPro;

// Provides methods for rendering UI changes based on state in GameHandler
public class UIHandler : MonoBehaviour
{
    // --- OBJECT REFERENCES ---

    // GameHandler
    [SerializeField] private GameHandler gameHandler;

    // UI Groups
    [SerializeField] private GameObject generalGroup;
    [SerializeField] private GameObject scoringDisable;
    [SerializeField] private GameObject knockoutDisable; // General UI elements to hide when knockout enabled
    [SerializeField] private GameObject knockoutGroup;
    [SerializeField] private GameObject loseGroup;

    // UI Elements - General
    [SerializeField] private TextMeshProUGUI roundLabel;
    // TODO: design and add round timeline widget
    [SerializeField] private TextMeshProUGUI roundFutureThreshold;
    [SerializeField] private TextMeshProUGUI roundScoreNumber;
    [SerializeField] private TextMeshProUGUI roundScoreLabel;
    [SerializeField] private GameObject nextButton;

    // UI Elements - Knockout
    [SerializeField] private TextMeshProUGUI roundScoreNumberKnockout;
    // TODO: design and add progress bar
    [SerializeField] private TextMeshProUGUI thresholdLabel;

    // UI Elements - Lose Screen
    [SerializeField] private TextMeshProUGUI loseTitle;
    [SerializeField] private TextMeshProUGUI roundLabelLosing;
    [SerializeField] private TextMeshProUGUI scoreDifference;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        UpdateScoreState();
    }

    // State Changes
    public async Task EnterPlacingState()
    {
        SetupPlacing();
        try
        {
            await nextButton.GetComponent<UIAnimatable>().Hide();
        }
        catch (Exception e)
        {
            print(e.Message);
            return;
        }
        roundLabel.text = "Round " + GameHandler.Round;

        int nextKnockoutRound = GameHandler.Round + GameHandler.KNOCKOUT_ROUNDS
            - (GameHandler.Round % GameHandler.KNOCKOUT_ROUNDS);
        roundFutureThreshold.text = "Must score " + GameHandler.ScoreThreshold + 
            (GameHandler.Round % GameHandler.KNOCKOUT_ROUNDS == 0?
                " this round."
                : " by round " + nextKnockoutRound);
    }

    public async Task EnterScoringState()
    {
        if (GameHandler.IsKnockout)
        {
            SetupKnockout();
        } else
        {
            SetupScoring();
        }
        
        await nextButton.GetComponent<UIAnimatable>().Hide();
    }

    public async Task EnterLosingState()
    {
        SetupLosing();
    }

    // Button
    public async Task ShowNextButton()
    {
        await nextButton.GetComponent<UIAnimatable>().Show();
    }

    public void OnNextButtonClicked()
    {
        if (GameHandler.CurrentPhase == GameHandler.Phase.Placing)
        {
            gameHandler.StartScoring();
        } else
        {
            gameHandler.StartPlacing();
        }
        
    }
    public void CreateScoreGraphic(Vector3 worldPos, int score)
    {
        try {
            GameObject scoreGraphic = Instantiate(GameHandler.GetResource("Prefabs/UI/ScoreGraphic") as GameObject);
            scoreGraphic.transform.SetParent(transform);
            scoreGraphic.transform.position = Camera.main.WorldToScreenPoint(worldPos);
            TextMeshProUGUI scoreText = scoreGraphic.GetComponent<TextMeshProUGUI>();
            scoreText.text = (score >= 0? "+" : "-") + score;
            ScoreGraphic graphic = scoreGraphic.GetComponent<ScoreGraphic>();
            graphic.timeToLive = 0.5f;
            graphic.Init();
        } catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void UpdateScoreState()
    {
        roundScoreNumber.text = GameHandler.RoundScore + "";
        roundScoreNumberKnockout.text = GameHandler.RoundScore + "";
    }

    // Setters
    public void SetRoundLabel()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // --- PRIVATE METHODS ---
    private void SetupPlacing()
    {
        generalGroup.SetActive(true);
        knockoutGroup.SetActive(false);
        loseGroup.SetActive(false);

        knockoutDisable.SetActive(true);
        scoringDisable.SetActive(false);
    }

    private void SetupScoring()
    {
        generalGroup.SetActive(true);
        knockoutGroup.SetActive(false);
        loseGroup.SetActive(false);

        knockoutDisable.SetActive(true);
        scoringDisable.SetActive(true);
    }

    private void SetupKnockout()
    {
        generalGroup.SetActive(true);
        knockoutGroup.SetActive(true);
        loseGroup.SetActive(false);

        knockoutDisable.SetActive(false);
    }

    private void SetupLosing()
    {
        generalGroup.SetActive(false);
        knockoutGroup.SetActive(false);
        loseGroup.SetActive(true);
    }
}
