using UnityEngine;
using System.Threading.Tasks;
using System.Linq;


public class Termite : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Termite", 2, 0, 1.5f, 0.5f, "1 point for each Termite in tank (max 15 points each)");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }

    public override Bug[] GetAffectedBugs()
    {
        Termite[] termites = FindObjectsByType<Termite>(FindObjectsSortMode.None);
        return termites.Where(bug => !bug.Equals(this)).ToArray();
    }

    public override float CalculateOverallScore()
    {
        Termite[] termites = FindObjectsByType<Termite>(FindObjectsSortMode.None);
        return (this.baseScore + Mathf.Min(termites.Length, 15)) * this.multiplier;
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
