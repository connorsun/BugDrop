using UnityEngine;
using System.Threading.Tasks;


public class Worm : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Worm", 2, 1, 4.5f);
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
        print(contacts.Length);
        foreach (ContactPoint2D contact in contacts)
        {
            print(contact.collider?.gameObject);
            Bug otherBug = contact.collider?.gameObject?.GetComponent<Bug>();
            print(otherBug);
            if (otherBug != null)
            {
                otherBug.Trigger(false);
                return;
            }
        }
    }
}
