using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
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

    }

    // State Changes
    public async Task EnterPlacingState()
    {
        SetupPlacing();
        nextButton.SetActive(false);
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
        
        nextButton.SetActive(false);
    }

    public async Task ShowNextButton()
    {
        nextButton.SetActive(true);
    }

    public async Task EnterLosingState()
    {
        SetupLosing();
    }

    public void OnNextButtonClicked()
    {
        gameHandler.StartScoring();
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
