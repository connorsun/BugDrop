using UnityEngine;
using System.Threading.Tasks;


public class Mosquito : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Mosquito", 1, 1, 1.5f, 0.5f, "-1 base from adjacent bugs, +2 base to self for each adjacent bug");
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

    public override void StartScoring()
    {
        base.StartScoring();
        ContactPoint2D[] contacts = this.GetContacts();
        print("reducing contacts" + contacts.Length);
        foreach (ContactPoint2D contact in contacts)
        {
            Bug otherBug = contact.collider?.gameObject?.GetComponentInParent<Bug>();
            print(otherBug);
            if (otherBug != null)
            {
                otherBug.baseScore--;
                this.baseScore += 2;
            }
        }
        // _ = ReduceAdjacentBugScore();
    }

    // private async Task ReduceAdjacentBugScore()
    // {
    //     await Task.Yield();
    // }

    protected override async Task Score(bool isPrimary)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
