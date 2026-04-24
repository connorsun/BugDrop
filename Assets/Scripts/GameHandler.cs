using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
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
    public enum PlaceMode
    {
        Placing,
        Moving,
        Deleting
    }
    public delegate void BugAction(Bug bug);
    
    // --- CONSTANTS ---
    public const bool BUILD_FLAG = false;
    public static GameHandler SingletonGameHandler;
    public static UIHandler SingletonUIHandler;
    public static AudioSource SingletonSFXSource;
    public static AudioSource SingletonPitchedSource;
    public static GameObject SingletonCircleIndicator;
    public static Bug.BugInfo[] BugTypes;
    public static Dictionary<int, List<Bug.BugInfo>> BugRarityTypes = new Dictionary<int, List<Bug.BugInfo>>();
    public static Dictionary<string, GameObject> LoadedResources = new Dictionary<string, GameObject>();
    public static Dictionary<string, Sound> LoadedSounds = new Dictionary<string, Sound>();
    public static Bug[] AllBugs;
    public const int KNOCKOUT_ROUNDS = 5;
    public static Dictionary<int, float[]> RoundRarityChances = new Dictionary<int, float[]>
    {
        [0] = new[]{1f},
        [10] = new[]{0.5f, 0.5f},
        [20] = new[]{0.33f, 0.33f, 0.33f},
        [25] = new[]{0.33f, 0.33f, 0.31f, 0.02f}
    };
    public static float[] CurrentRarityChances = {1f};
    public const int THRESHOLD_BASE = 10;
    public const float THRESHOLD_SCALE = 1.6f;
    public const float MOUSE_DETECTION_RADIUS = 0.05f;
    private const string BUG_PATH = "Prefabs/Bugs";
    public const float FAST_GAME_SPEED = 0.25f;
    private const float DROP_Y = 6.3f;
    private const float EDGE_X = 11.5f;
    private const float SCORE_PITCH_BASE = 1f;
    private const float SCORE_PITCH_MAX = 4f;

    private const float SCORE_PITCH_INCR = 0.1f;
    public static Vector3 ZapperPos = new Vector3(0f, -7f, 0f);
    public static Color PRIMARY_COLOR = new Color(255f / 255f, 240f / 255f, 137f / 255f);
    public static Color SECONDARY_COLOR = new Color(115f / 255f, 239f / 255f, 232f / 255f);
    

    // --- GLOBAL STATE ---
    public static PlayState GameState;
    public static int Round; // starts at 1
    public static Phase CurrentPhase;
    public static float RoundScore;
    public static float LastRoundScore;
    public static int ScoreThreshold;
    public static bool IsKnockout;
    public static bool FastForward;
    public static float DefaultGameSpeed;
    public static float GameSpeed;
    public static PlaceMode PlacingMode;
    public static Bug MovingBug;
    public static Bug OriginalMovingBug;
    public static float ScorePitch;
    public static HashSet<string> SoundsThisFrame = new HashSet<string>();


    // --- OBJECT REFERENCES ---
    [SerializeField] private UIHandler uiHandler;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource pitchedAudioSource;
    [SerializeField] private GameObject circleIndicator;
    [SerializeField] private GameObject[] placingClickExcludeButtons;
    private InputSystem_Actions controls;

    // --- PRIVATE STATE ---
    private bool trackingBug;
    private float movingSafeWidth;
    private Bug.BugInfo selectedBug;
    private GameObject placingBug;

    // --- PUBLIC METHODS ---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        // Setup control handling
        this.controls = new InputSystem_Actions();
        this.controls.Player.Drop.performed += OnDrop;
        this.controls.Player.Enable();
        GameSpeed = 1;
        FastForward = false;
        InitializeBugTypes();
        Init();
    }

    // Initialize game state on startup
    public void Init()
    {
        AllBugs = new Bug[0];
        LoadedResources = new Dictionary<string, GameObject>();
        SingletonGameHandler = this;
        SingletonUIHandler = uiHandler;
        SingletonSFXSource = audioSource;
        SingletonPitchedSource = pitchedAudioSource;
        SingletonCircleIndicator = circleIndicator;
        GameState = PlayState.Playing;
        Round = 0;
        ScoreThreshold = THRESHOLD_BASE;
        DefaultGameSpeed = 1;
        RoundScore = 0;
        LastRoundScore = 0;
        uiHandler.Init();
        _ = StartPlacing();
    }

    // Initiates the placing phase for a round
    public async Task StartPlacing()
    {
        if (IsKnockout && Round > 0)
        {
            // set up for next knockout round
            // ScoreThreshold = (int)Mathf.Round(Mathf.Pow((float)Round/KNOCKOUT_ROUNDS + Mathf.Sqrt(10), 2));
            ScoreThreshold = (int)(ScoreThreshold * THRESHOLD_SCALE);
        }
        BroadcastToBugs((Bug bug) => bug.Reset());
        PlacingMode = PlaceMode.Placing;
        Round++;
        int rarityRound = 0;
        foreach (int roundNum in RoundRarityChances.Keys)
        {
            if (roundNum > rarityRound && roundNum <= Round)
            {
                rarityRound = roundNum;
            }
        }
        CurrentRarityChances = RoundRarityChances[rarityRound];
        CurrentPhase = Phase.Placing;
        LastRoundScore = RoundScore;
        RoundScore = 0;
        IsKnockout = Round % KNOCKOUT_ROUNDS == 0;
        try {
        selectedBug = PickRandomBug();
        this.uiHandler.SetCurrentBugTooltip(selectedBug);
        await this.uiHandler.EnterPlacingState();
        uiHandler.SetScoreState();

        // Spawn next bug in
        placingBug = Instantiate(GetResource(BUG_PATH + "/" + selectedBug.name) as GameObject);
        placingBug.transform.localScale = new Vector3(UnityEngine.Random.value > 0.5f ? -1 : 1, 
        placingBug.transform.localScale.y, placingBug.transform.localScale.z);
        AllBugs = FindObjectsByType<Bug>(FindObjectsSortMode.None);

        // 1 - 0.6/(1+e^(4-0.4x))
        // DefaultGameSpeed = 1 - 0.6f / (1 + Mathf.Exp(4 - AllBugs.Length * 0.2f));
        // 1 - 0.5arctan(x/10)
        DefaultGameSpeed = 1 - Mathf.Atan(AllBugs.Length/10f)/2f;
        // print("Game Speed: " + DefaultGameSpeed);
        placingBug.GetComponent<Bug>().SetSimulated(false);
        float safeWidth = EDGE_X - selectedBug.safeHorizRadius;
        this.trackingBug = true;

        //await placement
        // while (!Mouse.current.leftButton.wasPressedThisFrame)
        MovingBug = null;
        OriginalMovingBug = null;
        while (trackingBug)
        {
            Vector3 worldPosition = GetMouseWorldPos();
            if (PlacingMode == PlaceMode.Placing) {
                placingBug.SetActive(true);
                placingBug.transform.position = new Vector3(Mathf.Clamp(worldPosition.x, -safeWidth, safeWidth), DROP_Y);
            } else if (PlacingMode == PlaceMode.Moving)
            {
                placingBug.SetActive(false);
                if (MovingBug != null) {
                    MovingBug.transform.position = new Vector3(Mathf.Clamp(worldPosition.x, -safeWidth, safeWidth), DROP_Y);
                }
            } else
            {
                placingBug.SetActive(false);
            }
            await Task.Yield();
        }
        if (PlacingMode == PlaceMode.Placing) {
            placingBug.GetComponent<Bug>().SetSimulated(true);
        } else if (PlacingMode == PlaceMode.Moving)
        {
            placingBug.GetComponent<Bug>().Destroy();
            OriginalMovingBug.Destroy();
            MovingBug.GetComponent<Bug>().SetSimulated(true);
            AllBugs = FindObjectsByType<Bug>(FindObjectsSortMode.None);
        }
        _ = this.uiHandler.HideCurrentBugTooltip();
        _ = this.uiHandler.HideModeButtons();
        // give the bug some time to start dropping
        await Task.Delay(TimeSpan.FromSeconds(0.2f));
        // wait until all bugs are stationary
        BroadcastToBugs((Bug bug) => bug.StartPlacing());
        while (true) {
            //print(AllBugs.Length);
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
            await Task.Yield();
        }
        await this.uiHandler.ShowNextButton();
        } catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    // Handles Drop input action
    private void OnDrop(InputAction.CallbackContext context)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        // ensure you did not just click a button
        foreach (RaycastResult res in results) 
        {
            if (this.placingClickExcludeButtons.Contains(res.gameObject))
            {
                return;
            }
        }
        if (PlacingMode == PlaceMode.Placing) {
            trackingBug = false;
        } else if (PlacingMode == PlaceMode.Moving)
        {
            if (MovingBug == null)
            {
                List<Collider2D> overlapColliders = new List<Collider2D>();
                Vector3 mouseWorldPos = GameHandler.GetMouseWorldPos();
                Physics2D.OverlapCircle(mouseWorldPos, MOUSE_DETECTION_RADIUS, ContactFilter2D.noFilter, overlapColliders);
                foreach (Collider2D col in overlapColliders)
                {
                    Bug bug = col.gameObject?.GetComponentInParent<Bug>();
                    if (bug != null)
                    {
                        OriginalMovingBug = bug;
                        GameObject movingBug = Instantiate(bug.gameObject as GameObject);
                        MovingBug = movingBug.GetComponent<Bug>();
                        for (int i = 0; i < MovingBug.positions.Length; i++)
                        {
                            MovingBug.segments[i].transform.localPosition = bug.positions[i];
                            MovingBug.positions[i] = bug.positions[i];
                            MovingBug.segments[i].transform.rotation = bug.rotations[i];
                            MovingBug.rotations[i] = bug.rotations[i];
                            // HingeJoint2D hinge = MovingBug.segments[i].GetComponent<HingeJoint2D>();
                            // if (hinge != null)
                            // {
                            //     var angles = hinge.limits;
                            //     Destroy(hinge);
                            //     HingeJoint2D newHinge = MovingBug.segments[i].AddComponent<HingeJoint2D>();
                            //     newHinge.limits = angles;
                            // }
                        }
                        MovingBug.Start();
                        MovingBug.SetSimulated(false);
                        this.movingSafeWidth = EDGE_X - MovingBug.thisBugInfo.safeHorizRadius;
                        bug.Hover(true, 0.85f, false);
                        break;
                    }
                }
            } else
            {
                trackingBug = false;
            }
        } else
        {
            
            List<Collider2D> overlapColliders = new List<Collider2D>();
            Vector3 mouseWorldPos = GameHandler.GetMouseWorldPos();
            Physics2D.OverlapCircle(mouseWorldPos, MOUSE_DETECTION_RADIUS, ContactFilter2D.noFilter, overlapColliders);
            foreach (Collider2D col in overlapColliders)
            {
                Bug bug = col.gameObject?.GetComponentInParent<Bug>();
                if (bug != null)
                {
                    bug.Destroy();
                    AllBugs = FindObjectsByType<Bug>(FindObjectsSortMode.None);
                    trackingBug = false;
                    break;
                }
            }
        }
    }

    // Initiates the scoring phase for a round and handles the flow of state
    // until scoring completes
    public async Task StartScoring() //gets called from button
    {
        CurrentPhase = Phase.Scoring;
        // Set all bugs to scoring phase
        BroadcastToBugs((bug) => bug.StartScoring());
        // wait for UI
        await this.uiHandler.EnterScoringState();
        ScorePitch = SCORE_PITCH_BASE;
        Bug[] sortedBugs = GetClosestBugs();
        if (sortedBugs.Length > 0)
        {
            await sortedBugs[0].Trigger(true, ZapperPos, 0);
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
        this.uiHandler.LockProgressBar(true);
        await this.uiHandler.ShowNextButton();
    }


    // Update is called once per frame
    void Update()
    {
        SoundsThisFrame = new HashSet<string>();
        if (!BUILD_FLAG) {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && this.trackingBug && this.placingBug != null)
            {
                // cycle bug
                foreach (int rarity in BugRarityTypes.Keys)
                {
                    int index = BugRarityTypes[rarity].IndexOf(this.selectedBug);
                    if (index != -1)
                    {
                        this.placingBug.GetComponent<Bug>().Destroy();
                        if (index == BugRarityTypes[rarity].Count - 1)
                        {
                            this.selectedBug = BugRarityTypes[rarity % BugRarityTypes.Keys.Count + 1][0];
                        } else
                        {
                            this.selectedBug = BugRarityTypes[rarity][index + 1];
                        }
                        this.placingBug = Instantiate(GetResource(BUG_PATH + "/" + this.selectedBug.name) as GameObject);
                        this.placingBug.transform.localScale = new Vector3(UnityEngine.Random.value > 0.5f ? -1 : 1, 
                        this.placingBug.transform.localScale.y, this.placingBug.transform.localScale.z);
                        AllBugs = FindObjectsByType<Bug>(FindObjectsSortMode.None);
                        placingBug.GetComponent<Bug>().SetSimulated(false);
                        float safeWidth = EDGE_X - this.selectedBug.safeHorizRadius;
                        this.uiHandler.SetCurrentBugTooltip(this.selectedBug);
                        break;
                    }
                }
            }
        }
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

    private Bug.BugInfo PickRandomBug()
    {
        System.Random rand = new System.Random();
        float value = (float)rand.NextDouble();
        float rarityThreshold = 0f;
        int rarity;
        //print("Random value: " + value);
        for (rarity = 0; rarity < CurrentRarityChances.Length; rarity++)
        {
            rarityThreshold += CurrentRarityChances[rarity];
            //print(rarityThreshold);
            if (rarityThreshold > value)
            {
                break;
            }
        }
        List<Bug.BugInfo> bugList = BugRarityTypes[rarity + 1];
        Bug.BugInfo selectedBug = bugList[rand.Next(0, bugList.Count)];
        return selectedBug;
    }
    
    // Returns an array of Bug scripts sorted in closest order to zap pos
    // This is the metric used to determine bug scoring order.
    private Bug[] GetClosestBugs()
    {
        return GameHandler.AllBugs.OrderBy(x => (x.center.position - ZapperPos).magnitude).ToArray();
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

    public static Sound GetSound(string path)
    {
        Sound resource;
        if (LoadedSounds.ContainsKey(path))
        {
            resource = LoadedSounds[path];
        } else
        {
            resource = Resources.Load<Sound>(path);
            LoadedSounds[path] = resource;
        }
        return resource;
    }
    public static void PlaySound(string sound)
    {
        Sound s = GetSound("Sounds/" + sound);
        if (sound.StartsWith("Score "))
        {
            if (SoundsThisFrame.Contains("Score Sound"))
            {
                return;
            }
            SoundsThisFrame.Add("Score Sound");
            SingletonPitchedSource.pitch = ScorePitch;
            if (ScorePitch < SCORE_PITCH_MAX) {
                ScorePitch += SCORE_PITCH_INCR * (1/1.4f - Mathf.Atan(ScorePitch - 2f)/2.8f);
            }
            SingletonPitchedSource.PlayOneShot(s.clip);
        } else {
            if (SoundsThisFrame.Contains(sound))
            {
                return;
            }
            SoundsThisFrame.Add(sound);
            SingletonSFXSource.pitch = 1;
            SingletonSFXSource.PlayOneShot(s.clip);
        }
    }

    public static Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = (Vector3)Mouse.current.position.ReadValue(); 
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}