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
        return new BugInfo("Fly", 1, 1, 1.5f, 0.5f);
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        base.Start();
        this.thisBugInfo = GetInfo();
    }

    protected override async Task Score()
    {
        base.Score();
        ContactPoint2D[] contacts = this.GetContacts();
        foreach (ContactPoint2D contact in contacts)
        {
            if (contact.collider?.gameObject.CompareTag("Ground") == true)
            {
                return;
            }
        }
        // if not touching ground
        ScorePoints(3);
    }
}
