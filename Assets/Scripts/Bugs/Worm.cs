using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;


public class Worm : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Worm", 2, 0, 3f, 0.5f, "Retriggers all touching bugs");
    }

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        base.Start();
        this.thisBugInfo = GetInfo();
    }

    protected override async Task Score()
    {
        float timestamp = Time.unscaledTime;
        await Task.Delay(TimeSpan.FromSeconds(0.2f * 1/GameHandler.GameSpeed));
        ContactPoint2D[] contacts = this.GetContacts();
        //print(contacts.Length);
        List<Task> bugsToTrigger = new List<Task>();
        foreach (ContactPoint2D contact in contacts)
        {
            Bug otherBug = contact.collider?.gameObject?.GetComponentInParent<Bug>();

            if (otherBug != null && !otherBug.secondaryTriggered)
            {
                bugsToTrigger.Add(otherBug.Trigger(false, this.center.position));
            }
        }
        await Task.WhenAll(bugsToTrigger);
    }
}
