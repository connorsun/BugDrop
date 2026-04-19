using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;


public class Cockroach : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Cockroach", 1, 2, 3f, 0.5f, "+2 for each adjacent Cockroach");
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
            Bug otherCockroach = contact.collider?.gameObject?.GetComponentInParent<Cockroach>();
            if (otherCockroach != null)
            {
                totalScore += 2;
            }
        }
        return totalScore;
    }

    protected override async Task Score(bool isPrimary)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
