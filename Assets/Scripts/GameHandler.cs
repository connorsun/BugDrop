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
    public static GameHandler SingletonGameHandler;
    public static UIHandler SingletonUIHandler;
    public static Bug.BugInfo[] BugTypes;
    public static Dictionary<int, List<Bug.BugInfo>> BugRarityTypes = new Dictionary<int, List<Bug.BugInfo>>();
    public static Dictionary<string, GameObject> LoadedResources = new Dictionary<string, GameObject>();
    public static Bug[] AllBugs;
    public const int KNOCKOUT_ROUNDS = 3;
    [SerializeField] private float[] rarityChances = {0.75f, 0.25f};
    public const int THRESHOLD_BASE = 1;
    public const float THRESHOLD_SCALE = 1.1;
    private const string BUG_PATH = "Prefabs/Bugs";
    private const float dropY = 6.3f;
    private const float edgeX = 12.5f;
    private Vector3 zapperPos = new Vector3(0f, -7.5f, 0f);
    

    // --- GLOBAL STATE ---
    public static PlayState GameState;
    public static int Round; // starts at 1
    public static Phase CurrentPhase;
    public static int RoundScore;
    public static int ScoreThreshold;
    public static bool IsKnockout;
    public static float GameSpeed;

    // --- OBJECT REFERENCES ---
    [SerializeField] private UIHandler uiHandler;
    private InputSystem_Actions controls;

    // --- PRIVATE STATE ---
    private bool trackingBug;

    // --- PUBLIC METHODS ---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        // Setup control handling
        this.controls = new InputSystem_Actions();
        this.controls.Player.Drop.performed += OnDrop;
        this.controls.Player.Enable();
        Init();
    }

    // Initialize game state on startup
    public void Init()
    {
        AllBugs = new Bug[0];
        LoadedResources = new Dictionary<string, GameObject>();
        SingletonGameHandler = this;
        SingletonUIHandler = uiHandler;
        InitializeBugTypes();
        GameState = PlayState.Playing;
        Round = 0;
        ScoreThreshold = THRESHOLD_BASE;
        GameSpeed = 1;
        RoundScore = 0;
        uiHandler.Init();
        _ = StartPlacing();
    }

    // Initiates the placing phase for a round
    public async Task StartPlacing()
    {
        Round++;
        CurrentPhase = Phase.Placing;
        RoundScore = 0;
        uiHandler.UpdateScoreState();
        IsKnockout = Round % KNOCKOUT_ROUNDS == 0;
        await this.uiHandler.EnterPlacingState();

        (GameObject, Bug.BugInfo) bugPair = SpawnRandomBug();
        AllBugs = FindObjectsByType<Bug>(FindObjectsSortMode.None);
        GameSpeed = 1 + AllBugs.Length * 0.04f;
        GameObject bug = bugPair.Item1;
        bug.GetComponent<Bug>().SetSimulated(false);
        float safeWidth = edgeX - bugPair.Item2.safeHorizRadius;
        this.trackingBug = true;
        //await placement
        // while (!Mouse.current.leftButton.wasPressedThisFrame)
        while (trackingBug)
        {
            Vector3 mousePos = (Vector3)Mouse.current.position.ReadValue(); 
            mousePos.z = Camera.main.nearClipPlane;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            bug.transform.position = new Vector3(Mathf.Clamp(worldPosition.x, -safeWidth, safeWidth), dropY);
            await Task.Yield();
        }
        bug.GetComponent<Bug>().SetSimulated(true);
        // give the bug some time to start dropping
        await Task.Delay(TimeSpan.FromSeconds(0.2f));
        // wait until all bugs are stationary
        BroadcastToBugs((Bug bug) => bug.StartPlacing());
        while (true) {
            try {
            print(AllBugs.Length);
            bool allStationary = true;
            foreach (Bug eachBug in AllBugs)
            {
                // cannot break for efficiency - need to check IsStationary on all bugs so all bugs do their frame checks
                if (!eachBug.IsStationary())
                {
                    allStationary = false;
                }
            }
            if (allStationary)
            {
                break;
            }
            } catch (Exception e)
            {
                Debug.LogError(e);
            }
            await Task.Yield();
        }
        await this.uiHandler.ShowNextButton();
    }

    // Handles Drop input action
    private void OnDrop(InputAction.CallbackContext context)
    {
        trackingBug = false;
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
        Bug[] sortedBugs = GetClosestBugs();
        if (sortedBugs.Length > 0)
        {
            await sortedBugs[0].Trigger(true, zapperPos);
        }
        float timestamp = Time.unscaledTime;
        while (Time.unscaledTime < timestamp + 0.5f)
        {
            await Task.Yield();
        }
        // check for lose condition
        if (IsKnockout && RoundScore < ScoreThreshold)
        {
            await uiHandler.EnterLosingState();
            return;
        }
        await this.uiHandler.ShowNextButton();
        if (IsKnockout)
        {
            // set up for next knockout round
            ScoreThreshold = (int)(ScoreThreshold * THRESHOLD_SCALE);
        }
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
        List<Bug.BugInfo> bugList = BugRarityTypes[rarity + 1];
        Bug.BugInfo selectedBug = bugList[rand.Next(0, bugList.Count)];
        GameObject createdBug = Instantiate(GetResource(BUG_PATH + "/" + selectedBug.name) as GameObject);
        createdBug.transform.localScale = new Vector3(UnityEngine.Random.value > 0.5f ? -1 : 1, 
                createdBug.transform.localScale.y, createdBug.transform.localScale.z);
        return (createdBug, selectedBug);
    }
    
    // Returns an array of Bug scripts sorted in closest order to zap pos
    // This is the metric used to determine bug scoring order.
    private Bug[] GetClosestBugs()
    {
        return GameHandler.AllBugs.OrderBy(x => (x.transform.position - zapperPos).magnitude).ToArray();
    }
    // --- STATIC HELPERS ---

    // Runs action on all bugs currently in scene
    public static void BroadcastToBugs(BugAction action)
    {
        foreach (Bug bug in AllBugs)
        {
            action(bug);
        }
    }
    public static GameObject GetResource(string path)
    {
        GameObject resource;
        if (LoadedResources.ContainsKey(path))
        {
            resource = LoadedResources[path];
        } else
        {
            resource = Resources.Load<GameObject>(path);
            LoadedResources[path] = resource;
        }
        return resource;
    }
}