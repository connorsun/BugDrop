using UnityEngine;
using System.Threading.Tasks;


public class Termite : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Termite", 2, 1, 1.5f, 0.5f, "+1 additional points for each Termite in tank, up to +7");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }

    public override float CalculateOverallScore()
    {
        Termite[] termites = FindObjectsByType<Termite>();
        return (this.baseScore + (termites.Length - 1)) * this.multiplier;
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
