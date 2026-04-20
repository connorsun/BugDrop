using UnityEngine;
using System.Threading.Tasks;


public class Fly : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Fly", 1, 1, 1.5f, 0.5f, "+2 if not touching ground");
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
        foreach (ContactPoint2D contact in contacts)
        {
            if (contact.collider?.gameObject.CompareTag("Ground") == true)
            {
                return this.baseScore;
            }
        }
        return this.baseScore + 2;
    }

    protected override async Task Score(bool isPrimary)
    {
        // if not touching ground
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
