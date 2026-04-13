using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
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
    public static Dictionary<int, List<Bug.BugInfo>> BugRarityTypes = new Dictionary<int, List<Bug.BugInfo>>();
    public static Dictionary<string, GameObject> LoadedResources = new Dictionary<string, GameObject>();
    public const int KNOCKOUT_ROUNDS = 20;
    public float[] rarityChances = {1};
    private const string BUG_PATH = "Prefabs/Bugs";
    private const float dropY = 6.3f;
    private const float edgeX = 13f;

    // --- GLOBAL STATE ---
    public static PlayState GameState;
    public static int Round; // starts at 1
    public static Phase CurrentPhase;
    public static int RoundScore;
    public static bool IsKnockout;

    // --- OBJECT REFERENCES ---
    [SerializeField] private UIHandler uiHandler;
    private Bug[] allBugs;
    private InputSystem_Actions controls;

    // --- PUBLIC METHODS ---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        Init();
    }

    // Initialize game state on startup
    public void Init()
    {
        InitializeBugTypes();
        GameState = PlayState.Playing;
        Round = 1;
        // Setup control handling
        this.controls = new InputSystem_Actions();
        StartPlacing();
    }

    // Initiates the placing phase for a round
    public async Task StartPlacing()
    {
        CurrentPhase = Phase.Placing;
        RoundScore = 0;
        IsKnockout = Round % KNOCKOUT_ROUNDS == 0;
        await this.uiHandler.EnterPlacingState();

        (GameObject, Bug.BugInfo) bugPair = SpawnRandomBug();
        GameObject bug = bugPair.Item1;
        bug.GetComponent<Rigidbody2D>().simulated = false;
        float safeWidth = edgeX - bugPair.Item2.safeHorizRadius;
        //await placement
        // while (!Mouse.current.leftButton.wasPressedThisFrame)
        while (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            print(this.controls.Player.Drop.IsPressed());
            Vector3 mousePos = (Vector3)Mouse.current.position.ReadValue(); 
            mousePos.z = Camera.main.nearClipPlane;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            bug.transform.position = new Vector3(Mathf.Clamp(worldPosition.x, -safeWidth, safeWidth), dropY);
            await Task.Yield();
        }

        await this.uiHandler.ShowNextButton();
    }

    // Initiates the scoring phase for a round and handles the flow of state
    // until scoring completes
    public async Task StartScoring() //gets called from button
    {
        CurrentPhase = Phase.Scoring;
        // Reset all bugs for scoring phase
        BroadcastToBugs((bug) => bug.Reset());
        // wait for UI
        await this.uiHandler.EnterScoringState();
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

    // Initialize the BugTypes and BugRarityTypes collections, which lists all of the info
    // about every type of bug
    private void InitializeBugTypes()
    {
        Type bugType = typeof(Bug);
        var bugSubtypes = bugType.Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(bugType) && !t.IsAbstract);
        BugTypes = new Bug.BugInfo[bugSubtypes.Count()];
        int i = 0;
        foreach (Type bugSubtype in bugSubtypes)
        {
            Bug.BugInfo result = (Bug.BugInfo)bugSubtype.GetMethod("GetInfo").Invoke(null, null);
            BugTypes[i] = result;
            if (!BugRarityTypes.ContainsKey(result.rarity))
            {
                BugRarityTypes[result.rarity] = new List<Bug.BugInfo>();
            }
            List<Bug.BugInfo> infos = BugRarityTypes[result.rarity];
            infos.Add(result);
            i++;
        }
    }

    private (GameObject, Bug.BugInfo) SpawnRandomBug()
    {
        System.Random rand = new System.Random();
        float value = (float)rand.NextDouble();
        float rarityThreshold = 0f;
        int rarity;
        for (rarity = 0; rarity < rarityChances.Length; rarity++)
        {
            rarityThreshold += rarityChances[rarity];
            if (value < rarityThreshold)
            {
                break;
            }
        }
        print("Spawning bug of rarity: " + rarity + " due to value: " + value);
        List<Bug.BugInfo> bugList = BugRarityTypes[rarity + 1];
        Bug.BugInfo selectedBug = bugList[rand.Next(0, bugList.Count)];
        GameObject bugResource;
        if (LoadedResources.ContainsKey(selectedBug.name))
        {
            bugResource = LoadedResources[selectedBug.name];
        } else
        {
            bugResource = Resources.Load<GameObject>(BUG_PATH + "/" + selectedBug.name);
            LoadedResources[selectedBug.name] = bugResource;
        }
        GameObject createdBug = Instantiate(bugResource as GameObject);
        return (createdBug, selectedBug);
    }
    
    // Returns an array of Bug scripts sorted in ascending order of the bug's y position
    // This is the metric used to determine bug scoring order.
    private Bug[] GetSortedBugs()
    {
        Array.Sort(allBugs, (x, y) => x.transform.position.y.CompareTo(y.transform.position.y));
        return allBugs;
    }

    // Runs action on all bugs currently in scene
    private void BroadcastToBugs(BugAction action)
    {
        foreach (Bug bug in allBugs)
        {
            action(bug);
        }
    }
}