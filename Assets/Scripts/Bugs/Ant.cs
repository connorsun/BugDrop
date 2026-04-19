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
        return new BugInfo("Ant", 1, 1, 1.5f, 0.5f, "+2 if touching ground");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        base.Start();
    }

    protected override async Task Score(bool isPrimary)
    {
        ContactPoint2D[] contacts = this.GetContacts();
        foreach (ContactPoint2D contact in contacts)
        {
            if (contact.collider?.gameObject.CompareTag("Ground") == true)
            {
                ScorePoints(this.baseScore + 2, isPrimary);
                return;
            }
        }
        ScorePoints(this.baseScore, isPrimary);
    }
}
