using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

public class Aphid : Bug
{
    // --- CONSTANTS ---
    private const float DETECTION_RADIUS = 3f;
    private const int MAX_TRIGGER = 2;
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Aphid", 2, 0, 3f, 0.5f, "Retriggers two closest bugs within 3 cm");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }
    
    public override int CalculateOverallScore()
    {
        return this.baseScore;
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {

        ScorePoints(CalculateOverallScore(), isPrimary);

        // Find bugs to retrigger
        List<Collider2D> overlapColliders = new List<Collider2D>();
        Physics2D.OverlapCircle(transform.position, DETECTION_RADIUS, ContactFilter2D.noFilter, overlapColliders);
        List<Collider2D> filteredBugs = overlapColliders.Where(bug => (transform.position - bug.transform.position).magnitude < DETECTION_RADIUS && 
            bug.gameObject?.GetComponentInParent<Bug>() != null).ToList();
        filteredBugs.Sort((Collider2D bug1, Collider2D bug2) => (int)Mathf.Sign((transform.position - bug1.transform.position).magnitude - (transform.position - bug2.transform.position).magnitude));

        // Retrigger logic
        List<Task> bugTasksToTrigger = new List<Task>();
        List<Bug> bugsToTrigger = new List<Bug>();
        int i = 0;
        foreach (Collider2D bugCol in filteredBugs)
        {
            Bug otherBug = bugCol.gameObject?.GetComponentInParent<Bug>();
            if (otherBug != null && !bugsToTrigger.Contains(otherBug) && !otherBug.secondaryTriggered)
            {
                bugsToTrigger.Add(otherBug);
                bugTasksToTrigger.Add(otherBug.Trigger(false, this.center.position, recursiveSecondaries + 1));
                i++;
            }
            if (i >= MAX_TRIGGER)
            {
                break;
            }
        }
        await Task.WhenAll(bugTasksToTrigger);
    }
}
