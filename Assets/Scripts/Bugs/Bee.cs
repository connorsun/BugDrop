using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;


public class Bee : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Bee", 2, 3, 1.5f, 0.5f, "Honeys bug directly below, giving it x2 score");
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
    }

    public override void StartScoring()
    {
        base.StartScoring();
        List<RaycastHit2D> rayHits = new List<RaycastHit2D>();
        Physics2D.Raycast(this.center.position, Vector2.down, ContactFilter2D.noFilter, rayHits);
        foreach (RaycastHit2D rayHit in rayHits)
        {
            Bug otherBug = rayHit.collider?.gameObject?.GetComponentInParent<Bug>();
            //print(otherBug);
            if (otherBug != null && otherBug != this)
            {
                if (!otherBug.effects.Contains(Effect.Honeyed)) {
                    otherBug.effects.Add(Effect.Honeyed);
                    otherBug.multiplier *= 2;
                }
            }
            if (otherBug != this)
            {
                break;
            }
        }
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
