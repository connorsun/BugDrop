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
        return new BugInfo("Worm", 2, 0, 3f, 0.5f, "Totals the base score of all adjacent bugs");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }
    
    public override int CalculateOverallScore()
    {
        ContactPoint2D[] contacts = this.GetContacts();
        int totalScore = this.baseScore;
        foreach (ContactPoint2D contact in contacts)
        {
            Bug otherBug = contact.collider?.gameObject?.GetComponentInParent<Bug>();
            if (otherBug != null)
            {
                totalScore += otherBug.baseScore;
            }
        }
        return totalScore;
    }

    protected override async Task Score(bool isPrimary)
    {

        ScorePoints(CalculateOverallScore(), isPrimary);

        // [OLD] retrigger logic. use for future retriggering bugs
        // ContactPoint2D[] contacts = this.GetContacts();
        // List<Task> bugsToTrigger = new List<Task>();
        // foreach (ContactPoint2D contact in contacts)
        // {
        //     Bug otherBug = contact.collider?.gameObject?.GetComponentInParent<Bug>();
        //     if (otherBug != null && !otherBug.secondaryTriggered)
        //     {
        //         bugsToTrigger.Add(otherBug.Trigger(false, this.center.position));
        //     }
        // }
        // await Task.WhenAll(bugsToTrigger);
    }
}
