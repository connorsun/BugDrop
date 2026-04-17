using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;


public class Worm : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Worm", 2, 0, 4.5f);
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
        float timestamp = Time.unscaledTime;
        while (Time.unscaledTime < timestamp + 0.3f)
        {
            await Task.Yield();
        }
        ContactPoint2D[] contacts = this.GetContacts();
        print(contacts.Length);
        List<Task> bugsToTrigger = new List<Task>();
        foreach (ContactPoint2D contact in contacts)
        {
            print(contact.collider?.gameObject);
            Bug otherBug = contact.collider?.gameObject?.GetComponent<Bug>();
            print(otherBug);
            if (otherBug != null && !otherBug.secondaryTriggered)
            {
                bugsToTrigger.Add(otherBug.Trigger(false, transform.position));
            }
        }
        await Task.WhenAll(bugsToTrigger);
    }
}
