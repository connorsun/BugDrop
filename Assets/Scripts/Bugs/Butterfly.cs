using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;


public class Butterfly : Bug
{
    // --- CONSTANTS ---
    private const float DETECTION_RADIUS = 4f;
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Butterfly", 3, 0, 0.75f, 0.5f, "Sums the points of all bugs in 4 cm if no other Butterflies are in radius");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }

    public override Bug[] GetAffectedBugs()
    {   
        // Find bugs in radius
        List<Collider2D> overlapColliders = new List<Collider2D>();
        Physics2D.OverlapCircle(this.center.position, DETECTION_RADIUS, ContactFilter2D.noFilter, overlapColliders);
        List<Collider2D> filteredBugs = overlapColliders.Where(bug => bug.gameObject?.GetComponentInParent<Bug>() != null/* && (this.center.position - bug.gameObject.GetComponentInParent<Bug>().center.position).magnitude < DETECTION_RADIUS*/
            ).ToList();
        float totalScore = this.baseScore;
        HashSet<Bug> bugsScored = new HashSet<Bug>();
        foreach (Collider2D bugCol in filteredBugs)
        {
            Bug otherBug = bugCol?.gameObject?.GetComponentInParent<Bug>();
            if (otherBug != null && otherBug != this && !bugsScored.Contains(otherBug))
            {
                if (otherBug is Butterfly)
                {
                    // check for asymmetry
                    if ((otherBug.center.position - this.center.position).magnitude <= DETECTION_RADIUS) {
                        return new Bug[0]{};
                    }
                } else {
                    bugsScored.Add(otherBug);
                }
            }
        }
        return bugsScored.ToArray();
    }
    
    public override float CalculateOverallScore()
    {
        float totalScore = this.baseScore;
        foreach (Bug bug in GetAffectedBugs())
        {
            totalScore += bug.CalculateOverallScore();
        }
        return totalScore * this.multiplier;
    }

    public override async Task Hover(bool on, float intensity, bool affectOthers)
    {
        base.Hover(on, intensity, affectOthers);
        if (affectOthers) {
            GameHandler.SingletonCircleIndicator.GetComponent<SpriteRenderer>().enabled = on;
            if (on) {
                GameHandler.SingletonCircleIndicator.transform.position = center.position;GameHandler.SingletonCircleIndicator.transform.localScale = new Vector3(2f, 2f, 1f);
            }
        }
    }

    public override void Destroy()
    {
        GameHandler.SingletonCircleIndicator.GetComponent<SpriteRenderer>().enabled = false;
        base.Destroy();
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
