using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;


public class Centipede : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Centipede", 4, 0, 12f, 0.5f, "Retriggers all adjacent bugs");
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
        // ContactPoint2D[] contacts = this.GetContacts();
        // int totalScore = this.baseScore;
        // foreach (ContactPoint2D contact in contacts)
        // {
        //     Bug otherBug = contact.collider?.gameObject?.GetComponentInParent<Bug>();
        //     if (otherBug != null)
        //     {
        //         totalScore += otherBug.baseScore;
        //     }
        // }
        // return totalScore;
    }

    public override Bug[] GetAffectedBugs()
    {
        ContactPoint2D[] contacts = this.GetContacts();
        HashSet<Bug> bugsToTrigger = new HashSet<Bug>();
        foreach (ContactPoint2D contact in contacts)
        {
            Bug otherBug = contact.collider?.gameObject?.GetComponentInParent<Bug>();
            if (otherBug != null && !otherBug.secondaryTriggered)
            {
                bugsToTrigger.Add(otherBug);
            }
        }
        return bugsToTrigger.ToArray();
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {

        ScorePoints(CalculateOverallScore(), isPrimary);

        // Retrigger logic
        List<Task> bugTasksToTrigger = new List<Task>();
        foreach (Bug bug in GetAffectedBugs())
        {
            bugTasksToTrigger.Add(bug.Trigger(false, this.center.position, recursiveSecondaries + 1));
        }
        await Task.WhenAll(bugTasksToTrigger);
    }
}
