using UnityEngine;
using System.Threading.Tasks;


public class Ant : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Ant", 1, 1, 1.5f, 0.5f, "+4 if touching ground");
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
                return this.baseScore + 4;
            }
        }
        return this.baseScore;
    }

    protected override async Task Score(bool isPrimary)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
