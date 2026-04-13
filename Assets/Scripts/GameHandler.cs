using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;
using System.Reflection;

// Handles overarching game systems and flow
public class GameHandler : MonoBehaviour
{
    // --- TYPES ---
    public enum Phase
    {
        Placing,
        Scoring,
    }
    public enum PlayState
    {
        Playing,
        Paused,
        Lost
    }
    public delegate void BugAction(Bug bug);
    
    // --- CONSTANTS ---
    public static Bug.BugInfo[] BugTypes;
    public const int KNOCKOUT_ROUNDS = 20;

    // --- GLOBAL STATE ---
    public static PlayState GameState;
    public static int Round; // starts at 1
    public static Phase CurrentPhase;
    public static int RoundScore;
    public static bool IsKnockout;

    // --- OBJECT REFERENCES ---
    private UIHandler uiHandler;

    // --- PUBLIC METHODS ---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        Init();
    }

    // Initialize game state on startup
    public void Init()
    {
        GetInitialReferences();
        InitializeBugTypes();
        GameState = PlayState.Playing;
        Round = 1;
        StartPlacing();
    }

    // Initiates the placing phase for a round
    public async Task StartPlacing()
    {
        CurrentPhase = Phase.Placing;
        RoundScore = 0;
        IsKnockout = Round % KNOCKOUT_ROUNDS == 0;

        //await placement
        
        this.uiHandler.ShowNextButton();
    }

    // Initiates the scoring phase for a round and handles the flow of state
    // until scoring completes
    public async Task StartScoring() //gets called from button
    {
        CurrentPhase = Phase.Scoring;
        // Reset all bugs for scoring phase
        BroadcastToBugs((bug) => bug.Reset());
        // wait for UI
        await this.uiHandler.StartScoring();
        Bug[] sortedBugs = GetSortedBugs();
        foreach (Bug bug in sortedBugs)
        {
            await bug.Score();
        }
        this.uiHandler.ShowNextButton();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // --- PRIVATE METHODS ---
    // Get all local references to relevant objects for the game handler on startup
    private void GetInitialReferences()
    {
        this.uiHandler = GameObject.Find("UIHandler")?.GetComponent<UIHandler>();
    }

    private void InitializeBugTypes()
    {
        Type bugType = typeof(Bug);
        IEnumerable<Type> bugSubtypes = bugType.Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(bugType) && !t.IsAbstract);
        
    }
    
    // Returns an array of Bug scripts sorted in ascending order of the bug's y position
    // This is the metric used to determine bug scoring order.
    private Bug[] GetSortedBugs()
    {
        Bug[] allBugs = GameObject.FindObjectsByType<Bug>();
        Array.Sort(allBugs, (x, y) => x.transform.position.y.CompareTo(y.transform.position.y));
        return allBugs;
    }

    private void BroadcastToBugs(BugAction action)
    {
        Bug[] allBugs = GameObject.FindObjectsByType<Bug>();
        foreach (Bug bug in allBugs)
        {
            action(bug);
        }
    }
}