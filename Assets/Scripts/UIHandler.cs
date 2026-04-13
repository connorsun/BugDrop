using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

// Provides methods for rendering UI changes based on state in GameHandler
public class UIHandler : MonoBehaviour
{
    // --- OBJECT REFERENCES ---

    // UI Groups
    [SerializeField] private GameObject generalGroup;
    [SerializeField] private GameObject knockoutGroup;
    [SerializeField] private GameObject knockoutDisable; // General UI elements to hide when knockout enabled
    [SerializeField] private GameObject loseGroup;

    // UI Elements - General
    [SerializeField] private TextMeshProUGUI roundLabel;
    // TODO: design and add round timeline widget
    [SerializeField] private TextMeshProUGUI roundFutureThreshold;
    [SerializeField] private TextMeshProUGUI roundScoreNumber;
    [SerializeField] private TextMeshProUGUI roundScoreLabel;
    [SerializeField] private Button nextButton;

    // UI Elements - Knockout
    [SerializeField] private TextMeshProUGUI roundScoreNumberKnockout;
    // TODO: design and add progress bar
    [SerializeField] private TextMeshProUGUI thresholdLabel;

    // UI Elements - Lose Screen
    [SerializeField] private TextMeshProUGUI loseTitle;
    [SerializeField] private TextMeshProUGUI roundLabelLosing;
    [SerializeField] private TextMeshProUGUI scoreDifference;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        Init();
    }

    // Initialize UI state on startup
    public void Init()
    {
        generalGroup.SetActive(true);
        knockoutGroup.SetActive(false);
        knockoutDisable.SetActive(true);
        loseGroup.SetActive(false);
    }

    public async Task StartPlacing()
    {
        
    }

    public async Task StartScoring()
    {
        
    }

    public void ShowNextButton()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // --- PRIVATE METHODS ---

    
}
