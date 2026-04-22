using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;


public class Worm : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Worm", 2, 0, 3f, 0.5f, "Retriggers all adjacent bugs");
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

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {

        ScorePoints(CalculateOverallScore(), isPrimary);

        // Retrigger logic
        ContactPoint2D[] contacts = this.GetContacts();
        List<Task> bugsToTrigger = new List<Task>();
        foreach (ContactPoint2D contact in contacts)
        {
            Bug otherBug = contact.collider?.gameObject?.GetComponentInParent<Bug>();
            if (otherBug != null && !otherBug.secondaryTriggered)
            {
                bugsToTrigger.Add(otherBug.Trigger(false, this.center.position, recursiveSecondaries + 1));
            }
        }
        await Task.WhenAll(bugsToTrigger);
    }
}
