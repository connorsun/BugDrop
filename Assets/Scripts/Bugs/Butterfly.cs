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
        return new BugInfo("Butterfly", 3, 0, 3f, 0.5f, "Scores 50% of points of all bugs in 4 cm if no other Butterflies are in radius");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }
    
    public override float CalculateOverallScore()
    {
        // Find bugs in radius
        List<Collider2D> overlapColliders = new List<Collider2D>();
        Physics2D.OverlapCircle(this.center.position, DETECTION_RADIUS, ContactFilter2D.noFilter, overlapColliders);
        List<Collider2D> filteredBugs = overlapColliders.Where(bug => bug.gameObject?.GetComponentInParent<Bug>() != null/* && (this.center.position - bug.gameObject.GetComponentInParent<Bug>().center.position).magnitude < DETECTION_RADIUS*/
            ).ToList();
        float totalScore = this.baseScore;
        List<Bug> bugsScored = new List<Bug>();
        print(filteredBugs.Count);
        foreach (Collider2D bugCol in filteredBugs)
        {
            Bug otherBug = bugCol?.gameObject?.GetComponentInParent<Bug>();
            if (otherBug != null && otherBug != this && !bugsScored.Contains(otherBug))
            {
                if (otherBug is Butterfly)
                {
                    print(otherBug);
                    totalScore = this.baseScore;
                    break;
                }
                bugsScored.Add(otherBug);
                totalScore += otherBug.CalculateOverallScore() / 2f;
            }
        }
        return totalScore * this.multiplier;
    }

    public override async Task Hover(bool on, float intensity, bool affectOthers)
    {
        base.Hover(on, intensity, affectOthers);
        GameHandler.SingletonCircleIndicator.GetComponent<SpriteRenderer>().enabled = on;
        if (on) {
            GameHandler.SingletonCircleIndicator.transform.position = center.position;GameHandler.SingletonCircleIndicator.transform.localScale = new Vector3(DETECTION_RADIUS, DETECTION_RADIUS, 1f);
        }
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
