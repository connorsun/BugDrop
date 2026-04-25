using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;


public class Cockroach : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Cockroach", 1, 2, 1.6f, 0.5f, "+2 additional points for each adjacent Cockroach");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }

    public override Bug[] GetAffectedBugs()
    {
        ContactPoint2D[] contacts = this.GetContacts();
        HashSet<Bug> allBugs = new HashSet<Bug>();
        foreach (ContactPoint2D contact in contacts)
        {
            Bug otherCockroach = contact.collider?.gameObject?.GetComponentInParent<Cockroach>();
            if (otherCockroach != null)
            {
                allBugs.Add(otherCockroach);
            }
        }
        return allBugs.ToArray();
    }

    public override float CalculateOverallScore()
    {
        Bug[] allBugs = GetAffectedBugs();
        float totalScore = this.baseScore + 2f * allBugs.Length;
        return totalScore * this.multiplier;
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
