using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using TMPro;
using System.Linq;

// Provides methods for rendering UI changes based on state in GameHandler
public class UIHandler : MonoBehaviour
{
    // -- TYPES --

    private enum UIState
    {
        Placing,
        Scoring,
        ScoringKnockout,
        Lose,
        None
    }

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

    // UI Groups
    [SerializeField] private UIAnimatable[] placingElements;
    [SerializeField] private UIAnimatable[] scoringElements;
    [SerializeField] private UIAnimatable[] scoringKnockoutElements;
    [SerializeField] private UIAnimatable[] loseElements;

    private UIState uiState = UIState.None;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        Init();
    }

    public void Init()
    {
        SetRoundLabel();
        SetFutureThreshold();
        UpdateScoreState();
    }

    // -- OVERARCHING UI STATE CHANGES --

    public async Task EnterPlacingState()
    {
        SetRoundLabel();
        SetFutureThreshold();
    
        await RenderState(UIState.Placing, HideNextButton());
    }

    public async Task EnterScoringState()
    {
        UpdateScoreState();
        if (GameHandler.IsKnockout)
        {
            await RenderState(UIState.ScoringKnockout, HideNextButton());
        } else
        {
            await RenderState(UIState.Scoring, HideNextButton());
        }
    }

    public async Task EnterLosingState()
    {
        roundLabelLosing.text = "Round " + GameHandler.Round;

        scoreDifference.text = "Score: " + GameHandler.RoundScore + " Needed: " + GameHandler.ScoreThreshold;
        await RenderState(UIState.Lose, HideNextButton());
    }

    // -- SETTERS/LOGIC FOR INDIVIDUAL ELEMENTS --

    // Round Label
    public void SetRoundLabel()
    {
        roundLabel.text = "Round " + GameHandler.Round;
    }

    // Future Threshold
    public void SetFutureThreshold()
    {
        int nextKnockoutRound = GameHandler.Round + GameHandler.KNOCKOUT_ROUNDS
            - (GameHandler.Round % GameHandler.KNOCKOUT_ROUNDS);
        roundFutureThreshold.text = "Must score " + GameHandler.ScoreThreshold + 
            (GameHandler.Round % GameHandler.KNOCKOUT_ROUNDS == 0?
                " this round."
                : " by round " + nextKnockoutRound);
    }

    // Button
    public async Task ShowNextButton()
    {
        await nextButton.GetComponent<UIAnimatable>().Show();
    }

    public async Task HideNextButton()
    {
        await nextButton.GetComponent<UIAnimatable>().Hide();
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

    public void OnRetryButtonClicked()
    {
        GameHandler.BroadcastToBugs((Bug bug) => bug.Destroy());
        GameHandler.AllBugs = new Bug[0];
        print(GameHandler.AllBugs.Length);
        gameHandler.Init();
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    // Score UI
    public void UpdateScoreState()
    {
        roundScoreNumber.text = GameHandler.RoundScore + "";
        roundScoreNumberKnockout.text = GameHandler.RoundScore + "";
        thresholdLabel.text = GameHandler.ScoreThreshold + "";
    }

    // -- INSTANTIATE WORLD SPACE UI --
    public void CreateScoreGraphic(Vector3 worldPos, int score)
    {
        try {
            GameObject scoreGraphic = Instantiate(GameHandler.GetResource("Prefabs/UI/ScoreGraphic") as GameObject);
            scoreGraphic.transform.SetParent(transform);
            scoreGraphic.transform.position = Camera.main.WorldToScreenPoint(worldPos);
            scoreGraphic.GetComponent<UIAnimatable>().SetOriginalPosition(scoreGraphic.GetComponent<RectTransform>().position);
            TextMeshProUGUI scoreText = scoreGraphic.GetComponent<TextMeshProUGUI>();
            scoreText.text = (score >= 0? "+" : "-") + score;
            //ScoreGraphic graphic = scoreGraphic.GetComponent<ScoreGraphic>();
            //graphic.timeToLive = 0.5f;
            //graphic.Init();
        } catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    // --- PRIVATE METHODS ---

    // Shows and hides elements based on UI Group Arrays assigned in editor
    private async Task RenderState(UIState newState, Task additionalTask = null)
    {
        if (newState == uiState)
        {
            return;
        }
        
        UIAnimatable[] prevElements = GetUIElements(uiState);
        UIAnimatable[] newElements = GetUIElements(newState);

        UIAnimatable[] hideElements = prevElements.Except(newElements).ToArray();
        UIAnimatable[] showElements = newElements.Except(prevElements).ToArray();

        List<Task> animations = new();

        foreach (UIAnimatable element in hideElements)
        {
            animations.Add(element.Hide());
        }

        foreach (UIAnimatable element in showElements)
        {
            animations.Add(element.Show());
        }

        if (additionalTask != null)
        {
            animations.Add(additionalTask);
        }

        await Task.WhenAll(animations);

        uiState = newState;
    }

    // Defines which enum values correspond to which UI Group Arrays for RenderState method
    private UIAnimatable[] GetUIElements(UIState state)
    {
        switch (state)
        {
            case UIState.Placing:
                return placingElements;
            case UIState.Scoring:
                return scoringElements;
            case UIState.ScoringKnockout:
                return scoringKnockoutElements;
            case UIState.Lose:
                return loseElements;
            default:
                return new UIAnimatable[0];
        }
    }
}
