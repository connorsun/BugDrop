using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;


public class Bee : Bug
{
    // --- CONSTANTS ---
    private const float RAY_DIST = 0.5f;
    private const float RAY_MID_DIST = 0.8f;
    // --- OBJECT REFERENCES --- 
    private Bug cachedAffectedBug;
    // collider of the single bee obj
    [SerializeField] private Collider2D col;

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Bee", 2, 3, 0.8f, 0.5f, "Honeys bug directly below, giving it x2.5 score");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }

    public override float CalculateOverallScore()
    {
        return this.baseScore * this.multiplier;
    }
    
    public override async Task Hover(bool on, float intensity, bool affectOthers)
    {
        base.Hover(on, intensity, affectOthers);
        if (affectOthers) {
            if (this.cachedAffectedBug != null)
            {
                this.cachedAffectedBug.Hover(on, -100f, false);
            } else
            {
                Bug affectedBug = GetBeeAffectedBug();
                if (affectedBug != null) {
                    affectedBug.Hover(on, -100f, false);
                }
            }
        }
    }

    // debugging
    // private async Task DrawRay(float xpos, float ypos, float dist)
    // {
    //     for (int i = 0; i < 200; i++) {
    //         Debug.DrawRay(new Vector3(xpos, ypos, 0f), Vector2.down * dist, Color.green);
    //         await Task.Yield();
    //     }
    // }

    private Bug GetBeeAffectedBug()
    {
        Bounds bounds = col.bounds;
        Bug[] bugsFound = new Bug[3];
        Vector2[] contactPoints = new Vector2[3];
        int i = 0;
        foreach (float xpos in new float[]{bounds.min.x, bounds.center.x, bounds.max.x}) {
            // halfway between mid and bottom
            float ypos = (bounds.min.y + bounds.center.y)/2f;
            List<RaycastHit2D> rayHits = new List<RaycastHit2D>();
            float dist = RAY_DIST;
            if (i == 1)
            {
                dist = RAY_MID_DIST;
            }
            Physics2D.Raycast(new Vector3(xpos, ypos, 0f), Vector2.down, ContactFilter2D.noFilter, rayHits, dist);
            foreach (RaycastHit2D rayHit in rayHits)
            {
                Bug otherBug = rayHit.collider?.gameObject?.GetComponentInParent<Bug>();
                if (otherBug != null && otherBug != this)
                {
                    bugsFound[i] = otherBug;
                    contactPoints[i] = rayHit.point;
                }
                if (otherBug != this)
                {
                    break;
                }
            }
            i++;
        }
        // logic to determine which bug to pick
        print(bugsFound);
        Dictionary<Bug, int> bugCount = new Dictionary<Bug, int>();
        foreach (Bug bug in bugsFound)
        {
            if (bug != null) {
                bugCount[bug] = bugCount.GetValueOrDefault(bug) + 1;
            }
        }
        int highest = -1;
        Bug highestBug = null;
        foreach (Bug bug in bugCount.Keys)
        {
            if (bugCount[bug] == highest)
            {
                // must mean multiple bugs have 1 raycast point
                highestBug = null;
                break;
            }
            if (bugCount[bug] > highest)
            {
                highest = bugCount[bug];
                highestBug = bug;
            }
        }
        if (highestBug != null)
        {
            // return bug with highest raycast hits
            return highestBug;
        }
        // if tie, pick middle
        if (bugsFound[1] != null)
        {
            return bugsFound[1];
        }
        if (bugsFound[0] == null)
        {
            return bugsFound[2];
        }
        if (bugsFound[2] == null)
        {
            return bugsFound[0];
        }
        // return bug with highest contact hit position
        return contactPoints[0].y > contactPoints[2].y? bugsFound[0] : bugsFound[2];
    }

    public override void StartScoring()
    {
        base.StartScoring();
        Bug affectedBug = GetBeeAffectedBug();
        if (affectedBug == null)
        {
            // no bug to honey
            return;
        }
        if (!affectedBug.effects.Contains(Effect.Honeyed)) {
            affectedBug.effects.Add(Effect.Honeyed);
        }
        affectedBug.multiplier *= 2.5f;
    }
    
    public override void Reset()
    {
        base.Reset();
        this.cachedAffectedBug = null;
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
