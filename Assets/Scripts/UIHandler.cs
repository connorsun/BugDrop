using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
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
        None,
        Title
    }

    // --- OBJECT REFERENCES ---

    // GameHandler
    [SerializeField] private GameHandler gameHandler;

    // UI Groups
    [SerializeField] private GameObject worldspaceCanvas;

    // UI Elements - General
    [SerializeField] private TextMeshProUGUI roundLabel;
    // TODO: design and add round timeline widget
    [SerializeField] private TextMeshProUGUI lastRoundScore;
    [SerializeField] private TextMeshProUGUI roundFutureThreshold;
    [SerializeField] private TextMeshProUGUI roundScoreNumber;
    [SerializeField] private TextMeshProUGUI roundScoreLabel;
    [SerializeField] private RectTransform tooltipRectTransform;
    [SerializeField] private TextMeshProUGUI currentBugTooltipTitle;
    [SerializeField] private TextMeshProUGUI currentBugTooltipDescription;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject modeButtons;

    // UI Elements - Knockout
    [SerializeField] private TextMeshProUGUI roundScoreNumberKnockout;
    // TODO: design and add progress bar
    [SerializeField] private TextMeshProUGUI thresholdLabel;
    [SerializeField] private GameObject fastForward;

    // UI Elements - Lose Screen
    [SerializeField] private TextMeshProUGUI loseTitle;
    [SerializeField] private TextMeshProUGUI roundLabelLosing;
    [SerializeField] private TextMeshProUGUI scoreDifference;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject quitButton;

    // Knockout Bar
    [SerializeField] private KnockoutProgressBar[] knockoutProgressBar;

    // Intro Cutscene
    [SerializeField] private TextMeshProUGUI introCutsceneDialogue;
    [SerializeField] private TextMeshProUGUI introCutsceneSpeaker;
    [SerializeField] private UIAnimatable skipButton;
    [SerializeField] private ToggleGroupController tg;

    // UI Groups
    [SerializeField] private UIAnimatable[] placingElements;
    [SerializeField] private UIAnimatable[] scoringElements;
    [SerializeField] private UIAnimatable[] scoringKnockoutElements;
    [SerializeField] private UIAnimatable[] loseElements;
    [SerializeField] private UIAnimatable[] titleElements;

    

    private UIState uiState = UIState.None;
    private float lerpScore;
    private const float LERP_SOUND_THRESH_RATIO = 0.05f;
    private float lerpSoundThreshold;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        //Init();
    }

    public void Init()
    {
        SetRoundLabel();
        SetFutureThreshold();
        SetScoreState();
    }

    // -- OVERARCHING UI STATE CHANGES --

    public async Task EnterPlacingState()
    {
        SetRoundLabel();
        SetFutureThreshold();
        SetLastRoundScore();
        
        tg.SetSelection(0);
    
        await RenderState(UIState.Placing, HideNextButton());
    }

    public async Task EnterScoringState()
    {
        lerpScore = 0;
        SetScoreState();
        if (GameHandler.IsKnockout)
        {
            LockProgressBar(false);
            await RenderState(UIState.ScoringKnockout, HideNextButton());
        } else
        {
            await RenderState(UIState.Scoring, HideNextButton());
        }
    }

    public async Task EnterLosingState()
    {
        UpdateLoseState();
        await RenderState(UIState.Lose, HideNextButton());
    }

    public async Task EnterTitleScreen()
    {
        await RenderState(UIState.Title);
    }

    public void SetIntroCutsceneLine(string dialogue, string speaker)
    {
        introCutsceneDialogue.text = dialogue;
        introCutsceneSpeaker.text = speaker;
    }

    public async Task NextIntroCutsceneLine(string dialogue, string speaker)
    {
        UIAnimatable dialogueAnimatable = introCutsceneDialogue.GetComponent<UIAnimatable>();
        UIAnimatable speakerAnimatable = introCutsceneSpeaker.GetComponent<UIAnimatable>();
        await Task.WhenAll(
            dialogueAnimatable.Hide(),
            speakerAnimatable.Hide()
        );
        introCutsceneDialogue.text = dialogue;
        introCutsceneSpeaker.text = speaker;
        await Task.WhenAll(
            dialogueAnimatable.Show(),
            speakerAnimatable.Show()
        );
    }

    // -- SETTERS/LOGIC FOR INDIVIDUAL ELEMENTS --

    // Round Label
    public void SetRoundLabel()
    {
        roundLabel.text = "Round " + GameHandler.Round;
    }

    public void SetLastRoundScore()
    {
        lastRoundScore.text = GameHandler.LastRoundScore + " points last round";
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
        GameHandler.PlaySound("Whoosh");
        await nextButton.GetComponent<UIAnimatable>().Show();
    }

    public async Task HideNextButton()
    {
        GameHandler.PlaySound("Whoosh");
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
        GameObject mp = GameObject.Find("MusicPlayer");
        if (mp != null)
        {
            mp.GetComponent<MusicHandler>().Unlose();
        }
        GameHandler.BroadcastToBugs((Bug bug) => bug.Destroy());
        GameHandler.AllBugs = new Bug[0];
        //print(GameHandler.AllBugs.Length);
        gameHandler.Init();
    }

    public void OnQuitButtonClicked()
    {
        GameObject mp = GameObject.Find("MusicPlayer");
        GameHandler.Controls.Player.Drop.performed -= GameHandler.SingletonGameHandler.OnDrop;
        GameHandler.Controls.Player.Disable();
        if (mp != null)
        {
            mp.GetComponent<MusicHandler>().Unlose();
        }
        GameHandler.AllBugs = new Bug[0];
        //print(GameHandler.AllBugs.Length);
        gameHandler.ResetGlobals();
        SceneManager.LoadScene("Title Screen");
        //Application.Quit();
    }

    public void OnFastForwardButtonClicked()
    {
        GameHandler.FastForward = !GameHandler.FastForward;
        if (GameHandler.FastForward)
        {
            GameHandler.GameSpeed = GameHandler.FAST_GAME_SPEED;
        } else
        {
            GameHandler.GameSpeed = GameHandler.DefaultGameSpeed;
        }
    }

    public void OnPlacePaletteClicked()
    {
        if (GameHandler.PlacingMode != GameHandler.PlaceMode.Placing) {
            _ = ShowCurrentBugTooltip();
        }
        GameHandler.PlacingMode = GameHandler.PlaceMode.Placing;
        tg.SetSelection(0);
        if (GameHandler.MovingBug != null) {
            GameHandler.MovingBug.Destroy();
            GameHandler.MovingBug = null;
            GameHandler.OriginalMovingBug.Hover(false, 0f, false);
        }
    }

    public void OnMovePaletteClicked()
    {
        if (GameHandler.PlacingMode == GameHandler.PlaceMode.Placing) {
            _ = HideCurrentBugTooltip();
        }
        GameHandler.PlacingMode = GameHandler.PlaceMode.Moving;
        tg.SetSelection(1);
    }

    public void OnDeletePaletteClicked()
    {
        if (GameHandler.PlacingMode == GameHandler.PlaceMode.Placing) {
            _ = HideCurrentBugTooltip();
        }
        GameHandler.PlacingMode = GameHandler.PlaceMode.Deleting;
        tg.SetSelection(2);
        if (GameHandler.MovingBug != null) {
            GameHandler.MovingBug.Destroy();
            GameHandler.MovingBug = null;
            GameHandler.OriginalMovingBug.Hover(false, 0f, false);
        }
    }

    public void LockProgressBar(bool locked)
    {
        foreach (KnockoutProgressBar element in knockoutProgressBar)
        {
            element.SetAllowedToMove(!locked);
        }
    }

    // Score UI
    public void UpdateScoreState()
    {
        StopAllCoroutines();
        StartCoroutine(LerpScore((int)GameHandler.RoundScore));
    }

    public void SetScoreState()
    {
        lerpScore = (int)GameHandler.RoundScore;
        lerpSoundThreshold = lerpScore;
        roundScoreNumber.text = (int)GameHandler.RoundScore + "";
        roundScoreNumberKnockout.text = (int)GameHandler.RoundScore + "";
        thresholdLabel.text = GameHandler.ScoreThreshold + "";
    }

    // Lose UI
    public void UpdateLoseState()
    {
        roundLabelLosing.text = "Round " + GameHandler.Round;
        scoreDifference.text = "Score: " + (int)GameHandler.RoundScore + " Needed: " + GameHandler.ScoreThreshold;
    }

    public void SetCurrentBugTooltip(Bug.BugInfo bugInfo)
    {
        currentBugTooltipTitle.text = bugInfo.name;
        currentBugTooltipDescription.text = "[Base " + bugInfo.baseScore + (bugInfo.baseScore == 1 ? " point] " : " points] ") + bugInfo.tooltip;
        currentBugTooltipDescription.ForceMeshUpdate(true);
        int lineCount = currentBugTooltipDescription.textInfo.lineCount;
        tooltipRectTransform.sizeDelta = new Vector2(tooltipRectTransform.sizeDelta.x, 25 + (lineCount * 8));
    }

    public void ButtonHover()
    {
        GameHandler.PlaySound("Button Hover");
    }

    public void ButtonPress()
    {
        GameHandler.PlaySound("Button Press");
    }

    public async Task ShowCurrentBugTooltip()
    {
        GameHandler.PlaySound("Whoosh");
        await tooltipRectTransform.gameObject.GetComponent<UIAnimatable>().Show();
    }

    public async Task HideCurrentBugTooltip()
    {
        await tooltipRectTransform.gameObject.GetComponent<UIAnimatable>().Hide();
    }

    public async Task HideModeButtons()
    {
        GameHandler.PlaySound("Whoosh");
        await modeButtons.GetComponent<UIAnimatable>().Hide();
    }

    public async Task ShowSkipButton()
    {
        await skipButton.Show();
    }

    // -- INSTANTIATE WORLD SPACE UI --
    public void CreateScoreGraphic(Vector3 worldPos, int score, bool isPrimary)
    {
        try {
            GameObject scoreGraphic = Instantiate(GameHandler.GetResource("Prefabs/UI/ScoreGraphic") as GameObject);
            scoreGraphic.transform.SetParent(worldspaceCanvas.transform, false);

            scoreGraphic.transform.position = worldPos;

            scoreGraphic.GetComponent<UIAnimatable>().SetOriginalPosition(
                scoreGraphic.GetComponent<RectTransform>().anchoredPosition
            );

            ScoreGraphic sg = scoreGraphic.GetComponent<ScoreGraphic>();
            sg.SetText(score);
            sg.SetColor(isPrimary);

            //print(scoreText.color);
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
    private async Task RenderState(UIState newState, params Task[] additionalTasks)
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

        if (additionalTasks != null)
        {
            animations.AddRange(additionalTasks);
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
            case UIState.Title:
                return titleElements;
            default:
                return new UIAnimatable[0];
        }
    }

    // Score lerping
    private IEnumerator LerpScore(float target)
    {
        float threshDiff = Mathf.Max(1f, GameHandler.ScoreThreshold * LERP_SOUND_THRESH_RATIO);
        float diff = Mathf.Abs(target - lerpScore);
        float speed = Mathf.Max(8f /*MIN LERP SPEED*/, diff * 2f);

        while (Mathf.Abs(lerpScore - target) > 0.01f)
        {
            lerpScore = Mathf.Lerp(lerpScore, target, Time.deltaTime * speed);
            if (lerpScore >= lerpSoundThreshold + threshDiff)
            {
                GameHandler.PlaySound("ScoreLerp");
                lerpSoundThreshold = lerpScore;
            }
            roundScoreNumber.text = Mathf.CeilToInt(lerpScore) + "";
            roundScoreNumberKnockout.text = Mathf.CeilToInt(lerpScore) + "";
            thresholdLabel.text = GameHandler.ScoreThreshold + "";

            yield return null;
        }
    }
}
