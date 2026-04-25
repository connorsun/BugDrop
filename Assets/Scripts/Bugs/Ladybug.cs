using UnityEngine;
using System.Threading.Tasks;


public class Ladybug : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Ladybug", 1, 1, 1.25f, 0.5f, "+1 additional point for every 3 cm away from the lightning rod");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }

    public override float CalculateOverallScore()
    {
        return (this.baseScore + (int)(((Vector2)this.center.position - (Vector2)GameHandler.ZapperPos).magnitude / 3f)) * this.multiplier;
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
