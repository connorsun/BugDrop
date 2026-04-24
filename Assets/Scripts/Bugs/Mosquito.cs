using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;


public class Mosquito : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 
    private Bug[] cachedAffectedBugs;

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Mosquito", 1, 1, 0.6f, 0.5f, "-1 point from adjacent bugs' score, but +3 additional points to self for each adjacent bug");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }

    public override Bug[] GetAffectedBugs()
    {
        if (this.cachedAffectedBugs != null)
        {
            return this.cachedAffectedBugs;
        } else
        {
            ContactPoint2D[] contacts = this.GetContacts();
            //print("reducing contacts" + contacts.Length);
            HashSet<Bug> allBugs = new HashSet<Bug>();
            foreach (ContactPoint2D contact in contacts)
            {
                Bug otherBug = contact.collider?.gameObject?.GetComponentInParent<Bug>();
                if (otherBug != null) {
                    allBugs.Add(otherBug);
                }
            }
            return allBugs.ToArray();
        }
    }
    
    public override void Reset()
    {
        base.Reset();
        this.cachedAffectedBugs = null;
    }

    public override float CalculateOverallScore()
    {
        return this.baseScore * this.multiplier;
    }

    public override void StartScoring()
    {
        base.StartScoring();
            //print(otherBug);
        Bug[] allBugs = GetAffectedBugs();
        foreach (Bug bug in allBugs)
        {
            bug.baseScore--;
            this.baseScore += 3;
        }
        this.cachedAffectedBugs = allBugs;
        // _ = ReduceAdjacentBugScore();
    }

    // private async Task ReduceAdjacentBugScore()
    // {
    //     await Task.Yield();
    // }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
